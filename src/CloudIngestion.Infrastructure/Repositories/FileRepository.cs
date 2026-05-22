using CloudIngestion.Core.Enums;
using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using CloudIngestion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudIngestion.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _db;

    public FileRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FileRecord> AddAsync(FileRecord record, CancellationToken cancellationToken = default)
    {
        _db.FileRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.FileRecords.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<FileRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FileRecords
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileRecord>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FileRecords
            .Where(f => f.Status == ProcessingStatus.Pending)
            .OrderBy(f => f.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(FileRecord record, CancellationToken cancellationToken = default)
    {
        _db.FileRecords.Update(record);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
