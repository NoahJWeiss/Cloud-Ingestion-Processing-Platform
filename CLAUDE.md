# CLAUDE.md

## Instruction Hierarchy

This file is the primary operating guide for Claude and other coding agents working in this repository.

Use the files in this order:

1. `CLAUDE.md` — primary source of truth for agent behavior, implementation scope, architecture constraints, and skeleton execution.
2. `README.md` — secondary human-facing project guide. Use it for project purpose, architecture explanation, local commands, and documentation expectations.
3. Source code and tests — final authority for actual implementation details once the repository exists.

If `CLAUDE.md` and `README.md` conflict, follow `CLAUDE.md` and note the mismatch in the final response.

Do not treat the README as an instruction to expand scope. The README explains the project; this file controls how to build it.

---

## Project Mission

Build a portfolio-grade cloud/backend demo that demonstrates backend cloud engineering, asynchronous ingestion workflows, Dockerized services, automated testing, and a minimal React SPA.

The project is a **Cloud Ingestion & Processing Platform** for uploading, storing, asynchronously processing, and tracking large file artifacts such as images, PDFs, videos, STL files, and scan-like assets.

This is not a production medical, clinical, or regulated healthcare system. Do not claim that it processes real patient data. Use synthetic/demo data only.

---

## Current Objective: Initial Skeleton

Create a clean, runnable architecture skeleton that establishes the foundation for future iterative development.

The skeleton should be **real but thin**:

- Real enough to build, run, test, and demonstrate an end-to-end flow.
- Thin enough to remain understandable, maintainable, and easy to extend through PATH-managed implementation slices later.

Do not build a complete production system.

Prefer simple working vertical slices over incomplete pseudo-architecture.

---

## Skeleton Depth Rule

Every major component should exist and participate in the end-to-end flow, but each component should use the simplest maintainable implementation.

Use:

- Local filesystem storage before S3.
- Database-backed pending-job polling before SQS.
- Minimal React views before polished UI.
- Basic xUnit coverage before broad integration testing.
- Simple metadata extraction before advanced file analysis.
- PostgreSQL locally before cloud-hosted database deployment.
- Docker Compose before Kubernetes or Terraform.

Do not leave empty placeholder projects that compile but do not participate in the flow.

Do not build production-grade infrastructure prematurely.

---

## Target Architecture

```text
React Frontend
   |
   v
ASP.NET Core API
   |
   |-- Validate upload
   |-- Create file metadata record
   |-- Store uploaded object
   |-- Mark file as pending for processing
   |
   v
PostgreSQL
   |
   v
Worker Service
   |
   |-- Poll pending records
   |-- Mark file as Processing
   |-- Open uploaded object
   |-- Extract simple metadata
   |-- Mark file as Completed or Failed
   |
   v
PostgreSQL
```

The skeleton should model an event-driven/cloud-ingestion architecture, but it does not need a real external queue in the first version.

For the initial skeleton, prefer **database-backed pending-job polling** so the API and worker can run as separate processes without requiring AWS SQS, RabbitMQ, Kafka, or another message broker.

---

## Required Repository Structure

Create the repository using this structure:

```text
/
  README.md
  CLAUDE.md
  docker-compose.yml
  .gitignore

  src/
    CloudIngestion.Api/
    CloudIngestion.Worker/
    CloudIngestion.Core/
    CloudIngestion.Infrastructure/

  tests/
    CloudIngestion.Api.Tests/
    CloudIngestion.Worker.Tests/
    CloudIngestion.Core.Tests/

  frontend/
    cloud-ingestion-ui/

  docs/
    architecture.md
    path-development-notes.md
```

Keep the structure simple. Do not add extra projects unless they are clearly required for the skeleton.

---

## Backend Stack

Use:

- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- xUnit
- Docker

Use clean separation:

### `CloudIngestion.Core`

Contains:

- Domain models
- DTOs
- Enums
- Interfaces
- Core validation rules
- Simple business logic

### `CloudIngestion.Infrastructure`

Contains:

- EF Core database context
- PostgreSQL persistence
- Local filesystem storage implementation
- Pending-job/queue implementation
- Infrastructure service registrations

### `CloudIngestion.Api`

Contains:

- HTTP controllers
- API request/response models when needed
- Dependency injection setup
- API configuration
- Health endpoint

Controllers should stay thin. Do not put storage, database, or worker-processing logic directly in controllers.

