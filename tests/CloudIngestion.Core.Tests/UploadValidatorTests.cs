using CloudIngestion.Core.Services;
using Xunit;

namespace CloudIngestion.Core.Tests;

public class UploadValidatorTests
{
    private readonly UploadValidator _validator = new();

    [Theory]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("document.pdf", "application/pdf")]
    [InlineData("video.mp4", "video/mp4")]
    [InlineData("model.stl", "model/stl")]
    public void Validate_AcceptsSupportedFileTypes(string fileName, string contentType)
    {
        var result = _validator.Validate(fileName, contentType, 1024);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("script.exe", "application/octet-stream")]
    [InlineData("archive.zip", "application/zip")]
    [InlineData("page.html", "text/html")]
    [InlineData("data.csv", "text/csv")]
    public void Validate_RejectsUnsupportedFileTypes(string fileName, string contentType)
    {
        var result = _validator.Validate(fileName, contentType, 1024);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Validate_RejectsEmptyFile()
    {
        var result = _validator.Validate("file.pdf", "application/pdf", 0);
        Assert.False(result.IsValid);
        Assert.Contains("empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsFileThatExceedsMaxSize()
    {
        _validator.MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        var result = _validator.Validate("large.pdf", "application/pdf", 20 * 1024 * 1024);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Validate_AcceptsFileAtExactMaxSize()
    {
        _validator.MaxFileSizeBytes = 10 * 1024 * 1024;
        var result = _validator.Validate("file.pdf", "application/pdf", 10 * 1024 * 1024);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_IsCaseInsensitiveForExtension()
    {
        var result = _validator.Validate("PHOTO.PNG", "image/png", 1024);
        Assert.True(result.IsValid);
    }
}
