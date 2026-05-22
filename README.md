# Cloud Ingestion & Processing Platform

A cloud-style backend and processing platform for uploading, storing, tracking, and asynchronously processing large file artifacts such as images, videos, PDFs, STL files, and scan-like assets.

This project is designed as a practical software engineering portfolio system demonstrating backend/cloud architecture, asynchronous processing, containerized services, automated validation, and traceable AI-assisted development using PATH.

---

## Project Status

This repository is currently in the **initial skeleton phase**.

The first development milestone is to create a clean, runnable foundation that includes:

- ASP.NET Core backend API
- .NET worker service
- PostgreSQL metadata database
- Local object storage abstraction
- Database-backed pending-job processing
- React + TypeScript frontend
- Docker Compose local development
- Baseline automated tests
- Architecture documentation
- PATH development notes for future implementation slices

The initial skeleton is intentionally small. It is meant to establish a professional foundation, not a production platform.

---

## Agent Guidance

Coding agents should use `CLAUDE.md` as the primary instruction file.

This README is the human-facing project guide and secondary reference. It explains the project purpose, architecture, commands, and future direction. If a coding agent needs operational constraints, implementation boundaries, or skeleton execution rules, it should read `CLAUDE.md` first.

---

## Project Mission

The Cloud Ingestion & Processing Platform models the core architecture behind reliable file-ingestion systems.

The platform supports a workflow where a user uploads a file, the backend stores the object, metadata is persisted, the file is marked for processing, and a worker service asynchronously extracts simple metadata and updates the processing status.

The project demonstrates:

- Backend cloud engineering concepts
- File ingestion workflows
- Asynchronous background processing
- Clean backend architecture
- Object storage abstraction
- Queue/pending-job abstraction
- PostgreSQL persistence
- Dockerized local services
- React SPA development
- Automated validation
- Responsible AI-assisted development through PATH

This is not a production medical, clinical, or regulated healthcare system. It does not process real patient data. All files and data used with this project should be synthetic or demo-only.

---

## Core Engineering Thesis

Modern AI coding tools can generate and modify software quickly, but real engineering work still requires:

- Clear scope boundaries
- Reproducible validation
- Traceable implementation history
- Safe recovery from failed changes
- Reviewable diffs
- Explicit evidence that tests and validation commands ran
- Confidence that generated code stayed inside the requested task

This project will use PATH as a development harness after the initial skeleton is established.

PATH does not replace the engineer or the coding agent. Instead, it provides a controlled execution and validation layer around selected AI-assisted development slices.

For selected implementation slices, PATH is intended to record:

- The dispatch package defining the objective and allowed files
- Hard target files the agent is allowed to modify
- Reference files the agent may read but not change
- Context used for the implementation slice
- Execution plan and task results
- Files changed
- Validation commands executed
- stdout, stderr, exit code, signal, and duration metadata
- Diff summaries
- Run/session status
- Rollback guidance for failed runs
- Reviewer packets summarizing what happened

This makes the project more than a standard portfolio app. It demonstrates both cloud/backend engineering and responsible AI-assisted software development.

---

## Problem Statement

Large file ingestion systems need to handle more than simple CRUD operations.

They often require:

- Uploading binary objects
- Separating object storage from metadata storage
- Tracking processing state over time
- Running asynchronous background jobs
- Handling processing failures cleanly
- Preserving auditability
- Supporting operational visibility
- Validating behavior through automated tests
- Deploying services in a containerized environment

This project implements a small but realistic version of that architecture.

---

## Target Use Case

A user uploads a file such as:

- `.png`
- `.jpg`
- `.jpeg`
- `.pdf`
- `.mp4`
- `.stl`

The backend validates the file, stores it through an object storage abstraction, creates a metadata record, and makes the file available for background processing.

A worker service later processes the pending file, extracts simple metadata, and updates the file record with processing results.

The frontend displays uploaded files, processing status, and extracted metadata.

Example metadata includes:

- Original file name
- Content type
- File size
- Upload timestamp
- Processing status
- Processing start time
- Processing completion time
- Basic extracted metadata
- Failure reason, if processing fails

---

## Architecture Overview

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