### `CloudIngestion.Worker`

Contains:

- Background worker service
- Pending-job polling
- File processing orchestration
- Processing status updates
- Failure handling

---

## Frontend Stack

Use:

- React
- TypeScript
- Vite

The frontend should include:

- File upload page
- Uploaded files list
- Processing status display
- Basic metadata details view

Keep styling minimal. Functionality matters more than polish.

If tradeoffs are required, prioritize backend build correctness, tests, and local run instructions over frontend polish.

---

## Core Domain Model

Create a `FileRecord` entity with at least:

- `Id`
- `OriginalFileName`
- `ContentType`
- `ObjectKey`
- `SizeBytes`
- `Status`
- `UploadedAt`
- `ProcessedAt`
- `MetadataJson`
- `FailureReason`

Create a `ProcessingStatus` enum:

```text
Pending
Processing
Completed
Failed
```

Use UTC timestamps.

---

## Required API Endpoints

Create these initial endpoints:

```http
POST   /api/files/upload
GET    /api/files
GET    /api/files/{id}
GET    /api/health
```

Expected upload behavior:

1. Upload endpoint receives a file.
2. API validates file type and size.
3. API stores file using `IObjectStorage`.
4. API creates a `FileRecord`.
5. API makes the file available for processing through the pending-job/queue abstraction.
6. API returns file ID and initial status.

The API does not need production authentication in the initial skeleton.

---

## Upload Validation Rules

Initial upload validation:

- Allow `.png`, `.jpg`, `.jpeg`, `.pdf`, `.mp4`, `.stl`
- Reject unsupported file types
- Reject empty files
- Enforce a configurable max file size
- Return structured error responses

Do not implement malware scanning, DICOM parsing, content moderation, or production-grade upload security in the skeleton.

---

## Storage Abstraction

Create an interface similar to:

```csharp
public interface IObjectStorage
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        string objectKey,
        CancellationToken cancellationToken);
}
```

For the skeleton, implement local filesystem storage first.

Requirements:

- Use a configurable storage root path.
- Keep object keys stable and safe.
- Do not store large binary data in PostgreSQL.
- Prepare the code so S3 can be added later without changing controller logic.
- If Docker Compose runs API and worker as separate containers, use a shared volume so both services can access stored files.

Do not add AWS S3 in the initial skeleton unless explicitly requested later.

---

## Queue / Pending Job Abstraction

Create an interface similar to:

```csharp
public interface IProcessingQueue
{
    Task PublishAsync(
        FileProcessingJob job,
        CancellationToken cancellationToken);
}
```

For the skeleton, prefer a database-backed pending-job polling approach.

Acceptable skeleton implementation:

- API creates a `FileRecord` with `Status = Pending`.
- `IProcessingQueue.PublishAsync` records or confirms that the file is available for processing.
- Worker polls pending records from PostgreSQL.
- Worker transitions status from `Pending` to `Processing` to `Completed` or `Failed`.

The abstraction should make future SQS integration possible without changing controller logic.

Do not add:

- AWS SQS
- RabbitMQ
- Kafka
- MassTransit
- MediatR
- CQRS
- Event sourcing

These are out of scope for the skeleton.

---

## Worker Behavior

The worker should:

1. Poll or receive pending processing jobs.
2. Mark the file as `Processing`.
3. Open the uploaded object through `IObjectStorage`.
4. Extract simple metadata:
   - file size
   - content type
   - original filename
   - processed timestamp
5. Store metadata as JSON.
6. Mark the file as `Completed`.

On failure:

- Mark the file as `Failed`
- Store a failure reason
- Do not crash the entire worker process because of one failed job

The worker should be simple and deterministic. Do not add complex retry policy, distributed locks, or advanced scheduling in the initial skeleton.

---

## Database Requirements

Use EF Core with PostgreSQL for runtime persistence.

The database should store:

- File metadata
- Processing status
- Failure reason
- Extracted metadata JSON

Include an initial migration if practical.

If migrations are not automatically applied, document the exact migration/update command in `README.md`.

For local development, it is acceptable to apply migrations automatically in Development mode if the behavior is documented clearly.

---

## Docker Requirements

Create a `docker-compose.yml` that can run at least:

- PostgreSQL

If practical, also include:

- API
- Worker
- Frontend

When local filesystem storage is used with Docker Compose, configure a shared storage volume accessible to both API and worker.

