---
name: reaparr-backend-unit-tests
description: Use when creating or updating C# backend unit tests in Reaparr, especially for handlers, services, endpoints, and jobs that must follow the project's TUnit, Shouldly, Moq, BaseUnitTest, naming, placement, and deterministic test-data conventions.
---

# Reaparr Backend Unit Tests

## Required First Skill

Load `reaparr-backend` before this skill. It owns shared backend tooling, architecture, build/test commands, and verification gates.

## Overview

Use this skill to write backend unit tests that match Reaparr conventions exactly.

The goal is consistency and reliability:
- Keep tests deterministic.
- Match project structure and naming.
- Reuse BaseTests helpers instead of ad-hoc setup.
- Verify behavior and side effects (especially database state and mock calls).

## When to Use

Use this skill when:
- You are writing or modifying C# unit tests under `tests/UnitTests/`.
- The system under test is in backend projects (Application, BackgroundJobs, Data, Domain, External, FileSystem, FluentResultExtension, Logging, PlexApi, PublicApi, Settings).
- You are testing handlers, services, jobs, endpoints, or command logic.

Do not use this skill for frontend tests (Vitest/Cypress).

## Required Frameworks and Style

- Test framework: `TUnit` with `[Test]`, `[Arguments]`, and `async Task` where needed.
- Assertions: `Shouldly`.
- Mocks: `Moq` with explicit verification (`Times.Once()` / `Times.Never()`).
- Structure: Arrange -> Act -> Assert. Every test method **must** include the three comment markers `// Arrange`, `// Act`, and `// Assert` — no exceptions. Within Arrange, mock setups (`Mock.Mock<T>()`) must always be the **last step**, immediately before Act.
- Determinism: no random behavior in tests.
- Command-handler tests must derive from `BaseCommandUnitTest<TCommand>` and call `TestHandlerExecuteAsync(...)` in Act. Do **not** call `Sut.ExecuteAsync(...)` directly for command handlers; `TestHandlerExecuteAsync` runs the matching validator first and then executes the handler, so validator and handler behavior are tested together.

## Base Test Helpers

Prefer the shared `BaseUnitTest` helpers over manual container or SUT construction. For command handlers, prefer `BaseCommandUnitTest<TCommand>` over `BaseUnitTest<THandler>` so `TestHandlerExecuteAsync(...)` exercises validation automatically.

Before adding any test-local helper or custom setup method, inspect `tests/BaseTests/_Shared/BaseUnitTest/*` and existing `tests/BaseTests/*` utilities first. Reuse an existing helper when one already fits. Do not create ad-hoc test-class helpers for behavior already covered by `BaseUnitTest`, such as app build info setup, dependency overrides, filesystem setup, environment-variable scoping, or SUT creation.

- Use `SetupDatabase(...)` for database state.
- Use `SetupFileSystem(...)` for filesystem state only.
- Use `SetupDependencies(...)` when a test needs to replace a DI registration without overloading an unrelated helper.
- Use `SetAppBuildInfo(...)` for build/version metadata instead of constructing custom handlers or endpoints manually.

### Filesystem and dependency setup

Keep filesystem setup and DI overrides separate:

```csharp
SetupDependencies(builder => builder.RegisterInstance<IUserSettings>(new UserSettings()));
SetupFileSystem(system =>
{
    system.AddDirectory(configDirectory);
    system.AddFile(configPath, new MockFileData("{}"));
});

var result = Sut.Setup();
```

Do not hide dependency overrides inside `SetupFileSystem(...)`. If a test needs a real service instance, register it explicitly with `SetupDependencies(...)`.

### Sandbox path rule

Do not hard-code config, database, or download paths in backend unit tests when the test uses `BaseUnitTest` helpers.

Prefer `IPathProvider` values resolved from the test container:

```csharp
var configPath = Mock.Container.Resolve<IPathProvider>().ConfigFileLocation;
var databasePath = Mock.Container.Resolve<IPathProvider>().DatabasePath;
```

