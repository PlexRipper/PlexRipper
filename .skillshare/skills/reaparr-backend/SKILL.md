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
- Diagnostics fail -> retry the same file, then try project build diagnostics or file problems through the alternate Rider MCP namespace if available.

Fallback to filesystem tools only after repeated Rider MCP attempts cannot provide the needed result. State the attempted Rider MCP tools and the fallback reason before using filesystem tools.

## Secondary Skill Routing

Load this skill first, then load narrower skills when the task matches:

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
10. Run Rider diagnostics and the narrowest meaningful verifier before claiming completion.

## Build and Test Commands

Backend build:

```bash
dotnet build Reaparr.sln
```

Run backend AppHost:

```bash
dotnet run --project src/AppHost
```

Run a backend unit test project:

```bash
dotnet run --project tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj -- --no-ansi --disable-logo
```

Common unit test projects:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo
dotnet run --project tests/UnitTests/BackgroundJobs.UnitTests/BackgroundJobs.UnitTests.csproj -- --no-ansi --disable-logo
```

Run backend integration tests:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo
```

## Verification Gates

Always run Rider diagnostics first for changed backend files.

Then choose the narrowest meaningful verifier:
- Command/handler or service changes: relevant unit tests first, then broader project tests or build if needed.
- Unit test changes: run the relevant unit test project, preferably filtered first and then broader if risk warrants.
- Integration test changes or integration failures: run targeted integration tests first, then the full integration suite before claiming done.
- Cross-project or public contract changes: run affected tests plus `dotnet build Reaparr.sln` when compile impact is broader than one project.
- Job, SignalR, EF Core, or endpoint changes: verify with tests covering the runtime path when available; otherwise explain the missing verifier and run build/diagnostics.

Do not claim success unless verification was run and passed. If a verifier cannot run in the current environment, say so explicitly.

## Common Mistakes

- Skipping this umbrella skill and loading only a narrow backend skill.
- Treating this skill as optional for small backend changes.
- Using WebStorm MCP tools for backend files.
- Falling back to filesystem tools after one Rider MCP hiccup instead of retrying Rider MCP and trying narrower Rider tools.
- Creating backend code files in new folders without keeping the namespace at the project root and updating the owning `.csproj.DotSettings` `NamespaceFoldersToSkip` entry.
- Creating new abstractions before checking existing Reaparr patterns.
- Putting shared command records in implementation projects when they belong in `*.Contracts`.
- Returning raw values or `null` from Result-based command handlers.
- Letting Quartz job exceptions escape.
- Weakening tests or assertions to force green.
- Using `--filter` instead of TUnit `--treenode-filter`.
- Running frontend package managers for backend-only work.