The first version uses a database-backed pending-job workflow instead of a real external queue. This keeps the initial system simple, runnable, and suitable for local development while preserving a clean seam for future SQS integration.

---

## Planned Technology Stack

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- xUnit

### Worker

- .NET Worker Service
- Background processing
- Pending-record polling
- Metadata extraction
- Status updates

### Frontend

- React
- TypeScript
- Vite

### Storage

Initial local development:

- Local filesystem storage through `IObjectStorage`

Future cloud-ready implementation:

- AWS S3 through `S3ObjectStorage`

### Queue / Processing Dispatch

Initial local development:

- Database-backed pending-job polling through `IProcessingQueue` or equivalent abstraction

Future cloud-ready implementation:

- AWS SQS through `SqsProcessingQueue`

### Containers

- Docker
- Docker Compose

### AI-Assisted Development Harness

- PATH
- Dispatch packages
- Hard target enforcement
- Run/session tracing
- Validation command capture
- Reviewer packet generation

---

## Repository Structure

Planned structure:

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

---

## System Components

### API Service

The API service is responsible for:

- Accepting file uploads
- Validating file type and size
- Creating file metadata records
- Storing uploaded objects
- Marking files as pending for processing
- Exposing file status endpoints
- Returning processing results

Initial endpoints:

```http
POST   /api/files/upload
GET    /api/files
GET    /api/files/{id}
GET    /api/health
```

### Core Project

`CloudIngestion.Core` contains the domain and application-level contracts.

Expected contents:

- Domain models
- DTOs
- Enums
- Interfaces
- Upload validation rules
- Processing job contracts
- Core service abstractions

The core project should not depend on infrastructure details such as PostgreSQL, local filesystem storage, S3, or SQS.

### Infrastructure Project

`CloudIngestion.Infrastructure` contains concrete implementations for persistence, storage, and processing dispatch.

Expected contents:

- EF Core `DbContext`
- PostgreSQL configuration
- Local object storage implementation
- Pending-job implementation
- Infrastructure service registrations

Infrastructure code should remain outside controllers.

### Worker Service

`CloudIngestion.Worker` consumes pending processing work and updates file status.

Responsibilities:

- Poll for pending records
- Mark a file as `Processing`
- Open uploaded object through `IObjectStorage`
- Extract simple metadata
- Mark file as `Completed`
- Mark file as `Failed` on processing error
- Store a failure reason when processing fails
- Continue running after a single failed job

### React Frontend

The React frontend provides a minimal user interface for:

- Uploading files
- Viewing uploaded files
- Tracking processing status
- Viewing extracted metadata
- Seeing failure messages

The frontend is not the primary engineering focus of the first milestone. It should be functional, minimal, and easy to extend.

---

## Domain Model

The initial domain model includes a `FileRecord` entity with at least:

```text
Id
OriginalFileName
ContentType
ObjectKey
SizeBytes
Status
UploadedAt
ProcessedAt
MetadataJson
FailureReason
```

The processing status enum should include:

```text
Pending
Processing
Completed
Failed
```

Timestamps should use UTC.

---

## Object Storage Abstraction

The platform separates binary object storage from metadata storage.

Expected interface shape:

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

Initial implementation:

- `LocalObjectStorage`

Future implementation:

- `S3ObjectStorage`

Requirements:

- Use a configurable storage root path.
- Keep object keys stable and safe.
- Do not store large binary data in PostgreSQL.
- Keep controller logic independent from storage details.
- If Docker Compose runs API and worker as separate containers, use a shared volume so both services can access stored files.

---

## Processing Queue / Pending Job Abstraction

The first version should avoid introducing a real message broker. Instead, it should use a simple database-backed pending-job workflow.

Expected interface shape:

```csharp
public interface IProcessingQueue
{
    Task PublishAsync(
        FileProcessingJob job,
        CancellationToken cancellationToken);
}
```

Acceptable skeleton behavior:

- API creates a `FileRecord` with `Status = Pending`
- `IProcessingQueue.PublishAsync` records or confirms that the file is available for processing
- Worker polls pending records from PostgreSQL
- Worker transitions status from `Pending` to `Processing`
- Worker transitions status to `Completed` or `Failed`

Future implementation:

- `SqsProcessingQueue`

---

## Worker Processing Lifecycle

