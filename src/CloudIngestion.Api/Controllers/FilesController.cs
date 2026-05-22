using CloudIngestion.Core.DTOs;
using CloudIngestion.Core.Enums;
using CloudIngestion.Core.Interfaces;
using CloudIngestion.Core.Models;
using CloudIngestion.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudIngestion.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileRepository _repository;
    private readonly IObjectStorage _storage;
    private readonly IProcessingQueue _queue;
    private readonly UploadValidator _validator;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileRepository repository,
        IObjectStorage storage,
        IProcessingQueue queue,
        UploadValidator validator,
        ILogger<FilesController> logger)
    {
        _repository = repository;
        _storage = storage;
        _queue = queue;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(536_870_912)] // 512 MB
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        var validation = _validator.Validate(file.FileName, file.ContentType, file.Length);
        if (!validation.IsValid)
            return BadRequest(new { error = validation.ErrorMessage });

        string objectKey;
        await using (var stream = file.OpenReadStream())
        {
            objectKey = await _storage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
        }

        var record = new FileRecord
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            ObjectKey = objectKey,
            SizeBytes = file.Length,
            Status = ProcessingStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(record, cancellationToken);

        await _queue.PublishAsync(new FileProcessingJob
        {
            FileRecordId = record.Id,
            ObjectKey = record.ObjectKey,
            ContentType = record.ContentType,
            OriginalFileName = record.OriginalFileName
        }, cancellationToken);

        _logger.LogInformation("File uploaded: {FileId} ({FileName})", record.Id, record.OriginalFileName);

        return Ok(new UploadResultDto
        {
            FileId = record.Id,
            Status = record.Status.ToString(),
            OriginalFileName = record.OriginalFileName,
            SizeBytes = record.SizeBytes
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllAsync(cancellationToken);
        return Ok(records.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(id, cancellationToken);
        if (record is null)
            return NotFound(new { error = $"File {id} not found." });

        return Ok(ToDto(record));
    }

    private static FileRecordDto ToDto(FileRecord r) => new()
    {
        Id = r.Id,
        OriginalFileName = r.OriginalFileName,
        ContentType = r.ContentType,
        SizeBytes = r.SizeBytes,
        Status = r.Status.ToString(),
        UploadedAt = r.UploadedAt,
        ProcessedAt = r.ProcessedAt,
        MetadataJson = r.MetadataJson,
        FailureReason = r.FailureReason
    };
}
