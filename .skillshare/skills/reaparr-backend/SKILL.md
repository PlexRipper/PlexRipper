---
name: reaparr-backend
description: Use when doing any Reaparr backend work under src excluding ClientApp, backend tests, backend configs, C# code, FastEndpoints, EF Core, Quartz jobs, SignalR, command handlers, unit tests, integration tests, backend architecture, or backend reviews. This skill must always load before any narrower backend skill.
---

# Reaparr Backend

## Purpose

This skill is mandatory for every Reaparr backend task. Always load `reaparr-backend` before reading, searching, editing, reviewing, debugging, planning, or testing backend code. Do not skip it because a narrower backend skill appears to match; load this skill first, then load the narrower skill.

It gives the shared backend map, mandatory tool rules, verification gates, logging rules, and routing to narrower skills.

Backend work includes code, tests, configuration, plans, reviews, debugging, and refactors under:
- `src/` except `src/AppHost/ClientApp/`
- `tests/UnitTests/`
- `tests/IntegrationTests/`
- backend project files, DI modules, migrations, settings, job config, endpoint contracts, and generated backend API surfaces

Do not use this skill for frontend-only work under `src/AppHost/ClientApp/`; use the Reaparr frontend skills instead.

## Required Tooling

Rider MCP is mandatory for backend work. All backend file operations, searches, symbol inspection, refactors, and diagnostics must use Rider MCP tools first:
- `rider_*`
- `rider-official-mcp_*`
- `rider-index-mcp_*`
- `rider-debugger*` when debugging runtime behavior

Never use WebStorm MCP tools for backend work under `src/` excluding `ClientApp/`, `tests/UnitTests/`, or `tests/IntegrationTests/`.

### Rider MCP retry rule

Rider MCP can briefly hiccup. Do not give up after one failed call.

If a Rider MCP call fails, retry with the same tool once. If it still fails, try a narrower or adjacent Rider MCP tool before falling back:
- File read fails -> try `rider_read_file` or `rider_get_file_text_by_path` with fewer lines.
- Search fails -> try a narrower directory, exact text search, regex search, file-name search, or symbol search.
- Diagnostics fail -> retry the same file, then use Rider indexed file problems, diagnostics, symbol info, or alternate Rider MCP namespaces. Do not run a build to discover errors.

Fallback to filesystem tools only after repeated Rider MCP attempts cannot provide the needed result. State the attempted Rider MCP tools and the fallback reason before using filesystem tools.

## Secondary Skill Routing

Load this skill first, then load narrower skills when the task matches. When editing or creating backend unit tests under `tests/UnitTests/`, `reaparr-backend-unit-tests` is mandatory, even if the production code being tested lives in a tooling project such as `tools/Build`. 

| Task | Load next |
| --- | --- |
| Command records, handlers, validators, command dispatch, `ICommandExecutor` | `reaparr-command-handler-patterns` |
| Backend unit tests under `tests/UnitTests/` | `reaparr-backend-unit-tests` |
| Backend integration tests under `tests/IntegrationTests/` | `reaparr-backend-integration-tests` |
| Broad .NET or C# implementation choices | `dotnet`, `modern-csharp`, or `dotnet-best-practices` |
| ASP.NET Core hosting, middleware, auth, API composition | `aspnet-core` or `dotnet-backend` |
| FastEndpoints endpoint design | `fast-endpoints` |
| EF Core modeling, migrations, query behavior, persistence | `entity-framework-core`, `ef-core`, or `optimizing-ef-core-queries` |
| SignalR hubs, clients, realtime delivery | `signalr` |
| TUnit mechanics or test runner behavior | `tunit` |
| Data access performance | `database-performance` |
| Retry, timeout, circuit breaker, fault tolerance | `resilience-patterns` |

Prefer Reaparr-specific skills over generic skills when both apply.

## Backend Architecture Map

| Area | Role |
| --- | --- |
| `src/AppHost` | Application host, startup/configuration, Autofac composition, FastEndpoints registration, SignalR setup, generated API surface, frontend hosting boundary |
| `src/Application` | Primary use cases, command handlers, orchestration, Plex download flows, API-facing application behavior |
| `src/Application.Contracts` | Cross-project commands, DTO contracts, request/response types consumed outside implementation assemblies |
| `src/Domain` | Domain entities, shared abstractions, FastEndpoints command executor abstractions, domain services and shared primitives |
| `src/Data` | EF Core contexts, persistence configuration, migrations/seeding, database-specific behavior |
| `src/BackgroundJobs` | Quartz jobs and background workflows; jobs dispatch commands and must not throw out of `Execute` |
| `src/BackgroundJobs.Contracts` | Contracts for background job commands and cross-project scheduling requests |
| `src/PlexApi` and contracts | Plex API clients, DTOs, and translation around Plex server/library/media operations |
| `src/External` | External-service integrations and adapters |
| `src/FileSystem` | File and path abstractions, filesystem-side media/download behavior |
| `src/Settings` | Strongly typed settings and configuration models |
| `src/Logging` | Serilog/logging support |
| `src/PublicApi` | Public API-facing contracts and integration boundaries |
| `tests/BaseTests` | Shared unit/integration test infrastructure, seed data, fake Plex data, mock filesystem/http helpers |