`BaseUnitTest` uses a sandboxed `MockPathProvider`, so assertions like `"/config/..."` or `"/Config/..."` are brittle and should be avoided.

### Real settings object rule

When testing `ConfigManager.Setup()` or any path that can save settings, prefer a real `UserSettings` instance over a strict `IUserSettings` mock.

Reason:
- `Setup()` can trigger config save and serialization.
- A strict mock often forces a large amount of irrelevant property setup.
- A real `UserSettings` is simpler, more realistic, and more maintainable.

Use mocks for `IUserSettings` only when the test is explicitly asserting `Reset()`, `UpdateSettings(...)`, `SettingsUpdated`, or other interaction behavior.

### App build info rule

If a test changes app version or release channel, call `SetAppBuildInfo(...)` before Act and prefer it over manual container rewiring.

`SetAppBuildInfo(...)` now updates both:
- the current resolved `MockAppBuildInfo` instance for already-created SUTs
- the registration used for future container rebuilds

This avoids stale version metadata when a test resolves `Sut` before changing app build info.

### Strong typing over stringly test helpers

Prefer domain types, enums, and value objects in test helper parameters and `[Arguments(...)]` data.

Good:

```csharp
[Arguments(PlexMediaType.Movie)]
[Arguments(PlexMediaType.TvShow)]
private static string GetMediaDestinationFolder(PathProvider sut, PlexMediaType mediaType) => ...
```

Bad:

```csharp
[Arguments("Movies")]
[Arguments("TvShows")]
private static string GetMediaDestinationFolder(PathProvider sut, string mediaType) => ...
```

Rules:
- Prefer `PlexMediaType`, `DownloadTaskType`, IDs, and other project types over string literals when the production API already has a typed representation.
- Avoid stringly-typed switches in tests when an enum or typed model exists.
- If the test data must model parsing raw strings, keep that explicit in the test name and assertions.

### Prefer `Sut` before custom construction

If `BaseUnitTest<TSUT>` can construct the subject correctly, use `Sut` instead of adding a local `CreateSut(...)` helper.

Only add a local SUT factory when one of these is true:
- the test must pass constructor parameters that `AutoMock` cannot infer cleanly
- the test intentionally bypasses container wiring to validate raw constructor behavior
- there is no existing `BaseUnitTest` helper that covers the setup

If you think you need a local SUT helper, first check whether `SetAppBuildInfo(...)`, `SetupDependencies(...)`, `SetupFileSystem(...)`, or another `BaseUnitTest` helper already solves it.

### Environment override ordering

When a test uses `WithEnvironmentVariablesAsync(...)` and `SetAppBuildInfo(...)`, complete both in Arrange before first reading `Sut` or any derived property.

Good:

```csharp
using var _ = WithEnvironmentVariablesAsync(new Dictionary<string, string?>
{
    [EnvKeys.ReaparrDataPath] = "/custom/data",
});
SetAppBuildInfo(x => x.RuntimeMode = "desktop");
var sut = Sut;
```

Bad:

```csharp
var sut = Sut;
SetAppBuildInfo(x => x.RuntimeMode = "desktop");
```

This keeps test setup deterministic and avoids reading stale container state.

## Test Structure

Follow this exact order within every test method:

```
// Arrange — data and context first
var dbContext = IDbContext;
await SetupDatabase(seed, config => { ... });
// ... any other data setup ...

// Arrange — mocks last (always immediately before Act)
Mock.Mock<IFoo>()
    .Setup(x => x.Bar())
    .Returns(someValue)
    .Verifiable(Times.Once());

// Act
var result = await Sut.Handle(command, CancellationToken);

// Assert
result.ShouldBeSuccess();
// ... DB state checks ...
Mock.Mock<IFoo>().Verify();
```

