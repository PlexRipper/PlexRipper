---
name: reaparr-backend-integration-tests
description: Use when creating, updating, debugging, or stabilizing Reaparr backend integration tests under tests/IntegrationTests, especially when failures involve async jobs, seeded data, fake Plex responses, filesystem behavior, or end-to-end API and database state validation.
---

# Reaparr Backend Integration Tests

## IDE Tool Requirement

**All backend file operations and diagnostics MUST use Rider MCP tools** (`rider_*`, `rider-official-mcp_*`, `rider-index-mcp_*`).

Never use WebStorm MCP tools for any work under `src/` (excluding `ClientApp/`) or `tests/`.

---

## Overview

Use this skill for backend integration tests in Reaparr.

Core principles:
- Fix root causes, not symptoms.
- Never weaken assertions to force green tests.
- No shortcuts: keep working until the full integration suite passes.

## When to Use

Use this skill when:
- Adding or updating tests in `tests/IntegrationTests/IntegrationTests/`.
- Fixing failing backend integration tests.
- Investigating failures that span endpoint + jobs + DB + filesystem + fake Plex API data.

Do not use this skill for:
- Backend unit tests (`tests/UnitTests/`) — use `reaparr-backend-unit-tests`.
- Frontend tests (`Vitest`/`Cypress`).

## Non-Negotiable Rules

- Do not remove, loosen, or bypass assertions to make tests pass.
- Do not mark flaky tests as skipped instead of fixing the root cause.
- Do not stop at targeted-test green.
- Test framework is `TUnit` on Microsoft.Testing.Platform.
- Do not claim completion until this full command passes:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo
```

## Root-Cause Workflow (No Shortcuts)

1) Reproduce failure exactly
- Run the failing test(s) first with a focused filter to iterate quickly.
- Read the real failure message and stack trace fully.

2) Localize failure boundary
- Determine whether failure is in test setup, fake data, endpoint behavior, background job flow, DB state transitions, or filesystem effects.
- Gather concrete evidence (logs, persisted entity state, mocked response payload shape).

3) Fix source of truth
- Change production code, seed data, mock payload shape, or test infrastructure setup where the defect originates.
- Avoid band-aids in test assertions.

4) Keep assertions strict
- Assert exact, deterministic outcomes when possible.
- Add missing side-effect assertions instead of weakening expectations.

5) Verify in two stages
- Stage A: targeted rerun(s) for fast feedback.
- Stage B: mandatory full integration suite run (unfiltered) before done.

## Reaparr Integration Test Harness Patterns

### Base pattern

- Inherit from `BaseIntegrationTests`.
- Create test container via `CreateContainer(...)`.
- Authenticate API client with `await client.SignIn();` before endpoint calls.

Reference files:
- `tests/BaseTests/_Shared/BaseIntegrationTest/BaseIntegrationTests.cs`
- `tests/BaseTests/BaseContainer/BaseContainer.cs`
- `tests/BaseTests/_Shared/ReaparrWebApplicationFactory.cs`

### Container configuration knobs

Use `UnitTestDataConfig` in `CreateContainer(..., config => { ... })`:
- `DatabaseOptions`: seed DB graph via `FakeDataConfig`.
- `BaseMockHttpClientOptions`: generate fake Plex API responses via `PlexApiDataConfig`.
- `HttpClientOptions`: override specific HTTP request/response behavior.
- `FileSystemOptions`: prepare expected source files/directories for filesystem flows.
- `OverrideServices`: override Autofac dependencies for fault-injection scenarios.

Use the narrowest seam that the hosted integration path actually honors:
- Prefer overriding `ICommandExecutor` with `FakeCommandExecutor` when endpoint behavior depends on command dispatch. This is the most reliable way to intercept FastEndpoints command execution in integration tests.

Reference files:
- `tests/BaseTests/_Shared/UnitTestDataConfig.cs`
- `tests/BaseTests/_Shared/Config/Autofac/TestModule.cs`
- `tests/BaseTests/FakeData/FakeDataConfig.cs`
- `tests/BaseTests/FakePlexApiData/PlexApiDataConfig.cs`

### Async stability patterns

- Use `WaitForDatabaseConditionAsync(...)` for eventual DB consistency checks.
- Use `WaitForDownloadStatusAsync(...)` for download status convergence.
- Use `await container.SchedulerService.AwaitScheduler(...)` after scheduling job-driven operations.

Do not replace strict final assertions with long sleeps.

## Test Structure

Every integration test method **must** include the three AAA comment markers — no exceptions:

```csharp
// Arrange

