## Project summary
Reaparr is a cross-platform Plex media downloader:
- Backend: .NET 10, FastEndpoints, EF Core, Autofac, Quartz, SignalR (MessagePack), Serilog, Polly
- Frontend: Nuxt 4 / Vue 3, Pinia, Quasar, PrimeVue
- Testing: xUnit, Shouldly, Moq, Bogus; Vitest, Cypress
- Frontend package manager: **Bun only** (no npm/yarn/pnpm)

## Commands

### Backend
```bash
dotnet build Reaparr.sln
dotnet run --project src/AppHost
```

### Frontend (run from `src/AppHost/ClientApp/`)

```bash
bun run dev
bun run build
bun run lint
bun run lint:fix
bun run typecheck
bun run generate-ts
```

### Tests

Backend:

```bash
dotnet test tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj
```

To run a specific unit test project (replace `<Project>` with e.g. `BackgroundJobs`):

```bash
dotnet test tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj
```

Frontend (from `src/AppHost/ClientApp/`):

```bash
bun run unit-test
bun run cypress:ci
```

### Docker

```bash
docker compose -f docker/docker-compose.yml up
```

## Architecture (high-signal)

Solution layout:

```
src/
  Domain/
  Application/
  Application.Contracts/
  Data/
  Data.Contracts/
  PlexApi/
  PlexApi.Contracts/
  BackgroundJobs/
  BackgroundJobs.Contracts/
  External/
  External.Contracts/
  FileSystem/
  FileSystem.Contracts/
  FluentResultExtensions/
  PublicAPI/
  PublicAPI.Contracts/
  Settings/
  Settings.Contracts/
  Identity/
  Identity.Contracts/
  SignalR/
  SignalR.Contracts/
  Logging/
  Environment/
  AppHost/
    ClientApp/
```

Test projects (under `tests/UnitTests/`): Application, BackgroundJobs, Data, Domain, External, FileSystem, FluentResultExtension, Logging, PlexApi, PublicApi, Settings, plus BaseTests (shared helpers).

Core patterns:

* FastEndpoints: `BaseEndpoint<TRequest, TResponse>` (project-local base, not `Endpoint<,>` directly)
* CQRS: command/query records implementing `ICommand<Result<T>>`, handlers implementing `ICommandHandler<TCommand, TResult>`, dispatched via `ICommandExecutor`
* DI: Autofac modules (`*Module : Module`) registered in `AppHost/_Shared/Config/Autofac/ContainerConfig.cs`
* Realtime: SignalR typed hubs + MessagePack; broadcast via `IHubContext<THub, TClientInterface>.Clients.All`
* Jobs: Quartz `IJob` with `[DisallowConcurrentExecution]`; jobs use `JobDataMap` for parameters, dispatch via `ICommandExecutor`, and must never throw (swallow and log)

## Code patterns (reference before writing new code)

### FastEndpoints

```csharp
public class MyEndpoint : BaseEndpoint<MyRequest, MyResponse>
{
    public override string EndpointPath => ApiRoutes.SomeGroup + "/Action";

    public override void Configure()
    {
        Get(EndpointPath); // or Post, Put, Delete
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<MyResponse>))
            .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO)));
    }

    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var result = await _commandExecutor.Send(new MyCommand(req.Id), ct);
        await SendFluentResult(result, ct);
    }
}
```

### CQRS command

```csharp
public record MyCommand(int Id) : ICommand<Result<MyData>>;

public class MyCommandHandler : ICommandHandler<MyCommand, Result<MyData>>
{
    public async Task<Result<MyData>> ExecuteAsync(MyCommand request, CancellationToken ct)
    {
        try
        {
            // ...
            return Result.Ok(data);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex)).LogError();
        }
    }
}
```

### Autofac modules

Each project registers its own types in a `*Module : Module`. Lifetimes:
- `InstancePerDependency()` – default for handlers, DbContext, factory
- `SingleInstance()` – stateful services (e.g. `SchedulerService`)
- Keyed registrations (`Keyed<IInterface>(enumValue)`) for factory-selected implementations

### SignalR broadcasting

```csharp
// Inject IHubContext<THub, TClientInterface>
await _hub.Clients.All.SomeMethod(dto, cancellationToken);
// Exceptions are caught and logged; never re-thrown
```