**Rules:**
- Seed data, build commands/DTOs, and any other context setup come **before** mock setups.
- `Mock.Mock<T>()` setup blocks are the **last thing in Arrange**, right before the Act line.
- Never interleave mock setups with data setup.

## BaseTests.csproj Utilities

`tests/BaseTests/BaseTests.csproj` is the shared backend test toolkit. It is intentionally broad so unit test projects can reuse realistic helpers instead of re-implementing setup.

Key package-backed utilities:
- `Autofac` + `Autofac.Extras.Moq`: strict `AutoMock` container composition and dependency injection for SUT creation.
- `Bogus` + `Bogus.Hollywood`: deterministic fake domain data via shared faker extensions and datasets.
- `Moq` + `Moq.Contrib.HttpClient`: strict mocks plus concise `HttpMessageHandler` request/response setup.
- `Shouldly`: readable assertions used across all test projects.
- `TUnit` + Microsoft.Testing.Platform: project test runtime and discovery.
- `TestableIO.System.IO.Abstractions.TestingHelpers`: `MockFileSystem` support through `SetupFileSystem`.
- `FastEndpoints.Testing` + `Microsoft.AspNetCore.Mvc.Testing`: endpoint and application-host test helpers used by shared test infrastructure.

Project reference utility:
- `ProjectReference -> src/AppHost/AppHost.csproj` makes the full backend composition available to shared test helpers (endpoints, DI modules, contracts, defaults).

Common reusable helpers exposed from `tests/BaseTests/`:
- `BaseUnitTest` (`_Shared/BaseUnitTest/*`): strict mock container, logging setup, cancellation token, DB setup (`SetupDatabase`), and filesystem/http setup hooks.
- `BaseCommandUnitTest<TCommand>`: executes command validator + inferred command handler via `TestHandlerExecuteAsync`, reducing boilerplate command tests.
- `MockDatabase` (`MockDatabase/*`): in-memory SQLite contexts (`ReaparrDbContext` + `AuthDbContext`) and seeded graph setup from `FakeDataConfig`.
- `FakeData` (`FakeData/*`): deterministic entity and download-task builders for domain/database seeding.
- `FakePlexApiData` (`FakePlexApiData/*`): deterministic Plex API payload/response builders for HTTP-level testing.
- `MockPlexApiServer` (`MockPlexServer/MockPlexApiServer.cs`): end-to-end mocked Plex server behavior over `HttpMessageHandler`.
- `MoqExtensions` (`_Shared/Extensions/MoqExtensions.cs`): helper setup/verify extensions for commands, events, notifications, and HTTP request matching.
- HTTP test helpers: `TestHttpClientExtensions` (sign-in helper) and `HttpResponseMessageExtensions` (typed DTO deserialization).
- `Seed` + config objects (`Seed`, `FakeDataConfig`, `PlexApiDataConfig`) for repeatable, explicit test data generation.

## Project Placement Rules

- Place tests in the `*.UnitTests` project matching the SUT project.
- Handler location controls test project placement (not command record location).
- Folder layout should mirror the SUT file layout.
- Namespace must be exactly `<SUTProjectNamespace>.UnitTests`.

Examples:
- SUT in `src/BackgroundJobs/...` -> test in `tests/UnitTests/BackgroundJobs.UnitTests/...`
- SUT in `src/Application/...` -> test in `tests/UnitTests/Application.UnitTests/...`

## Naming Rules

- Test file: `<SutFileName>.UnitTests.cs`
- Test class: `<SutFileName>UnitTests`
- Test method: `ShouldExpectedBehavior_WhenCondition`

## Base Test Infrastructure (Required)

- Inherit from `BaseUnitTest<TSUT>`.
- Use provided members: `Sut`, `IDbContext`, `Mock`, `CancellationToken`.
- Reuse one DB context variable per test:
  - `var dbContext = IDbContext;`
- Seed data through:
  - `await SetupDatabase(seed, config => { ... });`
