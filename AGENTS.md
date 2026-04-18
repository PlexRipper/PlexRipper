## Project overview

Reaparr is a cross-platform Plex media downloader.

| Layer    | Stack                                                                                   |
|----------|-----------------------------------------------------------------------------------------|
| Backend  | .NET 10, FastEndpoints, EF Core, Autofac, Quartz, SignalR (MessagePack), Serilog, Polly |
| Frontend | Nuxt 4 / Vue 3, Pinia, Quasar, PrimeVue                                                 |
| Testing  | TUnit, Shouldly, Moq, Bogus; Vitest, Cypress                                            |

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

> **`generate-ts` prerequisite:** The backend must be running in dev mode before executing `bun run generate-ts`. Use
> the Rider run configuration at `.run/Reaparr Back-End Development.run.xml`.

### Build and test commands

**Backend build:**

```bash
dotnet build Reaparr.sln
```

**Backend run:**

```bash
dotnet run --project src/AppHost
```

**Backend tests:**

```bash
dotnet run --project tests/UnitTests/<Project>.UnitTests/<Project>.UnitTests.csproj -- --no-ansi --disable-logo
```

Replace `<Project>` with the actual project name (e.g., `Application`, `BackgroundJobs`).

---

## Architecture patterns

### Backend

- **Endpoints:** Inherit from `BaseEndpoint<TRequest, TResponse>` (project-local base class). Never use `Endpoint<,>`
  directly.
- **CQRS:** Command/query records implement `ICommand<Result<T>>`; handlers implement
  `ICommandHandler<TCommand, TResult>`; dispatch via `ICommandExecutor`.
- **DI:** Autofac modules (`*Module : Module`) registered in `AppHost/_Shared/Config/Autofac/ContainerConfig.cs`.
- **Realtime:** SignalR typed hubs with MessagePack serialization. Broadcast via
  `IHubContext<THub, TClientInterface>.Clients.All`.
- **Jobs:** Quartz `IJob` with `[DisallowConcurrentExecution]`. Jobs use `JobDataMap` for parameters, dispatch via
  `ICommandExecutor`, and **must never throw** — swallow and log all exceptions.

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

- **All agents operating in this project** must ask questions using clickable multiple-choice options via the question
  tool — never plain-text lists. Bundle related questions together whenever possible. Include a recommended option when
  appropriate. If the `question` tool is unavailable (e.g., plan-mode or non-OpenCode client), fall back to a numbered
  list with a clear prompt asking the user to reply with a number.
- Default to **read-only exploration and analysis**. Only write when edits are explicitly needed.
- Keep write access **workspace-scoped** — all changes stay inside the repo.
- Store all generated plans under this repository’s `plans/` directory. Do not place plans in any external `.claude`
  directory or other out-of-repo location, regardless of which AI agent creates them.

### Rider-first workflow

> **MANDATORY**: At the start of every task, load the `jetbrains-skill` skill. This is non-negotiable — it enforces the
> correct tool selection order below.

**NEVER use `grep`, `glob`, `read`, or bash file commands as a first tool.** Rider MCP tools are always first when
available.

- Fall back to `grep`, `glob`, or `read` **only** if Rider MCP is unavailable, errors, or cannot provide the needed
  result.
- If fallback is required, **explicitly state it** before using the fallback tool.

---

## Safety constraints

### File deletion

> **NEVER** use `rm`, `rmdir`, or `rm -rf` under any circumstances — even if the user requests it.

Use `trash` instead:

| Instead of     | Use            |
|----------------|----------------|
| `rm <file>`    | `trash <file>` |
| `rm -rf <dir>` | `trash <dir>`  |
| `rmdir <dir>`  | `trash <dir>`  |

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

| Field   | Rules                                                                        |
|---------|------------------------------------------------------------------------------|
| Type    | `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style` |
| Scope   | `WebAPI` for backend, `Web-UI` for frontend                                  |
| Message | Imperative present tense, capitalize first word, no trailing punctuation     |

**Never** add AI attribution trailers (e.g., `Co-Authored-By: Claude ...`).

### Branching

- `dev` is the integration branch and PR target.
- Feature branches merge into `dev`.

---

