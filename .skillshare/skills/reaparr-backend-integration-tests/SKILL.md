---
name: reaparr-backend-integration-tests
description: Use when creating, updating, debugging, or stabilizing Reaparr backend integration tests under tests/IntegrationTests, especially when failures involve async jobs, seeded data, fake Plex responses, filesystem behavior, or end-to-end API and database state validation.
---

# Reaparr Backend Integration Tests

## Required First Skill

Load `reaparr-backend` before this skill. It owns shared backend tooling, architecture, build/test commands, and verification gates.

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
- Do not claim completion until `dotnet-test-mcp:run_all_tests_for_project` passes with `projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"`.

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
- When polling for state changes caused by background jobs, prefer a **fresh `IReaparrDbContextFactory.CreateAsync()` context per poll** instead of reusing a long-lived scoped `DbContext`. Shared tracked entities can hide status transitions and make a healthy background flow look stuck.

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

### Filesystem path consistency

Never hardcode seeded filesystem roots for integration tests.
Always derive paths from the active test `PathProvider` defaults.

When seeded entities participate in real filesystem flows (download, move, cleanup, import/export), normalize persisted path metadata to the current integration-test sandbox before assertions. This prevents false failures caused by path drift between fake-data defaults and runtime sandbox roots.

Reference points:
- `src/Data/ReaparrDBContextSeed.cs`
- `tests/BaseTests/MockDatabase/`

### Generated payload shape compatibility

When integration tests rely on generated API payloads, ensure generated JSON shape is compatible with the strict contract expected by the consuming SDK/client models.

Do not rely on implicit/null union coercion. Prefer explicit, valid values for contract-sensitive fields, especially discriminated/union-like members.

Reference area:
- `tests/BaseTests/FakePlexApiData/`

### Async/polling diagnosis

If a focused integration run appears to hang or time out, do not assume deadlock immediately. First:

1. rerun only the failing class/test with `dotnet-test-mcp:run_all_tests_in_class` or `dotnet-test-mcp:run_single_test`
2. inspect generated `TestResults/*.diag` artifacts for timeout/failure telemetry
3. distinguish real backend non-completion from stale test-side observation

When polling for state changes, prefer fresh `DbContext` instances per poll to avoid stale tracked state masking real transitions.

## Integration Test Verification

All integration test execution goes through `dotnet-test-mcp` tools. Do not use terminal-style `dotnet run --project` or `dotnet test`.

For integration test work:
- Start with `dotnet-test-mcp:run_all_tests_in_class` when reproducing or iterating on a specific failing class.
- Use `dotnet-test-mcp:run_single_test` for a single failing test method.
- Use `dotnet-test-mcp:list_tests_summary` with `includeTests: true` if exact test/class names are unknown.
- Do not stop at targeted-test green.
- Mandatory completion gate for any integration test change or integration failure fix:

```
dotnet-test-mcp:run_all_tests_for_project
  projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"
```

CI alignment:
- CI workflow uses this same project in `.github/workflows/dev-test.yml` (via `dotnet-test-mcp`).

### Test discovery and execution via dotnet-test-mcp

All test filtering maps to `dotnet-test-mcp` tools. Do not use `--treenode-filter` or `--list-tests` shell arguments.

| Goal | `dotnet-test-mcp` tool | Key parameter |
|------|------------------------|---------------|
| Discover test/class names | `list_tests_summary` | `prefix`, `projectPath`, `includeTests: true` |
| Run single test method | `run_single_test` | `testName: "Fully.Qualified.ClassName.MethodName"` |
| Run all tests in a class | `run_all_tests_in_class` | `className: "Fully.Qualified.ClassName"` |
| Run entire integration project | `run_all_tests_for_project` | `projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"` |

Examples:

Run a test class:
```
dotnet-test-mcp:run_all_tests_in_class
  className: "Reaparr.IntegrationTests.Api.RefreshLibraryMediaEndpointIntegrationTests"
  projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"
```

Run a single test method:
```
dotnet-test-mcp:run_single_test
  testName: "Reaparr.IntegrationTests.Api.RefreshLibraryMediaEndpointIntegrationTests.ShouldReturn200_WhenLibraryRefreshSucceeds"
  projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"
```

Discover test names by namespace prefix:
```
dotnet-test-mcp:list_tests_summary
  prefix: "Reaparr.IntegrationTests.Api"
  projectPath: "tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj"
  includeTests: true
```

## Test Quality Gate

Do not write tests merely to reach a requested count. A number like "add 20 tests" is a budget or lower bound, not the success criterion. First map the code under test, identify high-risk behavior, and choose tests that would catch meaningful regressions. If the requested count would force low-value tests, stop and report the highest-value test plan instead of padding.

Before adding tests, inspect existing tests for the same class and explicitly avoid duplicate coverage. Prefer behavior that crosses boundaries or encodes contracts:
- API request parameter contracts and omitted/default parameters
- edge cases that previously failed or could plausibly regress

Reject weak tests such as:
- default value assertions that do not protect a behavior contract
- direct setter/getter tests with no observable consequence
- duplicating existing tests with different wording
- assertions that only prove mocks were configured
- broad "kitchen sink" tests added to inflate count

Every new test must earn its place by answering: "What bug would this fail for?" If the answer is unclear, replace it with a stronger test or do not add it.


## Definition of Done

You are done only when all are true:
- Root cause is identified and fixed.
- Assertions remain strict or become stricter.
- Targeted failing tests pass.
- Full integration suite passes unfiltered.
- No skips or assertion weakening introduced to force green.
