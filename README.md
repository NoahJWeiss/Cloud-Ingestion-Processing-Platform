# Cloud Ingestion & Processing Platform

A cloud-style backend and processing platform for uploading, storing, tracking, and asynchronously processing large file artifacts such as images, videos, PDFs, STL files, and scan-like assets.

This project is designed as a practical software engineering portfolio system demonstrating backend/cloud architecture, event-driven processing, containerized services, automated validation, and traceable AI-assisted development using PATH.

---

## 1. Project Overview

The Cloud Ingestion & Processing Platform is a full-stack engineering demo that models a reliable file-ingestion workflow similar to systems used for large operational, imaging, and artifact-processing workloads.

The platform supports:

- Secure file upload through an ASP.NET Core API
- Object storage for large files
- Metadata persistence in PostgreSQL
- Queue-based asynchronous processing
- A background worker for metadata extraction and status updates
- A React dashboard for upload and processing visibility
- Dockerized local development
- Automated tests for core ingestion and processing flows
- PATH-managed implementation slices for traceable AI-assisted development

The goal is not to build a real clinical or regulated medical system. The goal is to demonstrate the engineering patterns behind reliable cloud ingestion systems: durable storage, asynchronous workflows, validation, auditability, status tracking, and reviewable implementation.

---

## 2. Core Engineering Thesis

Modern AI coding tools can generate and modify software quickly, but real engineering work still requires:

- Clear scope boundaries
- Reproducible validation
- Traceable implementation history
- Safe recovery from failed changes
- Reviewable diffs
- Explicit evidence that tests and validation commands ran
- Confidence that generated code stayed inside the requested task

This project uses PATH as a development harness to manage AI-assisted implementation work.

PATH does not replace the engineer or the coding agent. Instead, it provides a controlled execution layer around agent-assisted development.

For selected implementation slices, PATH records:

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

## 3. Problem Statement

Large file ingestion systems need to handle more than simple CRUD operations.

They often require:

- Uploading large binary objects
- Separating file storage from metadata storage
- Tracking processing state over time
- Running asynchronous background jobs
- Handling processing failures cleanly
- Preserving audit logs
- Supporting operational visibility
- Validating the system through automated tests
- Deploying services in a containerized environment

This project implements a small but realistic version of that architecture.

---

## 4. Target Use Case

A user uploads a file such as:

- `.png`
- `.jpg`
- `.pdf`
- `.mp4`
- `.stl`

The backend stores the file, records metadata, queues a processing job, and returns a tracking record.

A worker service later processes the queued job, extracts basic metadata, and updates the file record with processing results.

The frontend displays upload status and processing metadata.

Example metadata includes:

- File name
- File size
- Content type
- Upload timestamp
- Processing status
- Processing start time
- Processing completion time
- Image dimensions, when applicable
- Basic STL/file metadata, when applicable
- Failure reason, if processing fails

---

## 5. Architecture