### Quartz jobs

```csharp
[DisallowConcurrentExecution]
public class MyJob : IJob
{
    public static JobKey GetJobKey(int id) => new($"MyJob_{id}", nameof(JobTypes.MyJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var id = context.JobDetail.JobDataMap.GetInt("Id");
        var result = await Result.Try(() => _commandExecutor.Send(new MyCommand(id), ct));
        // check result.IsCancelled, result.IsFailed — never throw
    }
}
```

## Formatting / style

EditorConfig enforces:
- Indent: 4 spaces, no tabs
- Max line length: 120
- Line endings: LF, encoding: UTF-8
- `var` preferred everywhere in C#
- Private constants: `ALL_UPPER_CASE`
- Private static readonly fields: `_camelCase`
- Modifier order: `public private protected internal new static abstract virtual sealed readonly override extern unsafe volatile async required`

## Change discipline

* Prefer small, focused diffs.
* Follow existing patterns; introduce new abstractions only if they remove duplication or reduce complexity.
* Keep behavior deterministic (especially tests).
* Fix root causes; do not add hacks to "quiet" symptoms.
* If uncertain: search the codebase for precedent and align with existing approach.

## Repo conventions

* Respect formatting/analyzers (EditorConfig/linters/formatters).
* Match existing naming and folder layout.
* Avoid breaking public APIs unless coordinated.

## Test rules (C#)

Frameworks:

* xUnit `[Fact]`, `async Task` when async
* Shouldly assertions
* Moq for verification

Required structure:

* Arrange–Act–Assert (comments allowed, keep short)
* Deterministic seeds only (no random data)
* Assert both result success/failure and relevant database state after execution

Helpers (expected usage):

* Inherit `BaseUnitTest<TSUT>` — provides `Sut`, `IDbContext`, `Mock`, `CancellationToken`
* Seed via `await SetupDatabase(seed, config => { ... })`
* Query/mutate DB via `IDbContext`; reuse single instance per test (`var dbContext = IDbContext;`)
* Mock resolution: `Mock.Mock<IFoo>()` returns the registered mock

Quality gates:

* Do not add "dirty" workarounds to make tests pass; fix the code or fix the test logic.
* Verify mock calls explicitly (`Times.Once()` / `Times.Never()`), no unverified mocks.

## Frontend unit test rules

* Vitest only
* Run via Bun: `bunx vitest`
* Run from `src/AppHost/ClientApp/`

## Test organization rules

Project mapping:

* Each production project has a corresponding `*.UnitTests` project.

Placement:

* Tests live in the `*.UnitTests` project matching the SUT project.
* Handler location determines test project, not command record location.

Folders and namespaces:

* Folder structure mirrors SUT for files only.
* Namespace is exactly `<SUTProjectNamespace>.UnitTests` (no folder-based namespace nesting).

Naming:

* Test file name: `<SutFileName>.UnitTests.cs` (no underscores)
* Test class name: `<SutFileName>UnitTests` (no dot, matches file name without `.cs`)
* Test method name: `ShouldExpectedBehavior_WhenCondition`

## File system tests

* If SUT touches file system: use `SetupFileSystem` (MockFileSystem)
* Do not manually mock `System.IO.Abstractions` interfaces in test files

## Test data rules

* Use builders/helpers from `tests/BaseTests`
* Prefer existing Faker-based builders
* Do not instantiate Bogus/Faker directly inside tests unless via BaseTests helper
* If a new pattern is needed: extend `BaseTests`, not individual tests

## Commit messages

Format:

```
<type>(WebAPI): <Imperative Message>
```

Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style`
Rules: imperative present tense, capitalize after colon, no trailing punctuation

Never add AI attribution trailers (e.g. `Co-Authored-By: Claude ...`). Commit messages are plain text only.

## Branching

* `dev` is the integration branch (PR target)
* Feature branches merge into `dev`

## Known constraints / gotchas (keep updated)

### BackgroundJobs.UnitTests referencing rules

* `BackgroundJobs.UnitTests` references only `BackgroundJobs` and `BaseTests`
* Types from `PlexApi.Contracts` are available transitively
* Do not reference `Application` / `Application.Contracts` directly from `BackgroundJobs.UnitTests`
* If handler lives under `src/BackgroundJobs/`, tests belong in `tests/UnitTests/BackgroundJobs.UnitTests/` (even if command record is in `Application.Contracts`)

### EF Core / SQLite bulk transactions

* `ExecuteBulkAsync` in `ReaparrDbContext` wraps each bulk op in its own `BeginTransactionAsync`
* Do **not** add outer transactions around `RemoveMedia` + `BulkInsert` chains
* SQLite nested transactions will fail ("connection is already in a transaction")

### IReaparrDbContextFactory threading rule

* Registered `InstancePerDependency` via Autofac
* Parallel fan-out (`Task.WhenAll`) requires each branch to resolve its own DbContext:

    * use `IReaparrDbContextFactory.Create()` per branch (sync) or `CreateAsync()` (async)
* Base test mock is prewired in `BaseUnitTest.MockDependencies.cs`:

    * both `Create()` and `CreateAsync()` are set up; do not re-mock them in test files

### LibrarySyncProgressStore behavior

* `UpdateItemAsync` upserts by `MediaType`; it does not remove completed items
* `SendProgressUpdateAsync` is private; the public surface is `UpdateItemAsync` / `UpdateErrorAsync` / `StartAsync`
* `LibraryProgress.TimeRemaining` is a computed property; do not assign it
* Strict mocks: set up both `UpdateItemAsync` and `UpdateErrorAsync` to avoid `MockException`

### RefreshPlexTvShowLibraryCommandHandler progress broadcasting

* Success path: `UpdateItemAsync` called 3× (TvShow/Season/Episode) using counts from `BulkInsertTvShowsRapport`
* Failure path: `UpdateErrorAsync` called once

### Quartz job rules

* Jobs implement `IJob` and must be decorated with `[DisallowConcurrentExecution]` unless fan-out is intentional
* Jobs must never throw — catch results and log; Quartz will reschedule on unhandled exceptions
* Parameters pass through `JobDataMap` (string keys defined as `public const string` on the job class)
* Use `Result.Try(...)` to wrap `_commandExecutor.Send(...)` and check `IsCancelled` before `IsFailed`
* `context.CancellationToken` carries the Quartz shutdown signal — pass it through to all async calls

### Settings interfaces with static abstract members

* `ISonarrSettings`, `IRadarrSettings` (and similar) extend `IBaseSettingsModule<T>` which declares `static abstract TModel Create()`
* Moq cannot mock these interfaces — attempting `Mock.Mock<ISonarrSettings>()` produces a CS8920 compiler error
* Pattern: inject concrete `SonarrSettings` / `RadarrSettings` record instances via `TypedParameter` when calling `Mock.Create<THandler>(...)`:

    ```csharp
    var sut = Mock.Create<MyHandler>(
        new TypedParameter(typeof(ISonarrSettings), new SonarrSettings { ... }),
        new TypedParameter(typeof(IIntegrationsSettings), IntegrationsSettings.Create())
    );
    ```

* Add `using Autofac;` to the test file to get `TypedParameter`

### FluentResults usage

* Return `Result.Ok(value)` or `Result.Fail(new ExceptionalError(ex)).LogError()`
* Propagate failures with `return failedResult.ToResult()`
* Check `Has504GatewayTimeoutError()` for Plex connectivity failures
* Never return `null` where a `Result` is expected

### Download client API compatibility

* `/api/v2/*` is rewritten to `/api/public/download-client/api/v2/*` in `Startup.Application` for qBittorrent-style clients
* `/torrents/createCategory` is handled as a no-op 200 OK to satisfy qBittorrent clients

## Performance rules during gaming (Arch Linux)

During gameplay, game performance has priority over builds/tests.

Required build command while gaming:

```bash
ionice -c2 -n7 nice -n 15 taskset -c 0-3 dotnet build -m:2
```

Optional if still lagging (raise game process priority):

```bash
sudo renice -n -5 -p $(pidof GameThread)
```

Do not renice other processes unless explicitly required.

## Agent self-update rule

After any non-trivial change, update this file with:

* newly discovered constraints (transactions, threading, ordering)
* corrections to wrong assumptions
* resolved ambiguities and "gotchas"
* removals of outdated rules

Keep additions short, concrete, and testable. Prefer rules that prevent repeat failures.
