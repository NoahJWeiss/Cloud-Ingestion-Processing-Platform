using CloudIngestion.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CloudIngestion.Infrastructure.Storage;

public class LocalObjectStorageOptions
{
    public string StorageRoot { get; set; } = "uploads";
}

public class LocalObjectStorage : IObjectStorage
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalObjectStorage> _logger;

    public LocalObjectStorage(LocalObjectStorageOptions options, ILogger<LocalObjectStorage> logger)
    {
        _storageRoot = options.StorageRoot;
        _logger = logger;
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var objectKey = $"{Guid.NewGuid()}/{SanitizeFileName(fileName)}";
        var fullPath = Path.Combine(_storageRoot, objectKey);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Saved object {ObjectKey} to local storage", objectKey);
        return objectKey;
    }

    public Task<Stream> OpenReadAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storageRoot, objectKey);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Object '{objectKey}' not found in local storage.", fullPath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
        return sanitized.Length > 200 ? sanitized[..200] : sanitized;
    }
}
