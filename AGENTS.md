## Project overview

Reaparr is a cross-platform Plex media downloader.

| Layer    | Stack |
|----------|-------|
| Backend  | .NET 10, FastEndpoints, EF Core, Autofac, Quartz, SignalR (MessagePack), Serilog, Polly |
| Frontend | Nuxt 4 / Vue 3, Pinia, Quasar, PrimeVue |
| Testing  | xUnit, Shouldly, Moq, Bogus; Vitest, Cypress |

> **Package manager:** The frontend uses **Bun exclusively** — never use npm, yarn, or pnpm.

---

## Commands

### Backend

```bash
dotnet build Reaparr.sln
dotnet run --project src/AppHost
```

### Frontend (`src/AppHost/ClientApp/`)

```bash
bun run dev          # Dev server
bun run build        # Production build
bun run lint         # Lint check
bun run lint:fix     # Lint auto-fix
bun run typecheck    # Type checking
bun run generate-ts  # Generate TypeScript types (requires backend running in dev mode — see below)
```

> **`generate-ts` prerequisite:** The backend must be running in dev mode before executing `bun run generate-ts`. Use the Rider run configuration at `.run/Reaparr Back-End Development.run.xml`, or start the backend manually with `dotnet run --project src/AppHost`.

### Performance-constrained build (while gaming on Arch Linux)

Game performance takes priority over builds and tests. Use:

```bash
ionice -c2 -n7 nice -n 15 taskset -c 0-3 dotnet build -m:2
```

If the game still lags, optionally raise its priority:

```bash
sudo renice -n -5 -p $(pidof GameThread)
```

Do **not** renice other processes unless explicitly requested.

---

## Architecture patterns

### Backend

- **Endpoints:** Inherit from `BaseEndpoint<TRequest, TResponse>` (project-local base class). Never use `Endpoint<,>` directly.
- **CQRS:** Command/query records implement `ICommand<Result<T>>`; handlers implement `ICommandHandler<TCommand, TResult>`; dispatch via `ICommandExecutor`.
- **DI:** Autofac modules (`*Module : Module`) registered in `AppHost/_Shared/Config/Autofac/ContainerConfig.cs`.
- **Realtime:** SignalR typed hubs with MessagePack serialization. Broadcast via `IHubContext<THub, TClientInterface>.Clients.All`.
- **Jobs:** Quartz `IJob` with `[DisallowConcurrentExecution]`. Jobs use `JobDataMap` for parameters, dispatch via `ICommandExecutor`, and **must never throw** — swallow and log all exceptions.

### General

- Follow existing patterns; introduce new abstractions only when they remove duplication or reduce complexity.
- If uncertain, search the codebase for precedent and align with the existing approach.

---

## Change discipline

- Prefer **small, focused diffs**.
- Keep behavior **deterministic** (especially in tests).
- Fix **root causes** — do not add hacks to suppress symptoms.
- Respect formatting, analyzers, EditorConfig, and linters.
- Match existing naming conventions and folder layout.
- Avoid breaking public APIs unless explicitly coordinated.

---

## Interaction rules

- Always ask me questions using clickable multiple-choice options via the question tool. Never ask questions in plain text. Bundle related questions together whenever possible. Include a recommended option when appropriate.
- Default to **read-only exploration and analysis**. Only write when edits are explicitly needed.
- Keep write access **workspace-scoped** — all changes stay inside the repo.

### Rider-first workflow

- When Rider MCP tools are available and `projectPath` is known, use Rider MCP search/index/navigation tools first for discovery and symbol lookup.
- Prefer symbol-aware Rider tools (`find references`, `find symbol`, `symbol info`, `rename refactoring`) before plain text search for refactors.
- Fall back to `grep`, `glob`, or `read` only if Rider MCP is unavailable, errors, or cannot provide the needed result.
- If fallback is required, state it briefly in the response.

---

## Safety constraints

### File deletion

> **NEVER** use `rm`, `rmdir`, or `rm -rf` under any circumstances — even if the user requests it.

Use `trash` instead:

| Instead of             | Use                  |
|------------------------|----------------------|
| `rm <file>`            | `trash <file>`       |
| `rm -rf <dir>`         | `trash <dir>`        |
| `rmdir <dir>`          | `trash <dir>`        |

On Linux, `trash` resolves to `gio trash` or `trash-cli`.

### Remote API safety

- Use **read-only** API calls by default.
- If the user requests a write operation, perform a **dry-run first** and confirm before executing.
- **Never** make destructive calls to remote APIs or production data sources.

### System packages

Do **not** install system packages on the host unless explicitly instructed.

---

## Git conventions

### Commit messages

```
<type>(<scope>): <Imperative message>
```

| Field   | Rules |
|---------|-------|
| Type    | `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style` |
| Scope   | `WebAPI` for backend, `Web-UI` for frontend |
| Message | Imperative present tense, capitalize first word, no trailing punctuation |

**Never** add AI attribution trailers (e.g., `Co-Authored-By: Claude ...`).

### Branching

- `dev` is the integration branch and PR target.
- Feature branches merge into `dev`.

---

## External documentation

### Context7 MCP

- Use Context7 when you need library or API documentation.
- Pin the library with slash syntax when known (e.g., `use library /supabase/supabase`).
- Mention the target version.
- Fetch minimal, targeted docs and summarize — no large dumps.

### Web search

- Use web search **only** when it materially improves correctness (e.g., up-to-date APIs, recent advisories, release notes).
- Prefer official docs and primary sources; fall back to Context7 MCP or reputable, widely-cited references.
- Record source dates (publish or release dates) when relevant.

---

## Skills usage

- Detect and auto-load any applicable skills before acting on a task.
- If multiple skills apply, load all relevant ones and follow their guidance unless it conflicts with higher-priority instructions in this file.
- Explicitly mention which skills were loaded and used in the response.