- Resolve mocks through:
  - `Mock.Mock<IFoo>()`

## Data and Builder Rules

- Prefer existing builders/helpers from `tests/BaseTests`.
- Do not instantiate Bogus/Faker directly inside tests unless done through BaseTests helpers.
- If a new test-data pattern is needed, extend BaseTests helpers rather than adding local per-test random generators.

## Mock Rules

- **Mocks return the expected type only.** Never put real business logic, DB writes, or side effects inside mock callbacks. If a side effect needs to be verified, use `Verifiable` — do not secretly implement it in a `.Returns(...)` callback.
- **Never simulate production state changes inside mocks.** If a mocked collaborator would normally update DB state, dispatch status transitions, queue work, or publish downstream side effects, do not reproduce that behavior in a callback/delegate. Return the expected `Result` only and verify the interaction contract instead.
- **For mocked side-effecting collaborators, assert exact call contracts.** Prefer `It.Is<...>(...)` for important parameters and `Verifiable(Times.X())` and/or `Verify(..., Times.X())` for call counts rather than relying on mocked callbacks to make later assertions pass.
- **Do not make database assertions that depend on mocked dependencies having executed real logic.** If the dependency is mocked, assert the SUT called it with the right values. Only assert persisted downstream state when the real implementation is part of the test.
- **Mock setups must be inline per test.** Do not extract them into shared helper methods or place them in the test class constructor. Constructor-level mock configuration is an anti-pattern because it hides per-test expectations and makes tests harder to read and reason about. Each test must be self-contained and readable without jumping elsewhere to understand what is mocked.
- **Every mock setup must end with `.Verifiable(Times.X())` using the exact expected invocation count.** Do not rely on broad shared setups or unstated defaults. Declare the precise number of calls on each mock inside the test that owns that expectation so unmet or extra invocations fail clearly.

Bad:

```csharp
Mock.Mock<IDownloadTaskUpdateDispatcher>()
    .Setup(x => x.OnStatusChangedAsync(...))
    .Returns<DownloadTaskKey, DownloadStatus, CancellationToken>(async (key, _, _) =>
    {
        await dbContext.SetDownloadStatus(key, DownloadStatus.Completed);
        return Result.Ok();
    });
```

Good:

```csharp
Mock.Mock<IDownloadTaskUpdateDispatcher>()
    .Setup(x =>
        x.OnStatusChangedAsync(
            It.Is<DownloadTaskKey>(k => k == expectedKey),
            It.Is<DownloadStatus>(s => s == DownloadStatus.Completed),
            It.IsAny<CancellationToken>()
        )
    )
    .ReturnsAsync(Result.Ok())
    .Verifiable(Times.Once());
```

## Assertion Requirements

Always verify both:
- Operation result (`Result`/`Result<T>` success or failure path).
- Relevant side effects: DB state when the real dependency writes to it, or `Verify` on the mock when the SUT delegates the write to a mocked dependency.

Also verify expected mock interactions explicitly; do not leave mocks unverified.

When a dependency is mocked, prefer verifying exact interaction parameters and call counts over asserting downstream state that only the real dependency would have produced.

### Critical-path strictness rules (mandatory)

For critical workflows (download lifecycle, restart/stop/pause/start, queue progression), every test must include multiple assertions per case and must not rely on a single boolean assertion.

Minimum strictness for command/handler tests on critical paths:
- Assert outcome shape (`IsSuccess`/`IsFailed`) **and** error count semantics (`Errors.Count == 0` for success, `> 0` for failure).
- Assert final persisted `DownloadStatus` for affected entities whenever the test wiring makes persistence observable.
- Assert interaction contracts (`Times.Once`/`Times.Never`) for key collaborators (`ICommandExecutor`, dispatcher, event publisher).
- Assert parent/child invariants where relevant (e.g., parent status transition + each child terminal status).