## Core Backend Patterns

### Endpoints

- Endpoints inherit from the project-local `BaseEndpoint<TRequest, TResponse>`.
- Do not inherit from raw FastEndpoints `Endpoint<,>` directly.
- Endpoint behavior should usually dispatch through `ICommandExecutor` rather than embedding orchestration logic.

### Dependency Injection

- Autofac modules use `*Module : Module`.
- Register modules through the existing AppHost composition path.
- Prefer existing services and abstractions before introducing new ones.

### Persistence

- EF Core access goes through project contexts/factories and existing repository/service patterns.
- Avoid application-side joins and unbounded queries.
- Prefer no-tracking reads when entities are not being updated.
- Keep query behavior deterministic and testable.

### Background Jobs

- Quartz jobs use `IJob` and should be marked `[DisallowConcurrentExecution]` when concurrent execution is unsafe.
- Pass parameters through `JobDataMap`.
- Dispatch real work through `ICommandExecutor`.
- Jobs must catch, log, and swallow exceptions; do not let exceptions escape `Execute`.

### Realtime

- SignalR uses typed hubs and MessagePack serialization.
- Broadcast through `IHubContext<THub, TClientInterface>.Clients.All` or the narrowest appropriate client target.

### Logging

- Use Serilog `ILogger` according to existing conventions.
- For command handlers and services, prefer assigning contextual loggers with `log.ForContext<T>()` when that is the local pattern.
- Every `_log` usage must include `.Here()` before the log-level call.

Good:

```csharp
_log.Here().Information("Refreshing Plex server access");
_log.Here().Error(ex, "Failed to queue library sync job");
```

Bad:

```csharp
_log.Information("Refreshing Plex server access");
_log.Error(ex, "Failed to queue library sync job");
```

## Code File and Folder Creation Rule

When backend work creates a new code file that requires a new folder, the folder creation is not complete until two things are true:

1. The new code file namespace uses only the owning project's root namespace.
2. The owning project's `.csproj.DotSettings` file has the new folder path registered in `NamespaceFoldersToSkip` with `True`.

Folders should never account for the namespace. Do not append folder names to the namespace of new backend code files.

Example:

If creating a new code file at:

```text
src/AppHost/Startup/HealthChecks/MyHealthCheck.cs
```

and `Startup/HealthChecks` is a new folder path for that project, the code file namespace should stay at the project root namespace:

```csharp
namespace Reaparr.AppHost;
```

Do not use a folder-derived namespace:

```csharp
namespace Reaparr.AppHost.Startup.HealthChecks;
```

Also update the owning project `.csproj.DotSettings` with an entry matching the existing encoding style, for example:

```xml
<s:Boolean x:Key="/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=startup_005Chealthchecks/@EntryIndexedValue">True</s:Boolean>
```

Rules:
- Applies to any backend code file creation that introduces a new folder.
- Applies to production backend projects and backend test projects.
- Use the owning project's `.csproj.DotSettings` file.
- Inspect existing entries in that file and copy the same casing and `_005C` path separator encoding style.
- Do not hardcode project paths from examples; determine the owning project from where the new file is being created.

## Backend Workflow

1. Load `reaparr-backend`; this is not optional for backend work.
2. Confirm the task is backend-scoped and load any narrower matching skills.
3. Use Rider MCP indexed search/symbol tools to find existing precedent.
4. Identify the owning project and, for tests, the matching test project.
5. Make the smallest maintainable change that fixes the root cause.
6. Preserve project boundaries; place shared contracts in `*.Contracts` projects only when cross-project consumption requires it.
7. Keep behavior deterministic, especially in tests and background jobs.
8. Before finishing any backend code file creation, check whether a new folder was introduced. If yes, use the root project namespace in the code file and update the owning `.csproj.DotSettings` `NamespaceFoldersToSkip` entry.
9. Re-read changed files after edits to confirm the intended changes landed.
10. Use Rider MCP intelligence/indexing to find errors that need fixing before claiming completion. Do not run a build as an error-discovery mechanism.