The initial processing lifecycle:

```text
Pending
   |
   v
Processing
   |
   |-- success --> Completed
   |
   |-- failure --> Failed
```

Worker behavior:

1. Fetch pending file records.
2. Mark the selected record as `Processing`.
3. Open the uploaded object from storage.
4. Extract simple metadata.
5. Store metadata as JSON.
6. Mark the record as `Completed`.

On failure:

1. Mark the record as `Failed`.
2. Store a failure reason.
3. Continue processing future jobs.

---

## Local Development

Expected backend commands:

```bash
dotnet build
dotnet test
dotnet run --project src/CloudIngestion.Api
dotnet run --project src/CloudIngestion.Worker
```

Expected Docker command:

```bash
docker compose up
```

Expected frontend commands:

```bash
cd frontend/cloud-ingestion-ui
npm install
npm run dev
npm run build
```

These commands should be updated after skeleton generation if the generated project uses slightly different paths or names.

---

## Testing Strategy

The project should include meaningful baseline tests.

Initial test coverage should include:

- Supported file types are accepted
- Unsupported file types are rejected
- Empty files are rejected
- File records are created with correct status
- Upload service calls object storage
- Uploaded files are made available for worker processing
- Pending records can be discovered by the worker
- Processing state transitions are persisted
- Worker marks a file as `Completed`
- Worker marks a file as `Failed` on processing exception

The first test suite should be useful but not exhaustive.

---

## PATH Development Workflow

The initial repository skeleton may be created outside PATH.

After the skeleton is established, future implementation work should be performed through PATH-managed slices where practical.

PATH will be used as a scoped execution, validation, and traceability harness for iterative implementation.

PATH is expected to help with:

- Defining bounded implementation slices
- Declaring hard target files
- Declaring reference-only files
- Enforcing file modification boundaries
- Capturing execution traces
- Capturing validation command evidence
- Recording failed runs
- Producing reviewer packets
- Preserving implementation history

PATH should not be described as an autonomous agent or production framework. It is a development harness for controlled, reviewable AI-assisted engineering work.

---

## Recommended PATH Implementation Slices

After the initial skeleton is complete, suggested PATH-managed slices include:

1. Add S3 storage adapter
2. Add SQS processing queue adapter
3. Add integration tests for upload-to-processing lifecycle
4. Add JWT-style authentication
5. Add audit event table
6. Add retry handling for failed processing jobs
7. Add richer metadata extraction
8. Add frontend polling for status updates
9. Add Dockerized API and worker services if not completed in the skeleton
10. Add deployment documentation
11. Add structured logs
12. Add processing history endpoint

Each slice should define:

- One objective
- Hard target files
- Reference files
- Required validation command
- Clear non-goals
- Reviewable diff expectations

---

## Documentation

The repository should include:

```text
README.md
CLAUDE.md
docs/architecture.md
docs/path-development-notes.md
```

### `docs/architecture.md`

Should explain:

- API service
- Worker service
- Database model
- Storage abstraction
- Queue/pending-job abstraction
- Processing lifecycle
- Local development architecture
- Future cloud integration points

### `docs/path-development-notes.md`

Should explain:

- Why PATH is used
- What PATH does and does not do
- How future slices should be scoped
- How validation evidence should be captured
- How reviewer packets should be used

Use careful language. Do not overstate PATH capabilities.

---

## Reliability and Auditability Goals

This project emphasizes reliable engineering practices.

Planned reliability features:

- Explicit processing states
- Durable metadata
- Failure reason capture
- Worker failure isolation
- Structured validation
- Dockerized local environment
- Automated tests

Planned auditability features:

- Upload records
- Processing status history
- PATH reviewer packets
- PATH dispatch/session traces
- Validation command evidence

---

## Non-Goals

This project does not attempt to build:

- A production healthcare platform
- HIPAA-compliant infrastructure
- Medical diagnosis tooling
- Real clinical image interpretation
- Real patient-data processing
- Full user/organization management
- Full observability stack
- Production-grade deployment automation
- Complex AI analysis pipelines
- Large-scale distributed processing
- Kubernetes infrastructure
- Terraform infrastructure
- Full authentication/authorization system in the skeleton
- Message broker infrastructure in the skeleton
- Production security posture in the skeleton

