using System.Text.Json;
using CloudIngestion.Core.Enums;
using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudIngestion.Worker;

public class FileProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FileProcessingWorker> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    public FileProcessingWorker(IServiceScopeFactory scopeFactory, ILogger<FileProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File processing worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingFilesAsync(stoppingToken);
            await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("File processing worker stopped");
    }

    private async Task ProcessPendingFilesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
        var storage = scope.ServiceProvider.GetRequiredService<IObjectStorage>();

        IReadOnlyList<FileRecord> pending;
        try
        {
            pending = await repository.GetPendingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query pending file records");
            return;
        }

        foreach (var record in pending)
        {
            if (stoppingToken.IsCancellationRequested) break;
            await ProcessFileAsync(record, repository, storage, stoppingToken);
        }
    }

    private async Task ProcessFileAsync(
        FileRecord record,
        IFileRepository repository,
        IObjectStorage storage,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing file {FileId} ({FileName})", record.Id, record.OriginalFileName);

        record.Status = ProcessingStatus.Processing;
        await repository.UpdateAsync(record, stoppingToken);

        try
        {
            await using var stream = await storage.OpenReadAsync(record.ObjectKey, stoppingToken);

            var metadata = new
            {
                originalFileName = record.OriginalFileName,
                contentType = record.ContentType,
                sizeBytes = record.SizeBytes,
                processedAt = DateTime.UtcNow
            };

            record.MetadataJson = JsonSerializer.Serialize(metadata);
            record.ProcessedAt = DateTime.UtcNow;
            record.Status = ProcessingStatus.Completed;

            await repository.UpdateAsync(record, stoppingToken);

            _logger.LogInformation("File {FileId} processed successfully", record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process file {FileId}", record.Id);

            record.Status = ProcessingStatus.Failed;
            record.FailureReason = ex.Message;

            try
            {
                await repository.UpdateAsync(record, stoppingToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update failure status for file {FileId}", record.Id);
            }
        }
    }
}