The local development workflow should be clear and reliable.

Required commands should be documented in `README.md`.

---

## Testing Requirements

Create meaningful baseline tests using xUnit.

Tests should cover:

- Upload validation accepts supported file types
- Upload validation rejects unsupported file types
- Empty files are rejected
- File metadata record is created
- Processing job is made available through the queue/pending-job mechanism
- Worker transitions status from `Pending` to `Processing` to `Completed`
- Worker marks failed jobs as `Failed`

Tests do not need to cover real cloud services yet.

Avoid massive test suites initially. The goal is baseline confidence, not exhaustive coverage.

---

## Documentation Requirements

Update `README.md` with accurate commands and project status after implementation.

Use the README for:

- Project purpose
- Architecture overview
- Tech stack
- Repository structure
- Local setup instructions
- Test commands
- Docker commands
- API endpoint list
- Known limitations
- Future AWS/S3/SQS roadmap
- PATH development workflow summary

Create `docs/architecture.md` explaining:

- API service
- Worker service
- Database model
- Storage abstraction
- Queue/pending-job abstraction
- Processing lifecycle
- Local development architecture
- Future cloud integration points

Create `docs/path-development-notes.md` explaining that future implementation slices will be developed and validated through PATH.

Use careful language:

> PATH will be used as a scoped execution, validation, and traceability harness for iterative implementation after the initial skeleton is established.

Do not describe PATH as an autonomous agent, production framework, or replacement for engineering review.

---

## PATH Handoff Requirements

The initial skeleton should be easy to continue through PATH.

To support that, keep the project:

- Modular
- Buildable
- Testable
- Clearly documented
- Organized around small future implementation slices

Recommended future PATH slices should be listed after the skeleton is complete.

Examples:

- Add S3 storage adapter
- Add SQS processing queue adapter
- Add integration tests for upload-to-processing lifecycle
- Add JWT-style authentication
- Add audit event table
- Add retry handling for failed processing jobs
- Add richer metadata extraction
- Add frontend polling for status updates
- Add Dockerized API and worker services if not completed in the skeleton
- Add deployment documentation

---

## Engineering Constraints

Follow these constraints:

- Prefer simple, maintainable code.
- Do not build a production medical system.
- Do not use real patient data.
- Do not implement authentication in the initial skeleton unless trivial.
- Do not add Kubernetes.
- Do not add Terraform.
- Do not add complex cloud deployment.
- Do not add AI/ML processing.
- Do not add real clinical image interpretation.
- Do not add AWS services in the initial skeleton unless explicitly requested.
- Do not add RabbitMQ, Kafka, MassTransit, CQRS, MediatR, or event sourcing.
- Do not overbuild the frontend.
- Do not create empty placeholder projects that do not compile.
- Keep the skeleton runnable and understandable.

Use only the abstractions needed for:

- Storage
- Processing dispatch
- File processing
- Persistence

Avoid architecture patterns that do not directly help this skeleton.

---

## Definition of Done

The skeleton is complete when:

- Solution builds successfully
- Backend API starts
- PostgreSQL can run locally
- Upload endpoint exists
- File metadata model exists
- Storage abstraction exists
- Local storage implementation exists
- Queue/pending-job abstraction exists
- Worker project exists
- Worker can process pending files
- Status transitions are visible
- At least basic tests exist
- Frontend app exists with upload/status views
- README explains how to run the project
- Architecture docs exist
- PATH development notes exist
- Recommended next PATH slices are listed

---

## Preferred Commands

Use commands similar to:

```bash
dotnet build
dotnet test
docker compose up
```

For frontend:

```bash
npm install
npm run dev
npm run build
```

Update these commands in `README.md` if the actual generated project uses slightly different ones.

---

## Final Output Expected From Agent

After building the skeleton, provide:

1. Summary of what was created
2. Repository structure
3. Commands to run backend
4. Commands to run tests
5. Commands to run frontend
6. Docker Compose commands
7. Known limitations
8. Recommended next implementation slices for PATH

---

## Quality Bar

The final skeleton should feel like a professional starting point for a cloud/backend engineering project.

It should be:

- Buildable
- Runnable
- Testable
- Understandable
- Small enough to review
- Structured enough to extend
- Honest about limitations
- Ready for PATH-managed iterative development

The goal is not maximum feature count.

The goal is a clean foundation that demonstrates engineering judgment.