If status transitions are delegated to `IDownloadTaskUpdateDispatcher`, and you need strict DB status assertions, use a deterministic callback in test Arrange:

```csharp
Mock.Mock<IDownloadTaskUpdateDispatcher>()
    .Setup(x => x.OnStatusChangedAsync(It.IsAny<DownloadTaskKey>(), It.IsAny<DownloadStatus>(), It.IsAny<CancellationToken>()))
    .Returns(async (DownloadTaskKey key, DownloadStatus status, CancellationToken _) =>
    {
        await IDbContext.SetDownloadStatus(key, status);
    });
```

This is allowed only when the explicit goal of the test is validating persisted status outcomes from dispatched transitions.

### Mapping assertions for remap/refresh logic

When a handler reconstructs/remaps entities (e.g., restart refresh for Movie/Episode tasks), tests must assert mapping correctness, not only success status.

At minimum assert:
- identity/link invariants preserved (Id, ParentId, media/part IDs, destination path IDs, hash IDs),
- reset invariants applied (transfer counters/speeds/time remaining reset to zero, expected status),
- directory metadata rules (expected roots/folders preserved or recomputed),
- content/title invariants (e.g., file name non-empty, full title contains file name).

Include at least one MovieData mapping test and one EpisodeData mapping test for restart-critical flows.

## Special Constraints and Gotchas

### BackgroundJobs.UnitTests references

- `BackgroundJobs.UnitTests` can reference `BackgroundJobs` and `BaseTests` only.
- Do not add direct references to `Application` or `Application.Contracts`.
- `PlexApi.Contracts` types are available transitively.

### File system tests

- If SUT touches filesystem, use `SetupFileSystem` (MockFileSystem).
- Do not manually mock `System.IO.Abstractions` interfaces in test files.

### Endpoint unit tests

Endpoint tests use endpoint-specific base classes from `tests/BaseTests/_Shared/BaseEndpointUnitTest/BaseEndpointUnitTest.cs`. Put the endpoint/request/response types on the test class once, then call `TestEndpointHandleAsync(...)` without method-level endpoint generics.

- **Request endpoints:** inherit `BaseEndpointUnitTest<TEndpoint, TRequest, TResponse>`.
- **No-request endpoints:** inherit `BaseEndpointWithoutRequestUnitTest<TEndpoint, TResponse>`.
- **Do not** inherit plain `BaseUnitTest<TEndpoint>` for endpoint tests unless intentionally bypassing endpoint helper behavior.
- **Do not** call endpoint `HandleAsync(...)` directly in normal endpoint unit tests; direct calls bypass validator execution.
- `TestEndpointHandleAsync(TRequest request, Action<IServiceCollection>? extraServices = null)` validates request endpoints before execution and skips `HandleAsync(...)` when validation fails.
- `TestEndpointHandleAsync(Action<IServiceCollection>? extraServices = null)` executes no-request endpoints.
- The helper returns `EndpointUnitTestResult<TEndpoint, TResponse>` with `Endpoint`, nullable typed `Result`, `RequiredResult`, `ValidationResult`, `HasValidator`, and `IsValid`.
- `Result` is nullable because invalid validation intentionally does not execute the endpoint. Use `RequiredResult` only after asserting/knowing the endpoint executed.
- `TResponse` must match the actual runtime `endpoint.Response` type. Use `BaseResultDTO` for endpoints that send non-generic command results, even if OpenAPI documents `ResultDTO<T>` for success. Use `ResultDTO<T>` only when the endpoint actually sets that runtime response type.
- `SetupEndpointUnitTest<T>()` remains the lower-level factory. Use it only when a test explicitly needs to bypass the validation-aware helper.
- `SetupEndpointUnitTest<T>()` provides a real in-memory `IReaparrDbContext` and registers: `ILogger`, `IReaparrDbContext`, `IReaparrDbContextFactory`, `IAuthDbContext`, `IAuthDbContextFactory`, `ICommandExecutor`, `ISchedulerService`, `IProgressHubService`, `IDownloadHubService`, `INotificationHubService`, `IDownloadTaskScheduler`.
- Avoid mocking `IReaparrDbContext` in endpoint tests unless intentionally re-registering a mock.
- **Endpoints with non-standard dependencies** (e.g. `UpdateManager`, custom services not in the list above): pass `extraServices` to `TestEndpointHandleAsync(...)` — never call `Factory.Create<T>` directly. `ILogger` is always registered by `SetupEndpointUnitTest`, so only add what is missing.

