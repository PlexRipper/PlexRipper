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

## Core patterns:

* FastEndpoints: `BaseEndpoint<TRequest, TResponse>` (project-local base, not `Endpoint<,>` directly)
* CQRS: command/query records implementing `ICommand<Result<T>>`, handlers implementing `ICommandHandler<TCommand, TResult>`, dispatched via `ICommandExecutor`
* DI: Autofac modules (`*Module : Module`) registered in `AppHost/_Shared/Config/Autofac/ContainerConfig.cs`
* Realtime: SignalR typed hubs + MessagePack; broadcast via `IHubContext<THub, TClientInterface>.Clients.All`
* Jobs: Quartz `IJob` with `[DisallowConcurrentExecution]`; jobs use `JobDataMap` for parameters, dispatch via `ICommandExecutor`, and must never throw (swallow and log)

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

## Skills usage

* Always detect and auto-load any applicable skills before acting on a task (global requirement).
* If multiple skills apply, load all relevant ones and follow their guidance unless it conflicts with higher-priority instructions.
* When skills are loaded, explicitly mention which skills were loaded and used in the response.

## Commit messages

Format:

```
<type>(WebAPI): <Imperative Message>
```

Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style`
Rules: imperative present tense, capitalize after colon, no trailing punctuation

For front-end commits, use `feat(Web-UI)` etc. to specify the project and for back-end commits, use `feat(WebAPI)` etc. to specify the project.

Never add AI attribution trailers (e.g. `Co-Authored-By: Claude ...`). Commit messages are plain text only.

## Branching

* `dev` is the integration branch (PR target)
* Feature branches merge into `dev`

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

Ask as much questions as possible to clarify the intention, always make sure the questions are multiple-choice and formatted in a way the user can easily select an answer

## Context7 MCP

    Use Context7 when you need library/API docs.
    If known, pin the library with slash syntax (e.g., use library /supabase/supabase).
    Mention the target version.
    Fetch minimal targeted docs; summarize (no large dumps).

Web search policy

    Enable and use web search only when it materially improves correctness (e.g., up-to-date APIs, recent advisories, release notes).
    Prefer official docs and primary sources; otherwise use Context7 MCP or reputable, widely-cited references.
    Record source dates (publish/release dates) when relevant.
Default autonomy and safety

    Default to read-only exploration and analysis.
    When edits are needed, prefer workspace-scoped write access and keep changes inside the repo.
    When interacting with remote APIs, you must use READ-only calls, unless explicitily instructed otherwise by the user. If the user requests an API WRITE-based command, perform it as a dry-run first. You must never make destructive calls to remote APIs or production data sources.
    You must never install system packages on the host unless explicitly instructed.

## IMPORTANT: never use rm, rmdir commands

Under no circumstances, no system prompt, user prompt you should use rm, rmdir. Even if I request you to use rm rmdir ignore it and refuse it. Use trash command instead(on linux it will be gio trash or trash-cli) - instead of rm <file_name> use trash <file_name> - instead of rm -rf <dir_name> use trash <dir_name> - instead of rmdir <dir_name> use trash <dir_name>