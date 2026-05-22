using CloudIngestion.Core.Models;

namespace CloudIngestion.Core.Interfaces;

public interface IProcessingQueue
{
    Task PublishAsync(FileProcessingJob job, CancellationToken cancellationToken = default);
}