## External documentation

### Context7 MCP

- **Always use Context7** for any library or API documentation lookup — no exceptions. Do not rely on training-time
  knowledge for library APIs; always fetch current docs via Context7.
- Pin the library with slash syntax when known (e.g., `use library /supabase/supabase`).
- Mention the target version.
- Fetch minimal, targeted docs and summarize — no large dumps.

### Web search

- Use web search **only** when it materially improves correctness (e.g., up-to-date APIs, recent advisories, release
  notes).
- Prefer official docs and primary sources; fall back to Context7 MCP or reputable, widely-cited references.
- Record source dates (publish or release dates) when relevant.

---

## Skills usage

- **Reaparr-specific skills take priority.** Before acting on any task in this project, check for a matching `reaparr-*`
  skill and load it first. These skills encode project-specific conventions that override generic guidance.
- Detect and auto-load any other applicable skills before acting on a task.
- If multiple skills apply, load all relevant ones and follow their guidance unless it conflicts with higher-priority
  instructions in this file.
- Explicitly mention which skills were loaded and used in the response.

---

## Agent reliability overrides

These overrides are mandatory. They exist to counter common failure modes during long refactors, large searches, and
multi-file edits.

### Known tool limits

- Do not treat a successful file write as proof that the change is correct. Bytes hitting disk is not verification.
- Long conversations and broad refactors increase context-loss risk. Re-read files instead of trusting memory.
- Large file reads can truncate. For practical purposes, treat files over 500 LOC as chunked-read candidates.
- Large tool outputs can truncate to previews. If a result count looks suspiciously small, narrow the scope and rerun.
- Text search is not semantic analysis. Grep can miss dynamic imports, string references, barrels, and type-only usage.

### Pre-work

- **Step 0 rule:** Before any structural refactor on a file over 300 LOC, first remove dead props, unused exports,
  unused imports, and obvious debug logging when it is safe to do so. Keep this cleanup as a separate phase. Only create
  a separate commit if the user explicitly asks for one.
- **Phased execution:** Do not attempt broad multi-file refactors in a single pass. Break work into explicit phases,
  verify each phase, and keep each phase to 5 touched files or fewer unless the user explicitly asks otherwise.

### Code quality

- **Senior dev override:** Do not hide behind the "minimum change" heuristic when the surrounding code is clearly
  inconsistent, duplicated, or structurally weak in a way that affects correctness or maintainability. Fix the real
  issue within scope.
- **Forced verification:** Do not report success after edits until you run the project-appropriate verification for the
  files you changed.

- Backend changes: ALWAYS use Rider IDE diagnostics first through MCP to verify and use run `dotnet build Reaparr.sln` as a last resort, or a narrower relevant build or test command when that is the better
  verifier.
- Frontend changes: ALWAYS use WebStorm IDE diagnostics first through MCP to verify. Prefer IDE diagnostics over a
  full backend build when the task is frontend-only. From `src/AppHost/ClientApp/`, run `bun run typecheck` and
  `bun run lint` only when a broader frontend verifier is still needed after IDE diagnostics.
- If a verifier does not exist or cannot run in the current environment, state that explicitly instead of implying
  success.

### Context management

- **Sub-agent swarming:** For tasks touching more than 5 independent files, split the work across parallel sub-agents in
  batches of roughly 5 to 8 files when tooling allows.
- **Context decay awareness:** After long conversations, after compression, or after substantial delay, re-read any file
  before editing it. Do not trust stale context.
- **File read budget:** For files over 500 LOC, read them in sequential chunks with offsets. Never assume a single read
  captured the whole file.
- **Tool result blindness:** If searches or command outputs look incomplete, rerun with narrower scope such as a single
  directory, tighter glob, or more targeted pattern, and state that truncation may have occurred.

### Edit safety

- **Edit integrity:** Before every file edit, re-read the file. After editing, read it again to confirm the intended
  change landed correctly. Do not batch more than 3 edits to the same file without a verification read.
- **No semantic search assumptions:** For any rename or signature change, search separately for direct calls, type
  references, string literals, dynamic imports, `require()` calls, re-exports, barrel files, test files, and mocks. Do
  not assume a single grep caught everything.
