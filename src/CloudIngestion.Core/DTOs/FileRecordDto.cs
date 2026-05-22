using CloudIngestion.Core.Enums;

namespace CloudIngestion.Core.DTOs;

public class FileRecordDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? MetadataJson { get; set; }
    public string? FailureReason { get; set; }
}
