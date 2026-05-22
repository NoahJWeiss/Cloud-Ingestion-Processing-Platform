using CloudIngestion.Core.Models;

namespace CloudIngestion.Core.Interfaces;

public interface IFileRepository
{
    Task<FileRecord> AddAsync(FileRecord record, CancellationToken cancellationToken = default);
    Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecord>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(FileRecord record, CancellationToken cancellationToken = default);
}
