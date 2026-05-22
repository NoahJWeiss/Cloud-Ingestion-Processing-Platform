namespace CloudIngestion.Core.DTOs;

public class UploadResultDto
{
    public Guid FileId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
