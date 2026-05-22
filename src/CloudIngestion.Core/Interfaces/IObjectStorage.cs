namespace CloudIngestion.Core.Interfaces;

public interface IObjectStorage
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}