```text
React Dashboard
   |
   v
ASP.NET Core API
   |
   |-- Validate upload
   |-- Store object
   |-- Persist metadata
   |-- Publish processing job
   |
   v
Object Storage + Queue + PostgreSQL
   |
   v
Worker Service
   |
   |-- Consume processing job
   |-- Load object
   |-- Extract metadata
   |-- Update processing status
   |
   v
PostgreSQL
6. Planned Technology Stack
Backend
C#
ASP.NET Core
REST APIs
Entity Framework Core
PostgreSQL
Worker
.NET Worker Service
Queue consumer
Metadata extraction pipeline
Frontend
React
TypeScript
Upload form
Processing status dashboard
Metadata detail view
Storage

Initial local development:

Local file storage or MinIO-compatible object storage

Cloud-ready implementation:

AWS S3
Queue

Initial local development:

In-memory or local queue abstraction

Cloud-ready implementation:

AWS SQS
Containers
Docker
Docker Compose
Testing
xUnit
Unit tests
Integration tests where practical
AI-Assisted Development Harness
PATH
Dispatch packages
Hard target enforcement
Run/session tracing
Validation command capture
Reviewer packet generation
7. System Components
7.1 API Service

The API service is responsible for:

Accepting file uploads
Validating file type and size
Creating file metadata records
Storing uploaded objects
Publishing processing jobs
Exposing file status endpoints
Returning processing results

Planned endpoints:

POST /api/files/upload
GET  /api/files
GET  /api/files/{id}
GET  /api/files/{id}/status

Possible later endpoints:

POST /api/auth/login
GET  /api/audit/events
POST /api/files/{id}/retry
7.2 Object Storage

The platform separates binary file storage from relational metadata storage.

The storage layer will be abstracted behind an interface such as:

public interface IObjectStorage
{
    Task<ObjectStorageResult> UploadAsync(
        Stream content,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream> DownloadAsync(
        string objectKey,
        CancellationToken cancellationToken);
}

Initial implementations may include:

LocalObjectStorage
S3ObjectStorage

This keeps the application testable and prevents cloud-specific details from leaking through the core API.

7.3 Metadata Database

PostgreSQL stores durable metadata records.

A file record may include:

Id
OriginalFileName
ObjectKey
ContentType
SizeBytes
UploadStatus
ProcessingStatus
UploadedAtUtc
ProcessingStartedAtUtc
ProcessingCompletedAtUtc
FailureReason
ExtractedMetadataJson

Processing status values may include:

Uploaded
Queued
Processing
Completed
Failed
7.4 Queue Publisher

After an upload is accepted and metadata is persisted, the API publishes a processing job.

The queue abstraction may look like:

public interface IProcessingQueue
{
    Task PublishAsync(
        FileProcessingJob job,
        CancellationToken cancellationToken);
}

Initial implementations may include:

InMemoryProcessingQueue
SqsProcessingQueue

The purpose is to demonstrate an event-driven architecture without forcing cloud infrastructure into the earliest development milestone.

7.5 Worker Service

The worker service consumes processing jobs and updates file records.

Responsibilities:

Pull job from queue
Mark file as Processing
Download the object
Extract metadata
Mark file as Completed
Record failure reason on error
Avoid corrupting file state during failed processing

The first worker implementation should stay intentionally simple. Metadata extraction should focus on file properties, not complex domain-specific analysis.

7.6 React Dashboard

The dashboard will provide a minimal user interface for:

Uploading files
Viewing uploaded files
Tracking processing status
Viewing extracted metadata
Seeing failure messages

The frontend is not the primary engineering focus, but it makes the system easier to demonstrate.

8. PATH Development Harness

This project will be developed using a hybrid workflow.

Initial repository scaffolding may be created outside PATH using a standard coding agent and an AGENTS.md file.

Once the base structure exists, PATH will manage bounded implementation slices.

PATH is used to demonstrate responsible AI-assisted development practices:

The agent receives a scoped dispatch package.
PATH enforces declared hard target files.
PATH captures execution and validation evidence.
Failed runs are recorded rather than hidden.
Validation commands are explicit and inspectable.
Reviewer packets summarize what changed and why.
9. PATH Portfolio Readiness Requirements

Before using PATH heavily on this project, PATH should support a small set of trust and review features.

9.1 Hard Target Enforcement

PATH must reject write attempts outside declared hard_target_files.

This applies to:

create_file
write_file
modify_file
replace_region
delete_file

Reference files are readable only.

Expected behavior:

{
  "status": "preflight_rejected",
  "failure_type": "out_of_scope",
  "attempted_path": "backend/UnexpectedFile.cs",
  "message": "Write rejected because attempted path is not in hard_target_files."
}

This is the most important distinction between PATH and a simple instruction file. A prompt can request scope discipline; PATH should enforce it.

9.2 Run Snapshot Per Execution

Each PATH run should preserve enough evidence to reconstruct what happened.

Expected captured artifacts include:

Dispatch package
Context request
Context bundle or context summary
Execution graph
Execution state
Files changed
Diff summary
Validation runtime health
Required validation command evidence
Command stdout/stderr/exit metadata
Final run status

A reviewer should be able to inspect a run folder and answer:

What was the agent asked to do?
What files was it allowed to modify?
What files did it actually change?
What validation command ran?
Did validation pass?
What failed, if anything?
9.3 Validation Command Capture

Validation commands should be explicit in the dispatch package and recorded in the run artifacts.

Example:

{
  "required_validation_commands": [
    "dotnet test ./backend/CloudIngestion.Tests/CloudInestion.Tests.csproj"
  ]
}

PATH should record:

{
  "command": "dotnet test ./backend/CloudIngestion.Tests/CloudIngestion.Tests.csproj",
  "working_directory": ".",
  "exit_code": 0,
  "stdout": "...",
  "stderr": "...",
  "duration_ms": 18342,
  "satisfied_required_validation": true
}

The goal is to avoid vague claims like “tests passed.” The exact command and result should be visible.

9.4 Basic Rollback Guidance

PATH does not need automatic rollback for this project.

Git remains the rollback layer.

PATH should record enough recovery metadata to support manual rollback:

Branch name
Base commit SHA
Pre-run commit SHA
Changed files
Failed run ID
Diff summary
Suggested recovery commands

Example:

{
  "artifact_type": "execution.rollback_guidance",
  "run_id": "example-run-id",
  "base_commit": "abc123",
  "changed_files": [
    "backend/Controllers/FilesController.cs"
  ],
  "failed": true,
  "suggested_recovery": [
    "Review diff_summary.json",
    "Use git checkout -- <file> to restore individual files",
    "Use git reset --hard <base_commit> only if discarding all run changes is acceptable"
  ]
}

The purpose is not to automate recovery prematurely. The purpose is to make failed agent work easier to review and safely unwind.

9.5 Reviewer Packet

Each meaningful PATH session should emit a human-readable reviewer packet.

Example path:

.path/sessions/<session_id>/reviewer_packet.md

The reviewer packet should summarize:

Objective
Slice type
Hard target files
Reference files
Files changed
Validation commands
Validation results
Failure classification
Diff summary
Scope enforcement result
Rollback guidance
Final dispatch status
Manual review notes

This packet should become the primary artifact used in the project README and portfolio writeup.

Raw JSON artifacts may remain available, but the reviewer packet should be the first place a human looks.

10. Development Strategy

The project will be built in milestones.

The goal is to keep each milestone small, validated, and demonstrable.

11. Milestone 1 — Local Backend Pipeline

Build the core ingestion pipeline locally.

Scope:

ASP.NET Core API
PostgreSQL metadata model
Local object storage adapter
Queue abstraction
Worker service
Basic metadata extraction
xUnit tests

Success criteria:

API accepts supported file uploads
Metadata record is created
File is stored
Processing job is queued
Worker processes job
Status transitions are persisted
Tests pass from the repository root

No AWS is required in this milestone.

12. Milestone 2 — Dockerized Local Environment

Add Docker support for local execution.

Scope:

API container
Worker container
PostgreSQL container
Optional MinIO container
Docker Compose configuration

Success criteria:

docker compose up starts the local system
API can upload files
Worker can process queued jobs
Database persists metadata
README documents local startup
13. Milestone 3 — Cloud-Ready Adapters

Add cloud-facing infrastructure abstractions and implementations.

Scope:

IObjectStorage
S3ObjectStorage
IProcessingQueue
SqsProcessingQueue
Configuration-based provider selection
Tests using mocks or local substitutes

Success criteria:

Storage and queue implementations are swappable
Local development remains functional
AWS-specific code is isolated
Cloud configuration is documented
14. Milestone 4 — React Dashboard

Add a minimal frontend.

Scope:

Upload form
File list
Processing status badges
Metadata detail view
Error display

Success criteria:

User can upload a file from the dashboard
Dashboard displays uploaded files
Dashboard updates or refreshes processing status
Metadata can be inspected after processing
15. Milestone 5 — Reliability and Security Polish

Add engineering polish that supports the cloud systems narrative.

Possible scope:

Request validation
File type validation
File size limits
Structured logs
Audit event records
Basic JWT-style authentication
Failed processing handling
Retry-safe worker behavior

Success criteria:

Invalid uploads are rejected clearly
Failed processing is visible and recoverable
Logs and audit records explain important system events
Auth exists if included, but does not overtake the project scope
16. Milestone 6 — PATH Trace Showcase

Curate selected PATH traces for the final portfolio version.

Representative slices:

Upload API slice
Metadata model slice
Queue publisher slice
Worker processing slice
S3 adapter slice
React dashboard slice

Each showcased slice should include:

Dispatch package
Reviewer packet
Diff summary
Validation command evidence
Final status

The repository should include a short section explaining how PATH was used responsibly.

17. Example PATH Dispatch Slice

Example implementation slice:

{
  "artifact_type": "control.dispatch_package",
  "version": "1.0",
  "slice_type": "vertical_backend_slice",
  "change_expectation": "implementation",
  "objective": "Add the initial file upload API endpoint that validates an uploaded file, stores metadata, and returns a tracking record.",
  "hard_target_files": [
    "backend/src/CloudIngestion.Api/Controllers/FilesController.cs",
    "backend/src/CloudIngestion.Application/Files/FileUploadService.cs",
    "backend/src/CloudIngestion.Application/Files/FileUploadRequest.cs",
    "backend/src/CloudIngestion.Application/Files/FileUploadResult.cs",
    "backend/tests/CloudIngestion.Tests/Files/FileUploadServiceTests.cs"
  ],
  "reference_files": [
    "backend/src/CloudIngestion.Domain/Files/FileRecord.cs",
    "backend/src/CloudIngestion.Infrastructure/Storage/IObjectStorage.cs",
    "backend/src/CloudIngestion.Infrastructure/Data/AppDbContext.cs"
  ],
  "required_validation_commands": [
    "dotnet test ./backend/CloudIngestion.sln -c Release"
  ],
  "key_constraints": [
    "Do not add AWS-specific code in this slice.",
    "Do not implement queue publishing yet.",
    "Do not modify frontend files.",
    "Do not change Docker configuration.",
    "Keep upload validation minimal and testable."
  ],
  "success_criteria": [
    "API accepts a supported uploaded file.",
    "File metadata is persisted.",
    "The uploaded object is passed to the storage abstraction.",
    "The API returns a tracking ID.",
    "Invalid file types are rejected.",
    "Required tests pass."
  ]
}
18. Repository Structure

Planned structure:

cloud-ingestion-platform/
  backend/
    src/
      CloudIngestion.Api/
      CloudIngestion.Application/
      CloudIngestion.Domain/
      CloudIngestion.Infrastructure/
      CloudIngestion.Worker/
    tests/
      CloudIngestion.Tests/
    CloudIngestion.sln

  frontend/
    src/
    package.json

  infra/
    docker/
    aws/

  docs/
    architecture.md
    path-traces/
    diagrams/

  .path/
    artifacts/
    runs/
    sessions/

  docker-compose.yml
  AGENTS.md
  README.md
19. Testing Strategy

The project should include tests for:

Upload Validation
Supported file types are accepted
Unsupported file types are rejected
Empty files are rejected
Oversized files are rejected if size limits are implemented
Metadata Creation
File records are created with correct status
Object keys are generated consistently
Upload timestamps are recorded
Storage Abstraction
Upload service calls object storage
Storage failures produce expected errors
Metadata is not incorrectly marked complete after failed storage
Queue Publishing
Processing job is published after successful upload
Queue message contains the correct file ID/object key
Queue publishing failure is handled explicitly
Worker Processing
Worker marks file as processing
Worker extracts metadata
Worker marks file as completed
Worker marks file as failed on processing exception
Integration Flow
Upload → queue → worker → completed status
20. Reliability and Auditability Goals

This project emphasizes reliable engineering practices.

Planned reliability features:

Explicit processing states
Durable metadata
Idempotent or retry-safe worker behavior where practical
Failure reason capture
Structured logs
Audit events for upload and processing transitions
Validation tests
Dockerized local environment

Planned auditability features:

Upload event records
Processing status history
PATH reviewer packets
PATH dispatch/session traces
Validation command evidence
21. Non-Goals

This project does not attempt to build:

A production healthcare platform
HIPAA-compliant infrastructure
Medical diagnosis tooling
Real clinical image interpretation
Full user/organization management
Full observability stack
Production-grade deployment automation
Complex AI analysis pipelines
Large-scale distributed processing

The project intentionally stays small enough to be reviewed, tested, and explained.

22. Success Criteria

The project is successful when it demonstrates:

A working local ingestion pipeline
Clear backend architecture
Asynchronous worker processing
Object storage abstraction
Queue abstraction
PostgreSQL metadata persistence
Dockerized development environment
Automated tests for core behavior
Minimal but usable React dashboard
PATH-managed implementation traces for selected slices
A README that clearly explains architecture, tradeoffs, and validation

The final system should be demoable in under a few minutes.

23. Resume Positioning

Suggested resume entry:

Cloud Ingestion & Processing Platform
C#, ASP.NET Core, React, Docker, PostgreSQL, AWS S3/SQS

Built a cloud-style ingestion platform for uploading and asynchronously processing large file artifacts including images, PDFs, videos, and STL files. Implemented REST APIs, object storage abstraction, queue-based worker processing, metadata extraction, processing-status tracking, Dockerized local services, and automated tests. Used a custom PATH execution harness to coordinate scoped AI-assisted implementation slices with validation traces, hard-target enforcement, and reviewable run artifacts.
24. Engineering Principles

This project follows several engineering principles:

Build small, complete slices

Each feature should be implemented as a bounded vertical slice with clear validation.

Prefer abstractions that protect the architecture

Storage and queue abstractions are useful because they allow local and cloud implementations without rewriting application logic.

Avoid premature platform complexity

The project should not become a fake enterprise system. It should remain small, working, and explainable.

Use AI assistance responsibly

AI-generated or AI-assisted implementation is acceptable only when changes are scoped, validated, and reviewable.

Treat validation as evidence

A passing test claim is not enough. The exact validation command and result should be recorded.

Keep the portfolio story clear

The final project should communicate backend/cloud competence first, and PATH-enabled traceability second.

25. Final Project Pitch

This project demonstrates a cloud-style ingestion and asynchronous processing architecture using C#, ASP.NET Core, PostgreSQL, object storage, queue-based workers, Docker, React, and automated tests.

It also demonstrates a responsible AI-assisted development workflow through PATH, a custom execution harness that scopes agent work, enforces hard target boundaries, records validation evidence, and produces reviewable implementation traces.

The result is a practical portfolio system that shows both cloud systems engineering capability and modern AI-assisted software development discipline.