The project intentionally stays small enough to be reviewed, tested, and explained.

---

## Milestones

### Milestone 1 — Local Backend Pipeline

Build the core ingestion pipeline locally.

Scope:

- ASP.NET Core API
- PostgreSQL metadata model
- EF Core persistence
- Local object storage adapter
- Pending-job abstraction
- Worker service
- Basic metadata extraction
- xUnit tests

Success criteria:

- API accepts supported file uploads
- Metadata record is created
- File is stored
- File is made available for processing
- Worker processes pending files
- Status transitions are persisted
- Tests pass from the repository root

No AWS is required in this milestone.

### Milestone 2 — Dockerized Local Environment

Add Docker support for local execution.

Scope:

- PostgreSQL container
- Shared local storage volume
- API container if practical
- Worker container if practical
- Frontend container if practical

Success criteria:

- `docker compose up` starts the local system
- API can upload files
- Worker can process pending files
- Database persists metadata
- README documents local startup

### Milestone 3 — Cloud-Ready Adapters

Add cloud-facing infrastructure abstractions and implementations.

Scope:

- `S3ObjectStorage`
- `SqsProcessingQueue`
- Configuration-based provider selection
- Tests using mocks or local substitutes

Success criteria:

- Storage and processing dispatch implementations are swappable
- Local development remains functional
- AWS-specific code is isolated
- Cloud configuration is documented

### Milestone 4 — React Dashboard

Add a minimal frontend.

Scope:

- Upload form
- File list
- Processing status badges
- Metadata detail view
- Error display

Success criteria:

- User can upload a file from the dashboard
- Dashboard displays uploaded files
- Dashboard shows processing status
- Metadata can be inspected after processing

### Milestone 5 — Reliability and Security Polish

Add engineering polish that supports the cloud systems narrative.

Possible scope:

- Request validation
- File type validation
- File size limits
- Structured logs
- Audit event records
- Failed processing handling
- Retry-safe worker behavior
- Basic JWT-style authentication if still appropriate

Success criteria:

- Invalid uploads are rejected clearly
- Failed processing is visible and recoverable
- Logs and audit records explain important system events
- Auth exists only if it does not overtake the project scope

### Milestone 6 — PATH Trace Showcase

Curate selected PATH traces for the final portfolio version.

Representative slices:

- Upload API slice
- Metadata model slice
- Processing dispatch slice
- Worker processing slice
- S3 adapter slice
- React dashboard slice

Each showcased slice should include:

- Dispatch package
- Reviewer packet
- Diff summary
- Validation command evidence
- Final status

---

## Success Criteria

The project is successful when it demonstrates:

- A working local ingestion pipeline
- Clear backend architecture
- Asynchronous worker processing
- Object storage abstraction
- Processing dispatch abstraction
- PostgreSQL metadata persistence
- Dockerized local development
- Automated tests for core behavior
- Minimal but usable React dashboard
- PATH-managed implementation traces for selected slices
- A README that clearly explains architecture, tradeoffs, and validation

The final system should be demoable in under a few minutes.

---

## Resume Positioning

Suggested resume entry:

```text
Cloud Ingestion & Processing Platform
C#, ASP.NET Core, React, Docker, PostgreSQL, AWS S3/SQS

Built a cloud-style ingestion platform for uploading and asynchronously processing large file artifacts including images, PDFs, videos, and STL files. Implemented REST APIs, object storage abstraction, queue-based worker processing, metadata extraction, processing-status tracking, Dockerized local services, and automated tests. Used a custom PATH execution harness to coordinate scoped AI-assisted implementation slices with validation traces, hard-target enforcement, and reviewable run artifacts.
```

---

## Final Project Pitch

This project demonstrates a cloud-style ingestion and asynchronous processing architecture using C#, ASP.NET Core, PostgreSQL, object storage abstraction, worker-based processing, Docker, React, and automated tests.

It also demonstrates a responsible AI-assisted development workflow through PATH, a custom execution harness that scopes agent work, enforces hard target boundaries, records validation evidence, and produces reviewable implementation traces.

The result is a practical portfolio system that shows both cloud systems engineering capability and modern AI-assisted software development discipline.