Request endpoint example:

```csharp
public class RefreshLibraryMediaEndpointUnitTests
    : BaseEndpointUnitTest<RefreshLibraryMediaEndpoint, RefreshLibraryMediaEndpointRequest, BaseResultDTO>
{
    [Test]
    public async Task ShouldReturnSuccess_WhenLibrarySyncJobQueued()
    {
        // Arrange
        var request = new RefreshLibraryMediaEndpointRequest(plexLibrary.Id);

        Mock.SetupCommand(It.IsAny<QueueLibrarySyncJobCommand>())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);
        var result = endpointResult.RequiredResult;

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
```

No-request endpoint with extra services:

```csharp
public class ApplyUpdateEndpointUnitTests
    : BaseEndpointWithoutRequestUnitTest<ApplyUpdateEndpoint, BaseResultDTO>
{
    [Test]
    public async Task ShouldReturnFailure_WhenDockerMode()
    {
        // Arrange
        SetAppBuildInfo(x => x.RuntimeMode = "docker");
        var mockManager = new Mock<UpdateManager>(mockSource.Object, null!, mockLocator.Object);

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            extraServices: s => s.AddSingleton(_ => mockManager.Object)
        );
        var result = endpointResult.RequiredResult;

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}
```

Validation failure example:

```csharp
var endpointResult = await TestEndpointHandleAsync(new RefreshLibraryMediaEndpointRequest(0));

endpointResult.IsValid.ShouldBeFalse();
endpointResult.ValidationResult.ShouldNotBeNull();
endpointResult.Result.ShouldBeNull();
Mock.Mock<ICommandExecutor>().Verify(
    x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()),
    Times.Never
);
```

### Static abstract settings interfaces

- Interfaces like `ISonarrSettings`/`IRadarrSettings` cannot be mocked with Moq.
- Inject concrete settings instances via Autofac `TypedParameter` when constructing SUT.

Example:

```csharp
var sut = Mock.Create<MyHandler>(
    new TypedParameter(typeof(ISonarrSettings), new SonarrSettings { ... }),
    new TypedParameter(typeof(IIntegrationsSettings), IntegrationsSettings.Create())
);
```

## Unit Test Workflow

1. Identify SUT location under `src/`.
2. Place the test in matching `tests/UnitTests/<Project>.UnitTests/` path.
3. Name file/class/methods with project naming conventions.
4. Inherit `BaseUnitTest<TSUT>` and prepare deterministic arrange step.
5. Execute SUT method once in Act section.
6. Assert result + database state + mock interactions.
7. Run the specific test class first with `dotnet-test-mcp:run_all_tests_in_class`, then broaden to `dotnet-test-mcp:run_all_tests_for_project` if needed.

## Unit Test Verification

All unit test execution goes through `dotnet-test-mcp` tools. Do not use terminal-style `dotnet run --project` or `dotnet test`.

For unit test work:
- Start with `dotnet-test-mcp:run_all_tests_in_class` for fast targeted iteration on a specific test class.
- Use `dotnet-test-mcp:run_single_test` to run a single test method.
- Use `dotnet-test-mcp:list_tests_summary` with `includeTests: true` to discover test names when unsure.
- Broaden to `dotnet-test-mcp:run_all_tests_for_project` before claiming completion when behavior or shared test infrastructure changed.

