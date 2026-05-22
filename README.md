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
