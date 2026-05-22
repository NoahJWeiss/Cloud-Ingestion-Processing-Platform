namespace CloudIngestion.Core.Models;

public class FileProcessingJob
{
    public Guid FileRecordId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
}
