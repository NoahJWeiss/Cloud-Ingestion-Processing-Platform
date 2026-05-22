using CloudIngestion.Core.Enums;

namespace CloudIngestion.Core.Models;

public class FileRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? MetadataJson { get; set; }
    public string? FailureReason { get; set; }
}
