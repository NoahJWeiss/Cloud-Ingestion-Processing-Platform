namespace CloudIngestion.Core.Services;

public class UploadValidator
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".pdf", ".mp4", ".stl"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "application/pdf",
        "video/mp4", "model/stl", "application/sla",
        "application/octet-stream"
    };

    public long MaxFileSizeBytes { get; set; } = 512 * 1024 * 1024; // 512 MB default

    public ValidationResult Validate(string fileName, string contentType, long sizeBytes)
    {
        if (sizeBytes <= 0)
            return ValidationResult.Fail("File is empty.");

        if (sizeBytes > MaxFileSizeBytes)
            return ValidationResult.Fail($"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return ValidationResult.Fail($"File type '{extension}' is not supported. Allowed types: {string.Join(", ", AllowedExtensions)}.");

        return ValidationResult.Ok();
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Ok() => new(true, null);
    public static ValidationResult Fail(string errorMessage) => new(false, errorMessage);
}