// Act

// Assert
```

Place `// Arrange` before setup/seeding, `// Act` before the HTTP call or operation under test, and `// Assert` before all post-operation checks.

## Assertion Strictness Standard

Prefer strict assertions in this order:

1) API contract
- `response.Response.IsSuccessStatusCode.ShouldBeTrue();`
- `result.IsSuccess.ShouldBeTrue();`

2) Persisted database state
- Exact counts: `ShouldBe(expectedCount)`.
- Exact status transitions: `ShouldBe(DownloadStatus.Completed)`.
- Exact relational side effects (child counts, join-table counts).

3) Runtime side effects
- Scheduler/job completion signals.
- Hub/event payload updates when behavior depends on notifications.
- Filesystem source/destination existence and expected moves.

Weak assertion examples to avoid:
- `ShouldBeGreaterThan(0)` where exact deterministic counts are known.
- Asserting only HTTP 200 without validating DB state.
- Asserting only parent entity without children/media side effects.

## Known Regression Guardrails

### Destination path regressions

Do not hardcode seeded destination folders as root paths like `/Downloads`.
Use `PathProvider.Default*DestinationFolder` conventions for seeded folder paths.

Relevant file:
- `src/Data/ReaparrDBContextSeed.cs`

### Strict Plex SDK payload unions

When fake Plex payloads are generated for integration tests, strict SDK union fields must not serialize to unexpected null shapes.
For fields like `HasVoiceActivity`, `SkipChildren`, and `SkipParent`, generate explicit union values compatible with current SDK expectations.

Relevant files:
- `tests/BaseTests/FakePlexApiData/FakePlexApiData.PlexMediaContainer.cs`
- `tests/BaseTests/FakePlexApiData/GetLibrarySectionsAllResponse/FakePlexApiData.GetLibrarySectionsAllMediaContainer.cs`
- `tests/BaseTests/FakePlexApiData/GetMediaMetaData/FakePlexApiData.MediaMetaDataMediaContainer.cs`

## Verification Commands

### TUnit test filtering

Use `--treenode-filter` (not `--filter`). Syntax: `/<Assembly>/<Namespace>/<Class>/<Test>` — exactly 4 path segments separated by `/`.

**CRITICAL: `[...]` bracket syntax is for property filters only (5th segment).** Never use brackets in the class or test name segments — doing so matches zero tests silently.

| Segment | Position | Example value |
|---------|----------|---------------|
| Assembly | 1st (`/*`) | wildcard always |
| Namespace | 2nd (`/*`) | `Reaparr.IntegrationTests.Api*` |
| Class name | 3rd | `RefreshLibraryMediaEndpointIntegrationTests` or `*Endpoint*` |
| Test name | 4th | `*` or exact method name |
| Property filter | 5th (optional) | `[Category=Smoke]` |

Filter by class name:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/RefreshLibraryMediaEndpointIntegrationTests/*"
```

Filter by class wildcard:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/CheckForUpdate*/*"
```

Filter multiple classes with OR:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/(RefreshLibraryMediaEndpointIntegrationTests)|(CheckForUpdateEndpointIntegrationTests)/*"
```

Filter by namespace prefix:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/Reaparr.IntegrationTests.Api*/*/*"
```

Filter by specific test method:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/*/ShouldReturn200_WhenLibraryRefreshSucceeds"
```

If you are unsure of exact names, list all tests first:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo --list-tests
```

**Anti-pattern (zero tests ran):** `"/*/*/*[CheckForUpdateEndpoint*]"` — this puts bracket property syntax in the class segment. Use `"/*/*/CheckForUpdateEndpoint*/*"` instead.

Mandatory completion gate:

```bash
dotnet run --project tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj -- --no-ansi --disable-logo
```

CI alignment:
- Workflow uses this same project in `.github/workflows/dev-test.yml`.

## Definition of Done

You are done only when all are true:
- Root cause is identified and fixed.
- Assertions remain strict or become stricter.
- Targeted failing tests pass.
- Full integration suite passes unfiltered.
- No skips or assertion weakening introduced to force green.
