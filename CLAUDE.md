# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Backend (.NET)
```bash
# Build the solution
dotnet build

# Run unit tests
dotnet test tests/UnitTests/UnitTests.sln

# Run integration tests
dotnet test tests/IntegrationTests/IntegrationTests.sln

# Run the web API
dotnet run --project src/WebAPI/WebAPI.csproj
```

### Frontend (Vue.js/Nuxt)
All frontend commands must be run from `src/WebAPI/ClientApp/`:
```bash
cd src/WebAPI/ClientApp/

# Development
bun run dev              # Start development server
bun run build            # Build for production
bun run preview          # Preview production build

# Testing and Quality
bun run lint             # Run ESLint
bun run lint:fix         # Fix ESLint issues
bun run typecheck        # TypeScript type checking
bun run test             # Run Vitest unit tests
bun run test-watch       # Run tests in watch mode
bun run cypress:e2e      # Run Cypress E2E tests

# Code Generation
bun run generate-ts      # Generate TypeScript API client from Swagger
```

## High-Level Architecture

PlexRipper follows Clean Architecture with Domain-Driven Design (DDD):

### Backend Structure
- **Domain**: Core business entities and logic (no dependencies)
- **Application**: CQRS handlers (Commands/Queries via MediatR), business rules
- **Infrastructure**: External concerns - Data (EF Core), FileSystem, PlexApi
- **WebAPI**: FastEndpoints API + SignalR hubs for real-time updates

### Frontend Structure
- **Nuxt 3** with Vue 3 composition API
- **Pinia** for state management
- **PrimeVue 4** + **Quasar 2** for UI components
- **SignalR** client for real-time download progress
- **TypeScript** with strict type checking

### Key Patterns
- **CQRS**: All business operations go through MediatR Commands/Queries
- **Repository Pattern**: Implicit via Entity Framework DbContext
- **Background Jobs**: Quartz.NET for scheduled tasks and download management
- **Real-time Updates**: SignalR for progress notifications
- **Dependency Injection**: Autofac containers with module-based registration

### Database
- **Main Database**: SQLite for application data
- **Identity Database**: Separate SQLite for authentication
- **Migrations**: Code-first with EF Core migrations in `src/Data/Migrations/`

### Docker and CI/CD
- Multi-platform Docker images (amd64, arm64, armv7)
- GitHub Actions for automated testing and releases
- Semantic versioning with automatic changelog generation
- Docker Compose support for local development

### Torznab API Integration
PlexRipper includes a Torznab API that allows it to function as an indexer for ARR applications (Sonarr, Radarr, etc.):

**Key Components:**
- **TorznabSettingsModule**: Configuration for API key, server filters, result limits
- **Search Endpoint**: `/api/torznab?t=search&q=query&apikey=key`
- **Capabilities Endpoint**: `/api/torznab?t=caps` (returns supported features)
- **Download Endpoint**: `/api/torznab/download/{id}?apikey=key`

**Setup for ARR Integration:**
1. Enable Torznab in settings and generate API key
2. Configure server filters to limit which Plex servers to search
3. Add PlexRipper as indexer in Sonarr/Radarr:
   - URL: `http://plexripper:7879/api/torznab`
   - API Key: From PlexRipper settings
   - Categories: Movies (2000), TV (5000)

**Features:**
- XML response format compliant with Torznab specification
- Category-based filtering (Movies/TV)
- Multi-server search across enabled Plex servers
- Webhook notifications for download status updates
- Automatic download task creation when ARR apps request downloads

Invoke Gemini AI for peer review or alternate opinion at any time. Rate limit is 60 requests per second.

- Installation: https://github.com/google-gemini/gemini-cli

- Command: gemini -p for analysis tasks

Use Gemini CLI when:

- Analyzing entire codebases or large directories

- Comparing multiple large files (especially ROM processing logic across gameMetadata.js and related files)

- Need to understand project-wide patterns or architecture

- Current context window is insufficient for the task

- Working with files totaling more than 100KB

- Verifying if specific features, patterns, or security measures are implemented across the codebase

- Checking for the presence of certain coding patterns across the entire codebase

- Reviewing EmulatorJS integration files in web/static/data/ and web/static/emulators/

- Analyzing the relationship between backend ROM processing and frontend emulator initialization

- Gemini's context window can handle entire codebases that would overflow Claude's context

- No need for --yolo flag for read-only analysis

- When checking implementations, be specific about what you're looking for to get accurate results

Project-Specific Gemini Use Cases:

- Review entire EmulatorJS integration for compatibility issues

- Analyze ROM file processing pipeline from upload to emulator initialization

- Security review across authentication, file upload, and session management

- Performance analysis of database queries and frontend JavaScript

- Cross-reference save state management between backend and frontend components
