using CloudIngestion.Core.DTOs;
using CloudIngestion.Core.Enums;
using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using CloudIngestion.Core.Services;
using CloudIngestion.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CloudIngestion.Api.Tests;

public class FilesControllerTests
{
    private readonly Mock<IFileRepository> _repoMock = new();
    private readonly Mock<IObjectStorage> _storageMock = new();
    private readonly Mock<IProcessingQueue> _queueMock = new();
    private readonly UploadValidator _validator = new();
    private readonly FilesController _controller;

    public FilesControllerTests()
    {
        _controller = new FilesController(
            _repoMock.Object,
            _storageMock.Object,
            _queueMock.Object,
            _validator,
            NullLogger<FilesController>.Instance);
    }

    [Fact]
    public async Task Upload_ReturnsOk_WhenFileIsValid()
    {
        var file = CreateFormFile("test.pdf", "application/pdf", 1024);

        _storageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("some/object-key");

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<FileRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord r, CancellationToken _) => r);

        var result = await _controller.Upload(file, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UploadResultDto>(ok.Value);
        Assert.Equal("Pending", dto.Status);
        Assert.Equal("test.pdf", dto.OriginalFileName);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileTypeNotSupported()
    {
        var file = CreateFormFile("malware.exe", "application/octet-stream", 1024);

        var result = await _controller.Upload(file, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _storageMock.Verify(s => s.SaveAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileIsEmpty()
    {
        var file = CreateFormFile("empty.pdf", "application/pdf", 0);

        var result = await _controller.Upload(file, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_CreatesFileRecord_WithPendingStatus()
    {
        var file = CreateFormFile("test.png", "image/png", 2048);
        FileRecord? capturedRecord = null;

        _storageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("obj/key");

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<FileRecord>(), It.IsAny<CancellationToken>()))
            .Callback<FileRecord, CancellationToken>((r, _) => capturedRecord = r)
            .ReturnsAsync((FileRecord r, CancellationToken _) => r);

        await _controller.Upload(file, CancellationToken.None);

        Assert.NotNull(capturedRecord);
        Assert.Equal(ProcessingStatus.Pending, capturedRecord.Status);
        Assert.Equal("test.png", capturedRecord.OriginalFileName);
    }

    [Fact]
    public async Task Upload_PublishesProcessingJob()
    {
        var file = CreateFormFile("test.jpg", "image/jpeg", 512);

        _storageMock
            .Setup(s => s.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("obj/key");

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<FileRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord r, CancellationToken _) => r);

        await _controller.Upload(file, CancellationToken.None);

        _queueMock.Verify(q => q.PublishAsync(
            It.Is<FileProcessingJob>(j => j.ContentType == "image/jpeg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfFiles()
    {
        var records = new List<FileRecord>
        {
            new() { Id = Guid.NewGuid(), OriginalFileName = "a.pdf", Status = ProcessingStatus.Completed },
            new() { Id = Guid.NewGuid(), OriginalFileName = "b.png", Status = ProcessingStatus.Pending }
        };

        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var result = await _controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<FileRecordDto>>(ok.Value);
        Assert.Equal(2, dtos.Count());
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRecordDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileRecord?)null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, long size)
    {
        var content = new byte[Math.Max(size, 0)];
        var stream = new MemoryStream(content);
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(size);
        mock.Setup(f => f.OpenReadStream()).Returns(stream);
        return mock.Object;
    }
}
