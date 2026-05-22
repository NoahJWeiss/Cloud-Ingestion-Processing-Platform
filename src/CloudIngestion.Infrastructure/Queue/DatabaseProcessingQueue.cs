using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using Microsoft.Extensions.Logging;

namespace CloudIngestion.Infrastructure.Queue;

/// <summary>
/// Database-backed processing queue. Publishing a job is a no-op because the
/// FileRecord with Status=Pending already acts as the queue entry. The worker
/// polls pending records directly from the database.
/// Future: Replace with SqsProcessingQueue without changing controller code.
/// </summary>
public class DatabaseProcessingQueue : IProcessingQueue
{
    private readonly ILogger<DatabaseProcessingQueue> _logger;

    public DatabaseProcessingQueue(ILogger<DatabaseProcessingQueue> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(FileProcessingJob job, CancellationToken cancellationToken = default)
    {
        // The FileRecord with Status=Pending is the queue entry.
        // The worker polls the database for pending records, so no further action is needed here.
        _logger.LogDebug("File {FileRecordId} queued for processing via database pending-job mechanism", job.FileRecordId);
        return Task.CompletedTask;
    }
}
