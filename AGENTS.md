## Project overview

Reaparr is a cross-platform Plex media downloader hosted on GitHub: https://github.com/Reaparr/Reaparr

| Layer    | Stack                                                                                   |
|----------|-----------------------------------------------------------------------------------------|
| Backend  | .NET 10, FastEndpoints, EF Core, Autofac, Quartz, SignalR (MessagePack), Serilog, Polly |
| Frontend | Nuxt 4 / Vue 3, Pinia, Quasar, PrimeVue                                                 |
| Testing  | TUnit, Shouldly, Moq, Bogus; Vitest, Cypress                                            |

---

## Skills usage

- **ALWAYS** load the following skills at the start of every task — no exceptions:
  - `karpathy-guidelines`
    - It encodes behavioral guidelines to
      reduce common LLM coding mistakes: avoid overcomplication, make surgical changes, surface assumptions, and define verifiable success criteria.
  - `reaparr-mcp-tools`
    - It defines how to use MCP tools in this project, including mandatory execution routing through MCP servers and required verification steps.
- **Reaparr-specific skills take priority.** Before acting on any task in this project, check for a matching `reaparr-*` skill and load it first. These skills encode project-specific conventions that override generic guidance.
- Detect and auto-load any other applicable skills before acting on a task.
- If multiple skills apply, load all relevant ones and follow their guidance unless it conflicts with higher-priority
  instructions in this file.
- Explicitly mention which skills were loaded and used in the response.

---

## Change discipline

- Prefer **small, focused diffs**.
- Keep behavior **deterministic** (especially in tests).
- Fix **root causes** — do not add hacks to suppress symptoms.
- Respect formatting, analyzers, EditorConfig, and linters.
- Match existing naming conventions and folder layout.
- Avoid breaking public APIs unless explicitly coordinated.

---

## Safety constraints

> **NEVER** install system packages on the host unless explicitly instructed.

> **NEVER** use `rm`, `rmdir`, or `rm -rf` under any circumstances — even if the user requests it.

Use `trash` instead:

| Instead of     | Use            |
|----------------|----------------|
| `rm <file>`    | `trash <file>` |
| `rm -rf <dir>` | `trash <dir>`  |
| `rmdir <dir>`  | `trash <dir>`  |

On Linux, `trash` resolves to `gio trash` or `trash-cli`.

---
### Backend (`src/`)

If working on the backend, then load `reaparr-backend` skill for project-specific backend conventions and `dotnet-devtools` for .NET development best practices.

### Frontend (`src/AppHost/ClientApp/`)

If working on the frontend, then load `reaparr-frontend` skill first for project-specific frontend conventions. This umbrella skill must be loaded before narrower frontend skills such as `reaparr-frontend-components`, `reaparr-pinia-store`, or `reaparr-frontend-unit-tests`.

> **Package manager:** The frontend uses **Bun exclusively** — never use npm, yarn, or pnpm.

> **`generate-ts` prerequisite:** The backend must be running in dev mode before executing `bun run generate-ts`. Use
> the Rider run configuration at `.run/Reaparr Back-End Development.run.xml`.


---

## Interaction rules

- **All agents operating in this project** must ask questions using clickable multiple-choice options via the question
  tool — never plain-text lists. Bundle related questions together whenever possible. Include a recommended option when appropriate. 
- Default to **read-only exploration and analysis**. Only write when edits are explicitly needed.
- Store all generated plans under this repository’s `plans/` directory. Do not place plans in any external `.claude`
  directory or other out-of-repo location, regardless of which AI agent creates them.

---

