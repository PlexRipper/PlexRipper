## Project Overview

Reaparr is a cross-platform Plex media downloader with a .NET 9.0 backend (FastEndpoints) and Nuxt 4/Vue 3 frontend. It features multi-threaded download management, SignalR real-time updates, and Plex API integration.

## Build and Development Commands

### Backend (.NET)

```bash
dotnet build Reaparr.sln          # Build solution
dotnet run --project src/AppHost  # Run application
```

### Frontend (Nuxt/Vue)

**Uses Bun, not npm!** All commands run from `src/AppHost/ClientApp/`:

```bash
bun run dev           # Start dev server
bun run build         # Build for production
bun run lint          # ESLint validation
bun run lint:fix      # Auto-fix linting
bun run typecheck     # TypeScript validation
```

### Testing

**Backend:**
```bash
dotnet test tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj
dotnet test tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj
```

**Frontend** (from `src/AppHost/ClientApp/`):
```bash
bun run unit-test         # Vitest single run
bun run unit-test-watch   # Vitest watch mode
bun run cypress:ci        # Cypress E2E (Firefox)
```

### Docker

```bash
docker compose -f docker/docker-compose.yml up
```

## Architecture

### Solution Structure

```
src/
├── Domain/                    # Entities and business logic
├── Application/              # Use cases, endpoints (FastEndpoints), handlers
├── Application.Contracts/    # Interface contracts
├── Data/                     # EF Core DbContext and repositories
├── Data.Contracts/
├── PlexApi/                  # Plex API client
├── PlexApi.Contracts/
├── BackgroundJobs/           # Quartz job scheduling
├── BackgroundJobs.Contracts/
├── FileSystem/               # File operations
├── Settings/                 # Configuration management
├── Identity/                 # Authentication
├── Logging/                  # Serilog infrastructure
└── AppHost/                  # ASP.NET Core host
    └── ClientApp/            # Nuxt 4 SPA frontend
```

### Key Patterns

- **FastEndpoints**: Each endpoint inherits `Endpoint<TRequest, TResponse>`
- **CQRS**: Commands/Queries with FluentResult for success/failure
- **DI**: Autofac container
- **Real-time**: SignalR hubs with MessagePack compression
- **Background Jobs**: Quartz scheduler

## Testing Conventions

### Backend Unit Tests (xUnit + Shouldly)

**Test Structure:**
- Use xUnit with `[Fact]` attributes
- Use `async Task` methods for async operations
- Use Shouldly assertions (`ShouldBeTrue()`, `ShouldNotBeNull()`, `ShouldBeEmpty()`)
- Follow Arrange-Act-Assert pattern with clear comments
- Keep tests deterministic: seed data explicitly, never random
- Ensure tests pass before stopping; fix bugs in code if found, no dirty workarounds

**Naming Conventions:**
- Method naming: `ShouldExpectedBehavior_WhenCondition`
- File naming: `{SUT}.UnitTests.cs` (e.g., `ClearCompletedDownloadTasksEndpoint.UnitTests.cs`)
- Class name matches file name without `.cs`
- Never use underscores in test file names

**Organization:**
- Each project has corresponding `*.UnitTests` project (e.g., `Reaparr.Application` → `Reaparr.Application.UnitTests`)
- Follow same folder structure as SUT, but namespace is always `{Project}.UnitTests` (no subfolders in namespace)

**Test Setup:**
- Use `BaseUnitTest<TEndpoint>` base class for endpoints
- Use `SetupDatabase(seed, config => { ... })` to seed EF Core test data
- Use `SetupEndpointUnitTest<TEndpoint>()` and call `HandleAsync` on it
- Single Context Pattern: Use `var dbContext = IDbContext;` once, then reuse throughout test
- Always assert both endpoint Response and database state after execution

**File System Testing:**
- Use `SetupFileSystem()` with `MockFileSystem` from BaseUnitTest class
- Don't mock `System.IO.Abstractions` interfaces manually

**Test Data Generation:**
- Use existing functionality from `tests/BaseTests` project
- Use provided Faker-based builders and helpers
- Never instantiate Bogus/Faker directly in tests
- Extend `BaseTests` project if new patterns needed

**Mocking (AutoMoq):**
- Use AutoMock for automatic dependency mocking
- Always verify mocks with explicit counts: `mock.Verify(x => x.Method(), Times.Once())`
- Assert unused dependencies with `Times.Never()`
- Every mock must be verified

### Frontend Unit Tests (Vitest)

- Always run via Bun: `bunx vitest` (never npm/yarn/pnpm)
- Run from `src/AppHost/ClientApp/`

## Commit Message Format

```
<type>(WebAPI): <Imperative Message>
```

**Types**: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style`

**Examples**:
- `feat(WebAPI): Add authentication endpoint`
- `fix(WebAPI): Resolve null reference in download workflow`
- `refactor(WebAPI): Simplify cache handling`

Always use `WebAPI` scope. Imperative, present tense, capitalize after colon, no trailing punctuation.

## Key Dependencies

- **Backend**: FastEndpoints, EF Core, Autofac, Quartz, Polly, Serilog, SignalR
- **Frontend**: Nuxt 4, Vue 3, Pinia, PrimeVue, Axios
- **Testing**: xUnit, Shouldly, Moq, Bogus, Vitest, Cypress

## Branches

- `dev`: Main development branch (target for PRs)
- Feature branches merge to `dev`
