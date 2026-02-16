# Contributing to Reaparr

Thank you for your interest in contributing. This document covers everything you need to know to develop effectively in this project.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Architecture and Key Patterns](#architecture-and-key-patterns)
- [Backend Development](#backend-development)
- [Frontend Development](#frontend-development)
- [Testing](#testing)
- [Docker](#docker)
- [Code Conventions](#code-conventions)
- [Commit Messages](#commit-messages)
- [Branch Strategy](#branch-strategy)
- [Pull Requests](#pull-requests)

---

## Project Overview

Reaparr is a cross-platform Plex media downloader with a .NET 10.0 backend (FastEndpoints) and Nuxt 4 / Vue 3 frontend. It features multi-threaded download management, SignalR real-time updates, and Plex API integration.

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 10.0+   | Backend runtime and build toolchain |
| Bun | Latest  | Frontend package manager — **do not use npm, yarn, or pnpm** |
| Docker | Latest  | For running the full stack in containers |
| Docker Compose | v2+     | `docker compose` (not `docker-compose`) |

---

## Getting Started

### 1. Clone and build the backend

```bash
git clone <repo-url>
cd Reaparr
dotnet build Reaparr.sln
```

### 2. Install frontend dependencies

```bash
cd src/AppHost/ClientApp
bun install
```

### 3. Run locally

**Backend:**

```bash
dotnet run --project src/AppHost
```

If you are using [JetBrains Rider](https://www.jetbrains.com/rider/), a pre-configured run configuration is included at `.run/Reaparr Back-End Development.run.xml`. It sets `DOTNET_ENVIRONMENT=Development`, `LOG_LEVEL=DEBUG`, `UNMASKED=true`, and points `DEVELOPMENT_ROOT_PATH` to `../../DATA/ReaparrCache/` relative to the project root. Open the project in Rider and select **Reaparr Back-End Development** from the run configurations dropdown to start the backend without any manual configuration.

**Frontend dev server** (from `src/AppHost/ClientApp/`):

```bash
bun run dev
```

### 4. Run via Docker

```bash
docker compose -f docker/docker-compose.yml up
```

For debug builds with `LOG_LEVEL=DEBUG`:

```bash
docker compose -f docker/docker-compose.debug.yml up
```

**Docker volume paths** (`/Config`, `/Downloads`, `/Movies`, `/TvShows`) must point to valid writable directories on your host.

---

## Project Structure

```
src/
├── Domain/                    # Core domain types and abstractions
├── Application/               # Application layer, endpoints, use cases
├── Application.Contracts/     # Shared contracts/interfaces for Application
├── Data/                      # EF Core DbContext, migrations, repositories
├── Data.Contracts/            # Database interfaces
├── PlexApi/                   # Plex API client implementations
├── PlexApi.Contracts/         # Plex API interfaces and DTOs
├── BackgroundJobs/            # Quartz background job handlers
├── BackgroundJobs.Contracts/  # Background job command/query contracts
├── FileSystem/                # File system abstractions
├── Settings/                  # Application settings management
├── Identity/                  # Authentication and identity
├── Logging/                   # Serilog configuration
└── AppHost/
    ├── Startup/               # ASP.NET Core startup configuration
    ├── _Shared/               # Shared middleware, filters, DI modules
    └── ClientApp/             # Nuxt 4 / Vue 3 frontend

tests/
├── BaseTests/                 # Shared test infrastructure, Faker builders
├── UnitTests/
│   ├── Application.UnitTests/
│   ├── BackgroundJobs.UnitTests/
│   └── ...
├── IntegrationTests/
└── E2ETests/
```

---

## Development Workflow

1. Branch from `dev` using a descriptive name (e.g. `fix/download-null-ref`, `feat/sonarr-integration`).
2. Make small, focused commits. Prefer a minimal diff.
3. Run all relevant tests before opening a PR.
4. Target `dev` for all pull requests.

**Before starting on any task:**

- Read existing implementations and tests for precedent before inventing new patterns.
- Follow what already exists; do not introduce new abstractions unless necessary.
- Fix root causes; do not paper over problems with hacks.

---

## Architecture and Key Patterns

### [FastEndpoints](https://fast-endpoints.com/)

All API endpoints inherit `Endpoint<TRequest, TResponse>`. Follow the existing endpoint structure in `src/Application/`.

### CQRS

Commands and queries use [FluentResults](https://github.com/altmann/FluentResults) (`Result<T>`) for success/failure. Handlers are registered via [Autofac](https://autofac.org/) modules.

### Dependency Injection

[Autofac](https://autofac.org/) is the DI container. Register services in the corresponding `*Module.cs` file for the project. `IReaparrDbContextFactory` is `InstancePerDependency` — each parallel branch must resolve its own `IReaparrDbContext` via `IReaparrDbContextFactory.Create()` to avoid [EF Core](https://learn.microsoft.com/en-us/ef/core/) thread-safety violations.

### Real-time Updates

[SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction) hubs use MessagePack compression. Progress stores (e.g. `LibrarySyncProgressStore`) broadcast updates to connected clients.

### Background Jobs

[Quartz.NET](https://www.quartz-scheduler.net/) scheduler drives background processing. Job handlers live in `src/BackgroundJobs/`.

### EF Core / SQLite

`ExecuteBulkAsync` in `ReaparrDbContext` already wraps bulk operations in their own transactions. Do **not** add outer transactions around bulk operation chains — SQLite does not support nested transactions.

### Resilience

HTTP clients use [Polly](https://github.com/App-vNext/Polly) retry/circuit-breaker policies.

### Logging

Structured logging is provided by [Serilog](https://serilog.net/).

---

## Backend Development

### Adding an Endpoint

1. Create the endpoint class in the appropriate folder under `src/Application/`, inheriting `Endpoint<TRequest, TResponse>`.
2. Define request/response types.
3. Register any new services in the relevant Autofac module.
4. Add a corresponding unit test in `Application.UnitTests` mirroring the same folder path.

### Adding a Background Job Command

1. Define the command record in `src/BackgroundJobs.Contracts/`.
2. Implement the handler in `src/BackgroundJobs/`.
3. Write tests in `tests/UnitTests/BackgroundJobs.UnitTests/` — **not** in `Application.UnitTests`.

### Code Style

- Follow EditorConfig and analyzer settings already in the repository.
- Match existing naming conventions and folder layout.
- Do not break public APIs without coordinating changes across all consumers.

---

## Frontend Development

All frontend commands must be run from `src/AppHost/ClientApp/` using **Bun**.

| Command | Purpose                                                                        |
|---------|--------------------------------------------------------------------------------|
| `bun run dev` | Start development server                                                       |
| `bun run build` | Production build                                                               |
| `bun run lint` | ESLint validation                                                              |
| `bun run lint:fix` | Auto-fix lint issues                                                           |
| `bun run typecheck` | TypeScript type checking                                                       |
| `bun run generate-ts` | Regenerate OpenAPI types from the backend spec, the back-end should be started |

### Regenerating API Types

After changing backend endpoints or DTOs, regenerate the TypeScript types:

```bash
bun run generate-ts
```

This writes generated files to `src/AppHost/ClientApp/src/types/api/generated/`. Do not manually edit files in that directory.

### Key Frontend Dependencies

- **[Nuxt 4](https://nuxt.com/) / [Vue 3](https://vuejs.org/)** — framework
- **[Pinia](https://pinia.vuejs.org/)** — state management
- **[Quasar](https://quasar.dev/) + [PrimeVue](https://primevue.org/)** — UI component libraries
- **[Axios](https://axios-http.com/)** — HTTP client

---

## Testing

### Backend Unit Tests

- Framework: **[xUnit](https://xunit.net/)** with `[Fact]` attributes.
- Assertions: **[Shouldly](https://github.com/shouldly/shouldly)** (`ShouldBeTrue`, `ShouldNotBeNull`, etc.).
- Mocking: **[AutoMoq](https://github.com/darrencauthon/AutoMoq) + [Moq](https://github.com/devlooped/moq)**.
- Test data: **[Bogus](https://github.com/bchavez/Bogus)** via helpers in `tests/BaseTests/`.

#### Test organization

| Rule | Example |
|------|---------|
| One `*.UnitTests` project per production project | `Reaparr.Application` → `Application.UnitTests` |
| Same folder structure as SUT (folders only, not namespaces) | `Application/Downloads/Foo.cs` → `Application.UnitTests/Downloads/Foo.UnitTests.cs` |
| Namespace is always the project root namespace + `.UnitTests` | `namespace Reaparr.Application.UnitTests;` |
| File named after SUT with `.UnitTests.cs` appended | `FooEndpoint.UnitTests.cs` |
| No underscores in test file names | |
| Test class name matches file name (minus `.cs`) | `FooEndpoint.UnitTests` |

#### Test method naming

```csharp
[Fact]
public async Task ShouldReturnNotFound_WhenMediaDoesNotExist()
```

#### Endpoint tests

```csharp
// Arrange
await SetupEndpointUnitTest<MyEndpoint>();

// Act
await HandleAsync(request);

// Assert
Response.ShouldNotBeNull();
var dbContext = IDbContext;
// verify database state using dbContext
```

Always assert both the endpoint `Response` **and** the resulting database state.

#### Database seeding

```csharp
await SetupDatabase(seed, config => { /* configure in-memory db */ });
var dbContext = IDbContext; // reuse this single instance throughout the test
```

Use a single `dbContext` variable — do not re-access `IDbContext` multiple times in one test.

#### Mocking rules

- Every mock must be verified with explicit call counts:
  ```csharp
  mock.Verify(x => x.Method(), Times.Once());
  mock.Verify(x => x.OtherMethod(), Times.Never());
  ```
- Never leave a mock unverified.
- Use `Times.Once()` or `Times.Never()` — avoid vague expectations.

#### Test data

- Use Faker-based builders from `tests/BaseTests/`.
- Never instantiate `Bogus.Faker` directly inside test files.
- Never use random or non-deterministic data in tests.

#### File system interactions

- Use `SetupFileSystem` from `BaseUnitTest` with `MockFileSystem`.
- Do not manually mock `System.IO.Abstractions` interfaces in test files.

### Running Backend Tests

```bash
# Unit tests
dotnet test tests/UnitTests/Application.UnitTests/Application.UnitTests.csproj

# Integration tests
dotnet test tests/IntegrationTests/IntegrationTests/IntegrationTests.csproj
```

### Frontend Unit Tests

- Framework: **[Vitest](https://vitest.dev/)**, always run via **[Bun](https://bun.sh/)**.

```bash
# From src/AppHost/ClientApp/
bunx vitest
bun run unit-test
```

### Frontend E2E Tests

Uses **[Cypress](https://www.cypress.io/)**.

```bash
# From src/AppHost/ClientApp/
bun run cypress:ci
```

---

## Docker

### Production

```bash
docker compose -f docker/docker-compose.yml up
```

### Debug (verbose logging)

```bash
docker compose -f docker/docker-compose.debug.yml up
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `PUID` | `1000` | User ID for file permissions |
| `PGID` | `1000` | Group ID for file permissions |
| `TZ` | `America/New_York` | Timezone |
| `LOG_LEVEL` | `INFORMATION` | Logging verbosity (`DEBUG`, `INFORMATION`, `WARNING`, `ERROR`) |
| `LOG_ENV_VARS` | `false` | Dump all env vars to log on startup |
| `UNMASKED` | `false` | Show sensitive data in logs |

### Volume Mounts

| Container Path | Purpose |
|---------------|---------|
| `/Config` | Application configuration and database |
| `/Downloads` | Active downloads staging area |
| `/Movies` | Movie media library |
| `/TvShows` | TV show media library |

All host paths must exist and be writable by the `PUID`/`PGID` user.

---

## Code Conventions

- **Small, focused changes.** Prefer minimal diffs. Do not clean up surrounding code that was not related to the task.
- **No unnecessary abstractions.** If something works with the existing pattern, use it.
- **Deterministic behavior.** Especially in tests — never rely on random data or ordering.
- **Fix root causes.** Do not use workarounds or hacks that mask underlying problems.
- **No over-engineering.** Do not add features, error handling, or fallbacks for scenarios that cannot happen.
- **No unused code.** Delete dead code completely rather than commenting it out or renaming with `_` prefixes.
- **Formatting.** Respect EditorConfig and all analyzer/linter settings already configured in the repository.

---

## Commit Messages

Format:

```
<type>(WebAPI): <Imperative message>
```

- The scope is always `WebAPI`.
- Message is imperative, present tense.
- Capitalize the first word after the colon.
- No trailing punctuation.

### Types

| Type | When to use |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `docs` | Documentation only |
| `build` | Build system or dependency changes |
| `chore` | Maintenance tasks |
| `style` | Formatting, whitespace, no logic change |

### Examples

```
feat(WebAPI): Add Sonarr integration endpoint
fix(WebAPI): Resolve null reference in download workflow
refactor(WebAPI): Simplify cache handling in library sync
test(WebAPI): Add unit tests for ClearCompletedDownloadTasksEndpoint
```

---

## Branch Strategy

| Branch | Purpose |
|--------|---------|
| `dev` | Main development branch — target all PRs here |
| Feature branches | Branch from `dev`, merge back into `dev` |

Do not target `main`/`master` directly unless explicitly instructed.

---

## Pull Requests

1. Target the `dev` branch.
2. Ensure all backend tests pass: `dotnet test Reaparr.sln`.
3. Ensure all frontend checks pass: `bun run lint`, `bun run typecheck`, `bun run unit-test`.
4. Keep PRs focused — one logical change per PR.
5. Write a clear description explaining the what and why, not just the what.
6. If your change modifies backend API contracts, regenerate TypeScript types (`bun run generate-ts`) and include those changes in the same PR.
