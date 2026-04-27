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

## Base Test Helpers

Prefer the shared `BaseUnitTest` helpers over manual container or SUT construction.

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

## Special Constraints and Gotchas

### BackgroundJobs.UnitTests references

- `BackgroundJobs.UnitTests` can reference `BackgroundJobs` and `BaseTests` only.
- Do not add direct references to `Application` or `Application.Contracts`.
- `PlexApi.Contracts` types are available transitively.

### File system tests

- If SUT touches filesystem, use `SetupFileSystem` (MockFileSystem).
- Do not manually mock `System.IO.Abstractions` interfaces in test files.

### Endpoint unit tests

- **Always inherit `BaseUnitTest<TEndpoint>`** — the generic form, with the endpoint as the type parameter. Do not use non-generic `BaseUnitTest` for endpoint tests even if `Sut` is not directly used; the generic form is the project standard.
- `SetupEndpointUnitTest<T>()` provides a real in-memory `IReaparrDbContext` and registers: `ILogger`, `IReaparrDbContext`, `IReaparrDbContextFactory`, `IAuthDbContext`, `IAuthDbContextFactory`, `ICommandExecutor`, `ISchedulerService`, `IProgressHubService`, `IDownloadHubService`, `INotificationHubService`, `IDownloadTaskScheduler`.
- Avoid mocking `IReaparrDbContext` in endpoint tests unless intentionally re-registering a mock.
- **Endpoints with non-standard dependencies** (e.g. `UpdateManager`, custom services not in the list above): pass an `extraServices` action to `SetupEndpointUnitTest<T>()` — never call `Factory.Create<T>` directly. `ILogger` is always registered by `SetupEndpointUnitTest`, so only add what is missing:

```csharp
var endpoint = SetupEndpointUnitTest<MyEndpoint>(s =>
    s.AddSingleton(_ => mockCustomDep.Object)
);
```

- **Accessing typed response DTOs**: `endpoint.Response` is declared as `BaseResultDTO`. For endpoints returning `ResultDTO<T>`, use a null-safe `as` cast — never a direct cast:

```csharp
var result = endpoint.Response as ResultDTO<MyDTO>;
result.ShouldNotBeNull();
result.Value!.SomeField.ShouldBe(expected);
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
7. Run the specific test project first, usually with a narrow TUnit `--treenode-filter`, then broaden the scope if needed.

## Unit Test Verification

Use the shared build/test commands from `reaparr-backend`.

For unit test work:
- Start with the relevant `tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj` project.
- Prefer a narrow `--treenode-filter` for fast iteration.
- Broaden to the full affected unit test project before claiming completion when behavior or shared test infrastructure changed.

### TUnit filtering for unit tests

Use `--treenode-filter`, not `--filter`.

Filter syntax is:

```text
/<Assembly>/<Namespace>/<Class>/<Test>
```

Use `*` as a wildcard for segments you do not want to pin exactly. Use parentheses with `|` for OR conditions inside a single segment.

Filter by class:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/DownloadJobUnitTests/*"
```

Filter by test name:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/*/ShouldSetDownloadClientErrorStatus_WhenClientStartFailsWithoutSpecificError"
```

Filter multiple classes with OR:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/*/(DownloadJobUnitTests)|(DeterminePlexDownloadClientCommandHandlerUnitTests)/*"
```

Filter by namespace prefix:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo --treenode-filter "/*/Reaparr.Application.UnitTests.PlexDownloads*/*/*"
```

If you need exact test or class names, list tests first:

```bash
dotnet run --project tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj -- --no-ansi --disable-logo --list-tests
```

## Common Mistakes

- Using `[ClassName*]` bracket syntax in the class segment of `--treenode-filter` — this causes "Zero tests ran". Brackets are for property filters only (5th segment). Use plain wildcards: `"/*/*/MyClassUnitTests/*"` not `"/*/*/*[MyClass*]"`.
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