### Verification fallback when execution environment is constrained

If test execution is blocked by environment constraints (for example, read-only obj writes), do not claim runtime pass. Instead:
- run Rider file problem checks and ensure zero errors in changed test files,
- state the exact execution blocker and raw error message,
- keep assertions strict and deterministic so rerun is straightforward once the environment is fixed.

Evidence-before-assertion rule:
- fixed compile or symbol issues may be claimed only with zero Rider file problems,
- tests pass may be claimed only with completed test execution output.

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


### Test discovery and execution via dotnet-test-mcp

All test filtering maps to `dotnet-test-mcp` tools. Do not use `--treenode-filter` or `--list-tests` shell arguments.

| Goal | `dotnet-test-mcp` tool | Key parameter |
|------|------------------------|---------------|
| List test projects | `list_test_projects` | `workingDirectory` |
| Discover test/class names | `list_tests_summary` | `prefix`, `projectPath`, `includeTests: true` |
| Run single test method | `run_single_test` | `testName: "Fully.Qualified.ClassName.MethodName"` |
| Run all tests in a class | `run_all_tests_in_class` | `className: "Fully.Qualified.ClassName"` |
| Run entire test project | `run_all_tests_for_project` | `projectPath: "tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj"` |
| Run all tests in solution | `run_all_tests` | `workingDirectory` |

Examples:

Run a single test class:
```
dotnet-test-mcp:run_all_tests_in_class
  className: "Reaparr.Application.UnitTests.PlexDownloads.DownloadJobUnitTests"
  projectPath: "tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj"
```

Run a single test method:
```
dotnet-test-mcp:run_single_test
  testName: "Reaparr.Application.UnitTests.PlexDownloads.DownloadJobUnitTests.ShouldSetDownloadClientErrorStatus_WhenClientStartFailsWithoutSpecificError"
  projectPath: "tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj"
```

Discover test names by namespace prefix:
```
dotnet-test-mcp:list_tests_summary
  prefix: "Reaparr.Application.UnitTests.PlexDownloads"
  projectPath: "tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj"
  includeTests: true
```

## Common Mistakes

- Using `[ClassName*]` bracket syntax — this is a TUnit `--treenode-filter` shell-command antipattern that causes "Zero tests ran". Use `dotnet-test-mcp:run_all_tests_in_class` with the fully qualified class name instead.
- Putting tests in the wrong `*.UnitTests` project because of command location instead of handler location.
- Using folder-based namespaces instead of `<SUTProjectNamespace>.UnitTests`.
- Asserting only return values and not checking database state or mock interactions.
- Using unverified mocks or loose mock expectations.
- Using random/non-deterministic test data.
- Mocking settings interfaces with static abstract members.
- Extracting mock setups into shared helper methods — keep all mock configuration inline per test.
- Hiding real logic (DB writes, status updates) inside mock callbacks instead of returning the expected type and verifying with `Verify`.
- Making post-Act DB assertions that only pass because a mocked dependency performed production logic in a callback.
- Placing `Mock.Mock<T>()` setups before data setup or mixed in with DB seeding — mock setups must always be the last step of Arrange.
- Using non-generic `BaseUnitTest` for endpoint tests — always use `BaseUnitTest<TEndpoint>` even when `Sut` is not directly referenced.
- Direct-casting `endpoint.Response` to `ResultDTO<T>` — use `as ResultDTO<T>` (null-safe) and assert non-null, not `(ResultDTO<T>)endpoint.Response`.
- Calling `Factory.Create<T>` directly for endpoint tests — always use `SetupEndpointUnitTest<T>()` instead. For non-standard dependencies, pass the `extraServices` parameter: `SetupEndpointUnitTest<MyEndpoint>(s => s.AddSingleton(_ => mockDep.Object))`. Never manually register `ILogger` — `SetupEndpointUnitTest` handles it.