## Run and Test Commands

Do not run `dotnet build` or project build commands for backend error discovery. Use Rider MCP diagnostics/indexing instead. Build-related execution is only allowed as part of running unit or integration tests.

Before running any dotnet related command, TUnit test command, first check whether THE FINALS is running:

```bash
 pgrep -afi 'GameThread|Discovery.exe'
```

If the command returns a matching game process, prefix the backend command with idle-priority I/O scheduling:

```bash
ionice -c 3 <backend-command>
```

If the command returns no matching process, run the backend command normally without `ionice`. On this machine, use `GameThread` as the authoritative process check for the running game. Do not use `ps aux | grep ...`; it can match the `grep` command itself and create a false positive.

Run backend AppHost:

```bash
dotnet run --project src/AppHost
# If THE FINALS is running:
ionice -c 3 dotnet run --project src/AppHost
```

Run a backend unit test project:

```bash
dotnet run --project tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj -- --no-ansi --disable-logo
# If THE FINALS is running:
ionice -c 3 dotnet run --project tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj -- --no-ansi --disable-logo
```

Common unit test projects:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo
dotnet run --project tests/UnitTests/BackgroundJobs.UnitTests/BackgroundJobs.UnitTests.csproj -- --no-ansi --disable-logo
# If THE FINALS is running:
ionice -c 3 dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo
ionice -c 3 dotnet run --project tests/UnitTests/BackgroundJobs.UnitTests/BackgroundJobs.UnitTests.csproj -- --no-ansi --disable-logo
```

Run backend integration tests:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo
# If THE FINALS is running:
ionice -c 3 dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo
```

## Verification Gates

Never rely on `dotnet build`, project builds, or solution builds to determine whether backend code has errors. Error discovery must come from Rider MCP intelligence/indexing.

Required error-checking flow:
1. Run Rider MCP diagnostics/file problems for each changed backend file.
2. If diagnostics are incomplete or fail, retry Rider MCP and use narrower or adjacent Rider indexed tools (`get_file_problems`, diagnostics, symbol info, indexed search, alternate Rider MCP namespace).
3. Fix all relevant Rider-reported errors.
4. Only after Rider MCP reports the changed files are clean, run tests when the change requires behavioral verification.

Build commands are not verification for compile errors in this workflow. Do not run `dotnet build Reaparr.sln` or project builds as a substitute for Rider MCP diagnostics.

Allowed build-related execution:
- Running backend unit tests.
- Running backend integration tests.
- Running a test project may compile as part of test execution; that is acceptable because the purpose is executing tests, not discovering compile errors.

Test routing:
- Command/handler or service changes: Rider MCP diagnostics first, then relevant unit tests when behavior changed.
- Unit test changes: Rider MCP diagnostics first, then the relevant unit test project, preferably filtered first and broader if risk warrants.
- Integration test changes or integration failures: Rider MCP diagnostics first, then targeted integration tests, then the full integration suite before claiming done.
- Cross-project or public contract changes: Rider MCP diagnostics/indexing across affected files and symbols first, then affected tests. Do not use solution build as the error detector.
- Job, SignalR, EF Core, or endpoint changes: Rider MCP diagnostics first, then tests covering the runtime path when available; otherwise explain the missing behavioral verifier.

Do not claim success unless Rider MCP diagnostics/indexing was used and required tests passed. If Rider MCP cannot run in the current environment after retries, state that explicitly instead of running a build to infer errors.

## Common Mistakes

- Skipping this umbrella skill and loading only a narrow backend skill.
- Editing backend unit tests without also loading `reaparr-backend-unit-tests`.
- Testing filesystem behavior with real `File`, `Directory`, temp directories, or host filesystem state instead of `BaseUnitTest.SetupFileSystem(...)` and `MockFileSystem`.
- Treating this skill as optional for small backend changes.
- Using WebStorm MCP tools for backend files.
- Falling back to filesystem tools after one Rider MCP hiccup instead of retrying Rider MCP and trying narrower Rider tools.
- Running `dotnet build` or a project build to discover compile errors instead of using Rider MCP intelligence/indexing.
- Creating backend code files in new folders without keeping the namespace at the project root and updating the owning `.csproj.DotSettings` `NamespaceFoldersToSkip` entry.
- Creating new abstractions before checking existing Reaparr patterns.
- Putting shared command records in implementation projects when they belong in `*.Contracts`.
- Returning raw values or `null` from Result-based command handlers.
- Letting Quartz job exceptions escape.
- Weakening tests or assertions to force green.
- Using `--filter` instead of TUnit `--treenode-filter`.
- Running frontend package managers for backend-only work.
