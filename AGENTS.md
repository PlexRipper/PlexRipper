## Project Overview

Reaparr is a cross-platform Plex media downloader with a .NET 9.0 backend (FastEndpoints) and Nuxt 4/Vue 3 frontend. It features multi-threaded download management, SignalR real-time updates, and Plex API integration.

## Build and Development Commands

### Backend (.NET)

```bash
dotnet build Reaparr.sln
dotnet run --project src/AppHost
````

### Frontend (Nuxt/Vue)

Uses Bun (not npm/yarn/pnpm). Run all commands from `src/AppHost/ClientApp/`.

```bash
bun run dev           # Start dev server
bun run build         # Build for production
bun run lint          # ESLint validation
bun run lint:fix      # Auto-fix linting
bun run typecheck     # TypeScript validation
bun run generate-ts   # Generated the OpenAPI endpoints and definitions in src/AppHost/ClientApp/src/types/api/generated 
```

### Testing

Backend:

```bash
dotnet test tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj
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

## Architecture

### Solution Structure

```
src/
├── Domain/
├── Application/
├── Application.Contracts/
├── Data/
├── Data.Contracts/
├── PlexApi/
├── PlexApi.Contracts/
├── BackgroundJobs/
├── BackgroundJobs.Contracts/
├── FileSystem/
├── Settings/
├── Identity/
├── Logging/
└── AppHost/
    └── ClientApp/
```

### Key Patterns

* FastEndpoints: endpoints inherit `Endpoint<TRequest, TResponse>`.
* CQRS: Commands/Queries with FluentResults for success/failure.
* DI: Autofac container.
* Real-time: SignalR hubs with MessagePack compression.
* Background jobs: Quartz scheduler.

## Working style

* Prefer small, focused changes with minimal diff.
* Follow existing patterns in the codebase; avoid new abstractions unless necessary.
* Keep behavior deterministic and reproducible (especially tests).
* Do not “paper over” problems with hacks; fix root causes.
* When uncertain, inspect existing implementations/tests for precedent before inventing a new approach or ask questions

## Repository conventions

* Respect existing formatting and analyzer settings (EditorConfig, formatters, linters).
* Match existing naming, folder layout, and dependency injection patterns.
* Avoid breaking public APIs without coordinated changes.

## Testing rules (C#)

* Write tests using xUnit with `[Fact]` attributes.
* Use `async Task` test methods whenever async operations are involved.
* Use Shouldly for assertions (`ShouldBeTrue`, `ShouldNotBeNull`, `ShouldBeEmpty`, etc.).
* Follow Arrange–Act–Assert with clear comments.
* Name test methods: `ShouldExpectedBehavior_WhenCondition`.
* Use `BaseUnitTest<TEndpoint>` base class for endpoints when available.
* Use `SetupDatabase(seed, config => { ... })` to seed EF Core test data.
* Use `IDbContext` for database queries and mutations.
* When testing endpoints, use `SetupEndpointUnitTest<TEndpoint>()` and call `HandleAsync`.
* Always assert against both the endpoint `Response` and the state of the database after execution.
* Keep tests deterministic: seed data explicitly, never random.
* Prefer expressive, scenario-driven test names over generic ones.
* Single Context Instance Pattern: when setting up data and verifying results, use a single `dbContext` variable rather than re-accessing `IDbContext` multiple times.
  Example: `var dbContext = IDbContext;` then reuse `dbContext` throughout the test.
* Ensure created tests are passing before stopping; if a bug is found in production code, fix it.
* Do not use dirty workarounds to make tests pass; ensure the test is logically correct and/or fix the code being tested.

## Front-end unit test rules

* All front-end unit tests must use Vitest as the test runner.
* Always run Vitest through Bun using `bunx vitest` (never npm/yarn/pnpm).
* Run from `src/AppHost/ClientApp/`.

## Test organization rules

* Each production project must have a corresponding UnitTest project with the same name + `.UnitTests` suffix.
  Example: `Reaparr.Application` → `Reaparr.Application.UnitTests`.
* Tests must be placed in the corresponding `*.UnitTests` project of the SUT.
* Within that project, follow the same folder structure as the SUT for files only, not for namespaces.
* The namespace of every test class must be exactly the project namespace + `.UnitTests` (no subfolders in namespace).
  Example: all tests in `Reaparr.Application.UnitTests` use `namespace Reaparr.Application.UnitTests;`.
* Each unit test file is named after the SUT file with `.UnitTests.cs` appended.
  Example: `ClearCompletedDownloadTasksEndpoint.cs` → `ClearCompletedDownloadTasksEndpoint.UnitTests.cs`.
* Never use underscores in test file names.
* Test class name matches the file name without the `.cs` extension.
  Example: `ClearCompletedDownloadTasksEndpoint.UnitTests`.

## File system interaction rules

