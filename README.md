# Cloud Ingestion & Processing Platform

A cloud-style backend and processing platform for uploading, storing, tracking, and asynchronously processing large file artifacts such as images, videos, PDFs, STL files, and scan-like assets.

This project demonstrates backend cloud engineering, asynchronous ingestion workflows, Dockerized services, clean architecture, and automated testing. After the initial skeleton, iterative development is conducted through a scoped AI-assisted harness called PATH.

---

## Project Status

**Initial skeleton complete.**

The skeleton is buildable, runnable, and testable end-to-end. It includes:

- ASP.NET Core Web API with upload, list, and status endpoints
- .NET Worker Service that polls and processes pending files
- PostgreSQL metadata persistence via EF Core
- Local filesystem object storage (swappable to S3)
- Database-backed pending-job queue (swappable to SQS)
- React + TypeScript frontend (Vite)
- Docker Compose for PostgreSQL + API + Worker + Frontend
- Baseline xUnit tests for validation, upload flow, and worker status transitions
- Architecture documentation and PATH development notes

---

## Architecture Overview

```
React Frontend (port 5173)
        |
        v  HTTP REST
ASP.NET Core API (port 5000)
        |
        |── Validate upload (type, size)
        |── Store file via IObjectStorage (local filesystem)
        |── Create FileRecord (Status=Pending) in PostgreSQL
        |── Publish via IProcessingQueue (no-op for DB-backed queue)
        |
        v
PostgreSQL (port 5432)
        |
        v  polls every 5 seconds
Worker Service
        |
        |── Mark file Processing
        |── Open file via IObjectStorage
        |── Extract metadata
        |── Mark file Completed (or Failed)
        |
        v
PostgreSQL
```

The first version uses database-backed pending-job polling instead of an external queue. This makes the system simple to run locally while preserving clean seams for future SQS and S3 integration.

---

## Technology Stack

| Layer        | Technology                           |
|--------------|--------------------------------------|
| API          | C#, ASP.NET Core Web API, net10.0    |
| Worker       | C#, .NET Worker Service, net10.0     |
| Domain/Core  | C# class library                     |
| Persistence  | EF Core 9, Npgsql, PostgreSQL 16     |
| Storage      | Local filesystem (IObjectStorage)    |
| Queue        | Database polling (IProcessingQueue)  |
| Frontend     | React 18, TypeScript, Vite           |
| Tests        | xUnit, Moq, EF Core InMemory         |
| Containers   | Docker, Docker Compose               |

---

## Repository Structure

```
/
  CloudIngestion.sln
  docker-compose.yml
  .gitignore
  CLAUDE.md
  README.md

  src/
    CloudIngestion.Core/          Domain models, interfaces, DTOs, validation
    CloudIngestion.Infrastructure/ EF Core, storage, queue, repository
    CloudIngestion.Api/           ASP.NET Core Web API
    CloudIngestion.Worker/        Background worker service

  tests/
    CloudIngestion.Core.Tests/    Upload validator tests
    CloudIngestion.Api.Tests/     Controller and upload flow tests
    CloudIngestion.Worker.Tests/  Worker status transition tests

  frontend/
    cloud-ingestion-ui/           React + TypeScript + Vite SPA

  docs/
    architecture.md               Detailed architecture documentation
    path-development-notes.md     PATH harness usage and future slices
```

---

## Local Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 20+](https://nodejs.org/) (for frontend)

---

## Running with Docker Compose

The simplest way to run the full system:

```bash
docker compose up --build
```

This starts:

| Service    | Port |
|------------|------|
| PostgreSQL | 5432 |
| API        | 5000 |
| Worker     | —    |
| Frontend   | 5173 |

Visit `http://localhost:5173` to use the UI.

To run only PostgreSQL (for local .NET development):

```bash
docker compose up postgres
```

---

## Running Locally (without Docker for app services)

### 1. Start PostgreSQL

```bash
docker compose up postgres
```

### 2. Run the API

```bash
dotnet run --project src/CloudIngestion.Api
```

The API starts at `http://localhost:5000`. Migrations are applied automatically in Development mode.

