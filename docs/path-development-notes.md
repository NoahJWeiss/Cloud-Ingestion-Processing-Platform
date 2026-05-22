# PATH Development Notes

## What Is PATH

PATH is a scoped execution, validation, and traceability harness for iterative AI-assisted implementation work.

It is not an autonomous agent, a production framework, or a replacement for engineering review. It is a structured execution layer that wraps selected implementation slices with explicit boundaries, captured validation evidence, and reviewable artifacts.

PATH is intended for use after the initial skeleton is established.

---

## Why PATH Is Used Here

AI coding tools can generate and modify software quickly. However, real engineering work still requires:

- Clear scope boundaries — so changes stay inside the intended area
- Reproducible validation — so test runs can be verified, not just claimed
- Traceable implementation history — so reviewers know what changed and why
- Safe recovery — so failed runs can be identified and rolled back
- Reviewable diffs — so human engineers can inspect what the agent produced

PATH addresses these needs by adding a structured harness around selected implementation slices.

This project uses PATH to demonstrate not only the cloud ingestion platform itself, but also what responsible AI-assisted development can look like in practice.

---

## What PATH Records

For each implementation slice run through PATH, the harness is intended to capture:

- The dispatch package defining the objective and allowed files
- Hard target files the agent is allowed to modify
- Reference files the agent may read but not change
- Context used for the implementation slice
- Execution plan and task breakdown
- Files changed and how they changed
- Validation commands executed (e.g., `dotnet build`, `dotnet test`)
- stdout, stderr, exit code, and duration for each validation command
- Diff summaries of the changes
- Run/session status (success, partial, failed)
- Rollback guidance for failed runs
- Reviewer packets summarizing what happened

---

## How Slices Should Be Scoped

A well-scoped PATH slice has:

- **One objective** — a single, named engineering goal
- **Hard target files** — the exact files the agent is allowed to modify
- **Reference files** — files the agent may read for context but must not change
- **Validation command** — at minimum `dotnet build && dotnet test`; more specific commands where appropriate
- **Non-goals** — explicit statement of what the slice does not address
- **Reviewable diff expectations** — a description of what a correct diff should look like

Slices should be small enough to review in a single session. A slice that modifies more than 5–7 files should probably be split.

---

## How Validation Evidence Should Be Captured

Validation evidence is what makes a PATH run trustworthy.

Each PATH run should capture:

- The exact command run
- The full stdout and stderr
- The exit code
- Whether the run passed or failed
- The timestamp of the run

Validation evidence should not be self-reported by the agent. It should be captured from actual command execution by the harness.

---

## How Reviewer Packets Are Used

A reviewer packet is the deliverable a PATH run produces for human review.

It should include:

- The dispatch package (what the agent was asked to do)
- The hard target and reference file lists
- A diff summary showing what changed
- The validation evidence (commands run and results)
- A final status (pass / partial / fail)
- Any noted issues, edge cases, or follow-up recommendations

Reviewer packets allow engineering review of AI-assisted work in the same way code review allows review of human-authored work.

---

## Recommended PATH Implementation Slices

The following slices are suggested for future PATH-managed development. Each is a bounded, testable unit of work that builds on the initial skeleton.

### Slice 1 — S3 Storage Adapter
- **Objective:** Implement `S3ObjectStorage` behind the existing `IObjectStorage` interface
- **Target files:** `src/CloudIngestion.Infrastructure/Storage/S3ObjectStorage.cs`, `ServiceRegistration.cs`
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Changing controller or worker logic; modifying the Core interface

### Slice 2 — SQS Processing Queue Adapter
- **Objective:** Implement `SqsProcessingQueue` behind the existing `IProcessingQueue` interface
- **Target files:** `src/CloudIngestion.Infrastructure/Queue/SqsProcessingQueue.cs`, `ServiceRegistration.cs`
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Modifying the worker polling logic; changing Core interfaces

### Slice 3 — Integration Tests for Upload-to-Processing Lifecycle
- **Objective:** Add integration tests covering the full upload → pending → processing → completed flow
- **Target files:** `tests/CloudIngestion.Api.Tests/`, `tests/CloudIngestion.Worker.Tests/`
- **Validation:** `dotnet test`
- **Non-goals:** Modifying production code; adding new features

### Slice 4 — JWT-Style Authentication
- **Objective:** Add a simple JWT validation middleware to the API
- **Target files:** `src/CloudIngestion.Api/Program.cs`, `src/CloudIngestion.Api/Middleware/`
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Full user management; OAuth flows

### Slice 5 — Audit Event Table
- **Objective:** Add an `AuditEvent` table that records status transitions for each file
- **Target files:** `src/CloudIngestion.Core/Models/AuditEvent.cs`, `src/CloudIngestion.Infrastructure/Data/AppDbContext.cs`, new migration
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Exposing audit events via API; modifying existing FileRecord logic

### Slice 6 — Retry Handling for Failed Jobs
- **Objective:** Add configurable retry logic for failed processing jobs in the worker
- **Target files:** `src/CloudIngestion.Worker/FileProcessingWorker.cs`
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Distributed locking; dead-letter queues

### Slice 7 — Richer Metadata Extraction
- **Objective:** Improve metadata extracted by the worker for supported file types
- **Target files:** `src/CloudIngestion.Worker/FileProcessingWorker.cs`
- **Validation:** `dotnet build && dotnet test`
- **Non-goals:** Third-party parsing libraries; DICOM or clinical metadata

### Slice 8 — Frontend Status Polling
- **Objective:** Add automatic polling on the Files page to refresh status while jobs are pending or processing
- **Target files:** `frontend/cloud-ingestion-ui/src/pages/FilesPage.tsx`
- **Validation:** Manual verification; `npm run build`
- **Non-goals:** WebSocket implementation; backend push events

### Slice 9 — Structured Logging
- **Objective:** Add structured log fields (file ID, status, duration) to key API and worker log events
- **Target files:** `src/CloudIngestion.Api/Controllers/FilesController.cs`, `src/CloudIngestion.Worker/FileProcessingWorker.cs`
- **Validation:** `dotnet build`
- **Non-goals:** External log aggregation; OpenTelemetry

### Slice 10 — Deployment Documentation
- **Objective:** Document how to deploy the platform to AWS (ECS, RDS, S3, SQS)
- **Target files:** `docs/deployment.md`
- **Validation:** Documentation review
- **Non-goals:** Terraform or CDK code; actual deployment

---

## Language to Use When Describing PATH

Use careful language when referring to PATH:

> PATH will be used as a scoped execution, validation, and traceability harness for iterative implementation after the initial skeleton is established.

Avoid describing PATH as:

- An autonomous agent
- A production framework
- A replacement for engineering review
- A tool that guarantees correctness

PATH improves reviewability and traceability. Engineering judgment remains with the human reviewers.