* If the SUT interacts with the file system, use `SetupFileSystem` from the `BaseUnitTest` class with `MockFileSystem`.
* Do not manually mock `System.IO.Abstractions` interfaces in test files.

## Test data generation rules

* Use existing functionality from the `tests/BaseTests` project for generating test data.
* Prefer provided Faker-based builders and helpers rather than creating random data inline.
* Never instantiate Bogus/Faker directly inside tests unless wrapped by a helper in `BaseTests`.
* If new patterns are needed, extend the `BaseTests` project, not individual test files.

## Mocking rules (AutoMoq + Moq)

* Use AutoMock to automatically create mocks for dependencies.
* Always verify mocks with explicit invocation counts using Moq’s Verify API.
  Example: `mock.Verify(x => x.Method(), Times.Once());`
* If a dependency should not be called, explicitly assert with `Times.Never()`.
* Do not leave mocks unverified; every mock must be asserted for calls or asserted that no calls occurred.
* Prefer `Times.Once()` or `Times.Never()` over vague call expectations.

## Commit message format

```
<type>(WebAPI): <Imperative Message>
```

* Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style`
* Always use `WebAPI` scope.
* Imperative, present tense, capitalize after colon, no trailing punctuation.

Examples:

* `feat(WebAPI): Add authentication endpoint`
* `fix(WebAPI): Resolve null reference in download workflow`
* `refactor(WebAPI): Simplify cache handling`

## Branches

* `dev`: main development branch (target for PRs)
* Feature branches merge into `dev`

## Key dependencies

* Backend: FastEndpoints, EF Core, Autofac, Quartz, Polly, Serilog, SignalR
* Frontend: Nuxt 4, Vue 3, Pinia, Quasar, PrimeVue, Axios
* Testing: xUnit, Shouldly, Moq, Bogus, Vitest, Cypress

## Known implementation details

### BackgroundJobs test project
* `BackgroundJobs.UnitTests` references only `BackgroundJobs` and `BaseTests`. Types from `PlexApi.Contracts` are available transitively through `BackgroundJobs`.
* `BackgroundJobs.UnitTests` does NOT reference `Application` or `Application.Contracts` directly. Tests for handlers that live in `BackgroundJobs` must go in `BackgroundJobs.UnitTests`, not `Application.UnitTests`.

### EF Core / SQLite transactions
* `ExecuteBulkAsync` in `ReaparrDbContext` already wraps every bulk operation in its own `BeginTransactionAsync`. Do NOT add outer transactions around `RemoveMedia` + `BulkInsert` call chains — SQLite does not support nested transactions and tests will fail with "connection is already in a transaction".

### IReaparrDbContextFactory
* Registered as `InstancePerDependency` in Autofac (`DataModule.cs`). Uses a `Func<IReaparrDbContext>` factory delegation that Autofac auto-provides.
* When a command fans out parallel work (e.g. `Task.WhenAll`), each parallel branch must resolve its own `IReaparrDbContext` via `IReaparrDbContextFactory.Create()` to avoid EF Core thread-safety violations.
* Test mock is pre-wired in `BaseUnitTest.MockDependencies.cs` via `factoryMock.Setup(x => x.Create()).Returns(...)`.

### LibrarySyncProgressStore
* `UpdateItemAsync` upserts by `MediaType` (adds if not present, replaces if it is). It does NOT auto-remove completed items.
* `SendProgressUpdateAsync` is `Task`-returning (not `Task<LibraryProgress?>`).
* `LibraryProgress.TimeRemaining` is a computed property — do not try to assign it.
* When mocking `ILibrarySyncProgressStore` in tests, always set up **both** `UpdateItemAsync` and `UpdateErrorAsync` to avoid `Strict` mock exceptions. Success paths call `UpdateItemAsync`; failure paths call `UpdateErrorAsync`.

### RefreshPlexTvShowLibraryCommandHandler progress broadcasting
* On the success path, `UpdateItemAsync` is called 3× — once each for `PlexMediaType.TvShow`, `PlexMediaType.Season`, and `PlexMediaType.Episode` — using counts from `BulkInsertTvShowsRapport`.
* On any failure path, `UpdateErrorAsync` is called once.

### BackgroundJobs test location (BackgroundJobs commands also tested in BackgroundJobs.UnitTests)
* Commands whose handlers live under `src/BackgroundJobs/` must be tested in `tests/UnitTests/BackgroundJobs.UnitTests/`, mirroring the same folder structure as the SUT.
* Do not place BackgroundJobs handler tests in `Application.UnitTests` even if the command record is defined in `Application.Contracts`.

## Agent self-update rule

After completing any non-trivial task, update this file with new findings:
* Undocumented implementation constraints discovered (e.g. transaction limits, threading rules).
* Corrections to previously wrong assumptions.
* Patterns that were ambiguous and are now resolved.
* Any "gotcha" that caused a bug or wasted time.

Keep entries concise and actionable. Remove entries that are no longer accurate.
