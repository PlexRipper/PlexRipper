---
name: reaparr-backend-unit-tests
description: Use when creating or updating C# backend unit tests in Reaparr, especially for handlers, services, endpoints, and jobs that must follow the project's xUnit, Shouldly, Moq, BaseUnitTest, naming, placement, and deterministic test-data conventions.
---

# Reaparr Backend Unit Tests

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

- Test framework: `xUnit` with `[Fact]` and `async Task` where needed.
- Assertions: `Shouldly`.
- Mocks: `Moq` with explicit verification (`Times.Once()` / `Times.Never()`).
- Structure: Arrange -> Act -> Assert. Within Arrange, mock setups (`Mock.Mock<T>()`) must always be the **last step**, immediately before Act.
- Determinism: no random behavior in tests.

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
- `xunit.v3` + runner: project test runtime and discovery.
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
- **Mock setups must be inline per test.** Do not extract them into shared helper methods. Each test must be self-contained and readable without jumping elsewhere to understand what is mocked.
- **Every mock setup must end with `.Verifiable(Times.X())`** to declare how many times it is expected to be called. This collocates the expectation with the setup and makes unmet expectations fail automatically.

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

### Endpoint unit tests and DbContext

- `SetupEndpointUnitTest<T>()` provides a real in-memory `IReaparrDbContext`.
- Avoid mocking `IReaparrDbContext` in endpoint tests unless intentionally re-registering a mock.

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
7. Run the specific test project first, then broader suite if needed.

## Commands

Run a specific backend unit test project:

```bash
dotnet test tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj
```

Common projects:

```bash
dotnet test tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/UnitTests/BackgroundJobs.UnitTests/BackgroundJobs.UnitTests.csproj
```

## Common Mistakes

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
