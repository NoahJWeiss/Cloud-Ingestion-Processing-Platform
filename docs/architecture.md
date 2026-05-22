# Architecture

## Overview

The Cloud Ingestion & Processing Platform is a backend-first system for uploading, storing, and asynchronously processing large file artifacts. It is structured as a set of cooperating services connected through a PostgreSQL database and a shared local file store.

```
React Frontend (Vite/TypeScript)
        |
        v (HTTP REST)
ASP.NET Core API  ──── PostgreSQL (FileRecords)
        |                    |
        | (writes file)      | (polls Pending rows)
        v                    v
  Local Filesystem     .NET Worker Service
  (shared volume)           |
        ^                   | (reads file via IObjectStorage)
        |                   v
        └────────── Updates FileRecord status
```

---

## Services

### API Service (`CloudIngestion.Api`)

- ASP.NET Core Web API (net10.0)
- Accepts multipart file uploads via `POST /api/files/upload`
- Validates file type, extension, and size before storing
- Writes the raw file to object storage via `IObjectStorage`
- Creates a `FileRecord` with `Status = Pending` in PostgreSQL
- Publishes a `FileProcessingJob` via `IProcessingQueue`
- Exposes `GET /api/files` and `GET /api/files/{id}` for status queries
- Exposes `GET /api/health` for health checks
- In Development mode, automatically applies EF Core migrations on startup
- CORS configured for the frontend origin

### Worker Service (`CloudIngestion.Worker`)

- .NET Worker Service (net10.0) with `FileProcessingWorker : BackgroundService`
- Polls for `FileRecord` rows with `Status = Pending` every 5 seconds
- For each pending record:
  1. Marks the record `Processing`
  2. Opens the file from object storage via `IObjectStorage`
  3. Extracts simple metadata (filename, content type, size, timestamp)
  4. Persists metadata as JSON
  5. Marks the record `Completed`
- On failure: marks the record `Failed`, stores a `FailureReason`
- A single failed job does not crash the worker process
- Applies EF Core migrations on startup

---

## Domain Model

```
FileRecord
  Id               : Guid        (PK)
  OriginalFileName : string      (max 512)
  ContentType      : string      (max 128)
  ObjectKey        : string      (max 512) — path within storage root
  SizeBytes        : long
  Status           : string      — Pending | Processing | Completed | Failed
  UploadedAt       : DateTime UTC
  ProcessedAt      : DateTime UTC (nullable)
  MetadataJson     : text        (nullable) — JSON extracted by worker
  FailureReason    : string      (nullable, max 2048)
```

### ProcessingStatus Enum

```
Pending    — file uploaded, waiting for worker
Processing — worker has claimed the record
Completed  — worker extracted metadata successfully
Failed     — worker encountered an error
```

---

## Storage Abstraction

Interface: `IObjectStorage` (in `CloudIngestion.Core`)

```csharp
Task<string> SaveAsync(Stream, fileName, contentType, ct);
Task<Stream> OpenReadAsync(objectKey, ct);
```

Current implementation: `LocalObjectStorage`

- Writes files to a configurable `StorageRoot` directory
- Generates a stable object key: `{guid}/{sanitized-filename}`
- API and Worker both use the same `StorageRoot`
- In Docker Compose, both containers mount a shared `uploads` volume

Future implementation: `S3ObjectStorage`
- Swappable without changing controller or worker logic
- Only `ServiceRegistration.cs` needs to change

---

## Queue / Pending-Job Abstraction

Interface: `IProcessingQueue` (in `CloudIngestion.Core`)

```csharp
Task PublishAsync(FileProcessingJob, ct);
```

Current implementation: `DatabaseProcessingQueue`
- `PublishAsync` is a no-op: the `FileRecord` with `Status = Pending` is the queue entry
- The worker polls `FileRecords WHERE Status = 'Pending'` directly
- No external broker required for local development

Future implementation: `SqsProcessingQueue`
- `PublishAsync` sends a message to an AWS SQS queue
- Worker reads from SQS instead of polling the database
- Swappable without changing controller logic

---

## Database

- PostgreSQL 16
- EF Core 9 with Npgsql provider
- Migrations are applied automatically on startup in Development mode
- Manual migration command: `dotnet ef database update --project src/CloudIngestion.Infrastructure --startup-project src/CloudIngestion.Api`
- Indexed columns: `Status`, `UploadedAt`

---

## Project Layout

```
CloudIngestion.Core
  ├── Enums/ProcessingStatus.cs
  ├── Models/FileRecord.cs
  ├── Models/FileProcessingJob.cs
  ├── Interfaces/IObjectStorage.cs
  ├── Interfaces/IProcessingQueue.cs
  ├── Interfaces/IFileRepository.cs
  ├── DTOs/FileRecordDto.cs
  ├── DTOs/UploadResultDto.cs
  └── Services/UploadValidator.cs

CloudIngestion.Infrastructure
  ├── Data/AppDbContext.cs
  ├── Data/Migrations/
  ├── Repositories/FileRepository.cs
  ├── Storage/LocalObjectStorage.cs
  ├── Queue/DatabaseProcessingQueue.cs
  └── ServiceRegistration.cs

CloudIngestion.Api
  ├── Controllers/FilesController.cs
  ├── Controllers/HealthController.cs
  ├── Program.cs
  └── appsettings*.json

CloudIngestion.Worker
  ├── FileProcessingWorker.cs
  └── Program.cs
```

---

## Processing Lifecycle

```
Upload received
    │
    ▼
Validate (type, size)
    │
    ▼
Store via IObjectStorage
    │
    ▼
Create FileRecord (Status=Pending)
    │
    ▼
Publish via IProcessingQueue
    │
    ▼
[Worker polls Pending records]
    │
    ▼
Mark Processing
    │
    ├─ success ──► Extract metadata ──► Mark Completed
    │
    └─ failure ──► Store FailureReason ──► Mark Failed
```

---

## Local Development Architecture

```
┌─────────────────────────────────────────────────────┐
│ docker compose up                                    │
│                                                      │
│  postgres:5432  ◄──── api:8080  ◄──── browser:5173  │
│       │                │                  │          │
│       │                │           frontend:80        │
│       └──────────── worker                           │
│                        │                             │
│                    uploads/ (shared volume)          │
└─────────────────────────────────────────────────────┘
```

Or run each service separately:

```bash
# Terminal 1 — PostgreSQL
docker compose up postgres

# Terminal 2 — API
dotnet run --project src/CloudIngestion.Api

# Terminal 3 — Worker
dotnet run --project src/CloudIngestion.Worker

# Terminal 4 — Frontend
cd frontend/cloud-ingestion-ui && npm run dev
```

---

## Future Cloud Integration Points

| Component         | Local (now)               | Cloud (future)        |
|-------------------|---------------------------|-----------------------|
| Object Storage    | `LocalObjectStorage`      | `S3ObjectStorage`     |
| Processing Queue  | `DatabaseProcessingQueue` | `SqsProcessingQueue`  |
| Database          | Docker Compose PostgreSQL | RDS or Aurora         |
| Deployment        | `docker compose up`       | ECS, App Runner, etc. |
| Auth              | None                      | JWT / Cognito         |
| Observability     | Console logs              | CloudWatch            |

All cloud integrations are designed as swappable implementations behind the same `Core` interfaces. No controller or worker code needs to change to switch providers.