### 3. Run the Worker

```bash
dotnet run --project src/CloudIngestion.Worker
```

The worker polls for pending files every 5 seconds.

### 4. Run the Frontend

```bash
cd frontend/cloud-ingestion-ui
npm install
npm run dev
```

The frontend starts at `http://localhost:5173`. The Vite dev server proxies `/api` requests to `http://localhost:5000`.

---

## Build and Test

### Build the solution

```bash
dotnet build
```

### Run all tests

```bash
dotnet test
```

### Run tests for a specific project

```bash
dotnet test tests/CloudIngestion.Core.Tests
dotnet test tests/CloudIngestion.Api.Tests
dotnet test tests/CloudIngestion.Worker.Tests
```

### Build the frontend

```bash
cd frontend/cloud-ingestion-ui
npm install
npm run build
```

---

## Database Migrations

Migrations are applied automatically when the API or Worker starts in Development mode.

To apply manually:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update \
  --project src/CloudIngestion.Infrastructure \
  --startup-project src/CloudIngestion.Api
```

---

## API Endpoints

| Method | Path                    | Description                        |
|--------|-------------------------|------------------------------------|
| POST   | `/api/files/upload`     | Upload a file                      |
| GET    | `/api/files`            | List all uploaded files            |
| GET    | `/api/files/{id}`       | Get a specific file and its status |
| GET    | `/api/health`           | Health check                       |

### Upload validation

- Allowed extensions: `.png`, `.jpg`, `.jpeg`, `.pdf`, `.mp4`, `.stl`
- Empty files are rejected
- Default max size: 512 MB (configurable via `Upload:MaxFileSizeMb`)

### Example upload

```bash
curl -X POST http://localhost:5000/api/files/upload \
  -F "file=@/path/to/your/file.pdf"
```

---

## Configuration

### API (`src/CloudIngestion.Api/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cloudingestion;Username=postgres;Password=postgres"
  },
  "Storage": {
    "StorageRoot": "uploads"
  },
  "Upload": {
    "MaxFileSizeMb": 512
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

### Worker (`src/CloudIngestion.Worker/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cloudingestion;Username=postgres;Password=postgres"
  },
  "Storage": {
    "StorageRoot": "uploads"
  }
}
```

In Docker Compose, environment variables override these values. The `uploads` volume is shared between the API and Worker containers.

---

## Known Limitations

- No authentication or authorization
- Local filesystem storage only (no S3)
- Database-backed queue only (no SQS)
- No retry logic for failed processing jobs
- No audit event history table
- Worker does not use distributed locking (safe for single-instance deployments)
- Metadata extraction is minimal (size, filename, content type, timestamp)
- Frontend does not auto-poll for status updates
- No production-grade observability or structured logging
- Dockerfiles target .NET 9 runtime (the SDK is .NET 10; use `mcr.microsoft.com/dotnet/sdk:10.0` and `aspnet:10.0` once stable images are available)

---

## Future Roadmap

The following work is planned as PATH-managed implementation slices:

1. Add S3 storage adapter (`S3ObjectStorage`)
2. Add SQS processing queue adapter (`SqsProcessingQueue`)
3. Add integration tests for upload-to-processing lifecycle
4. Add JWT-style authentication
5. Add audit event table
6. Add retry handling for failed processing jobs
7. Add richer metadata extraction
8. Add frontend status polling
9. Add structured logging
10. Add deployment documentation (ECS, RDS, S3, SQS)

See [docs/path-development-notes.md](docs/path-development-notes.md) for full slice definitions.

---

## PATH Development Workflow

After the initial skeleton, future implementation slices are developed through PATH — a scoped execution, validation, and traceability harness for AI-assisted development.

PATH is not an autonomous agent or production framework. It provides controlled execution boundaries, captures validation evidence, and produces reviewable artifacts for each implementation slice.

See [docs/path-development-notes.md](docs/path-development-notes.md) for details.

---

## Agent Guidance

Coding agents should use `CLAUDE.md` as the primary instruction file. This README is the human-facing project guide. If `CLAUDE.md` and this README conflict, follow `CLAUDE.md`.
