using CloudIngestion.Core.Enums;
using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using CloudIngestion.Infrastructure.Data;
using CloudIngestion.Infrastructure.Repositories;
using CloudIngestion.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CloudIngestion.Worker.Tests;

public class FileProcessingWorkerTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IServiceProvider BuildServiceProvider(AppDbContext db, IObjectStorage storage)
    {
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddScoped<IFileRepository>(_ => new FileRepository(db));
        services.AddScoped(_ => storage);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Worker_TransitionsFileFromPendingToCompleted()
    {
        using var db = CreateInMemoryDb();
        var storageMock = new Mock<IObjectStorage>();
        storageMock
            .Setup(s => s.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1, 2, 3]));

        var record = new FileRecord
        {
            OriginalFileName = "test.pdf",
            ContentType = "application/pdf",
            ObjectKey = "some/key",
            SizeBytes = 100,
            Status = ProcessingStatus.Pending
        };
        db.FileRecords.Add(record);
        await db.SaveChangesAsync();

        var sp = BuildServiceProvider(db, storageMock.Object);
        var worker = new FileProcessingWorker(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<FileProcessingWorker>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1.5), CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        var updated = await db.FileRecords.FindAsync(record.Id);
        Assert.Equal(ProcessingStatus.Completed, updated!.Status);
        Assert.NotNull(updated.MetadataJson);
        Assert.NotNull(updated.ProcessedAt);
    }

    [Fact]
    public async Task Worker_MarksFileAsFailedWhenStorageThrows()
    {
        using var db = CreateInMemoryDb();
        var storageMock = new Mock<IObjectStorage>();
        storageMock
            .Setup(s => s.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Not found"));

        var record = new FileRecord
        {
            OriginalFileName = "missing.pdf",
            ContentType = "application/pdf",
            ObjectKey = "missing/key",
            SizeBytes = 100,
            Status = ProcessingStatus.Pending
        };
        db.FileRecords.Add(record);
        await db.SaveChangesAsync();

        var sp = BuildServiceProvider(db, storageMock.Object);
        var worker = new FileProcessingWorker(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<FileProcessingWorker>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1.5), CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        var updated = await db.FileRecords.FindAsync(record.Id);
        Assert.Equal(ProcessingStatus.Failed, updated!.Status);
        Assert.NotNull(updated.FailureReason);
    }

    [Fact]
    public async Task Worker_ContinuesProcessingAfterOneFails()
    {
        using var db = CreateInMemoryDb();
        var storageMock = new Mock<IObjectStorage>();

        var failRecord = new FileRecord
        {
            OriginalFileName = "fail.pdf",
            ContentType = "application/pdf",
            ObjectKey = "fail/key",
            SizeBytes = 100,
            Status = ProcessingStatus.Pending,
            UploadedAt = DateTime.UtcNow.AddMinutes(-2)
        };
        var successRecord = new FileRecord
        {
            OriginalFileName = "success.pdf",
            ContentType = "application/pdf",
            ObjectKey = "success/key",
            SizeBytes = 200,
            Status = ProcessingStatus.Pending,
            UploadedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        db.FileRecords.AddRange(failRecord, successRecord);
        await db.SaveChangesAsync();

        storageMock
            .Setup(s => s.OpenReadAsync("fail/key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));
        storageMock
            .Setup(s => s.OpenReadAsync("success/key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream([1, 2, 3]));

        var sp = BuildServiceProvider(db, storageMock.Object);
        var worker = new FileProcessingWorker(
            sp.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<FileProcessingWorker>.Instance);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1.5), CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        var updatedFail = await db.FileRecords.FindAsync(failRecord.Id);
        var updatedSuccess = await db.FileRecords.FindAsync(successRecord.Id);

        Assert.Equal(ProcessingStatus.Failed, updatedFail!.Status);
        Assert.Equal(ProcessingStatus.Completed, updatedSuccess!.Status);
    }

    [Fact]
    public async Task FileRepository_ReturnsPendingRecords()
    {
        using var db = CreateInMemoryDb();
        db.FileRecords.AddRange(
            new FileRecord { Status = ProcessingStatus.Pending, OriginalFileName = "a.pdf", ContentType = "application/pdf", ObjectKey = "a" },
            new FileRecord { Status = ProcessingStatus.Completed, OriginalFileName = "b.pdf", ContentType = "application/pdf", ObjectKey = "b" },
            new FileRecord { Status = ProcessingStatus.Failed, OriginalFileName = "c.pdf", ContentType = "application/pdf", ObjectKey = "c" }
        );
        await db.SaveChangesAsync();

        var repo = new FileRepository(db);
        var pending = await repo.GetPendingAsync();

        Assert.Single(pending);
        Assert.Equal("a.pdf", pending[0].OriginalFileName);
    }
}
