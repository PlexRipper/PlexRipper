---
name: reaparr-desktop-build
description: Use when deploying Reaparr to desktop, changing tools/Build, modifying desktop publish/package/run/CI package workflows, updating desktop runtime identifiers, Velopack packaging, AppHost desktop publish profiles, frontend public asset copying for desktop builds, or writing tests for the Reaparr Build project. This skill is mandatory for all tools/Build work.
---

# Reaparr Desktop Build

## Mandatory Scope

This skill is mandatory for any Reaparr task that touches desktop deployment or the Build project. Load it before reading, searching, editing, planning, testing, reviewing, or documenting anything under `tools/Build/`, and before changing desktop publish/package/run behavior elsewhere in the repo.

Always load `reaparr-mcp-tools` first. This skill adds the desktop-build-specific rules on top of the Reaparr MCP workflow.

Use this skill for:
- `tools/Build/**` code, tests, README, or project configuration.
- Desktop release, deploy, publish, package, run, CI package, or Velopack work.
- Desktop runtime identifier changes (`linux-x64`, `win-x64`, `osx-arm64`, etc.).
- AppHost desktop publish profile changes.
- Frontend public asset copying into desktop publish output.
- Unit tests for `tests/UnitTests/Build.UnitTests`.

Do not use this skill for frontend-only ClientApp work unless the task affects desktop build output generation or copied assets.

## Required Related Skills

Load these as needed after this skill:

| Task | Load next |
| --- | --- |
| C# implementation or refactor in `tools/Build` | `dotnet`, `modern-csharp`, `dotnet-best-practices` |
| Unit test implementation under `tests/UnitTests/Build.UnitTests` | `tunit`, `reaparr-backend-unit-tests` when using shared Reaparr test conventions |
| CLI parsing or command behavior changes | `dotnet`, then inspect Spectre.Console usage in `Program.cs` |
| External command execution behavior | `systematic-debugging` for failures, `test-driven-development` before fixes |

## Build Project Map

| Path | Role |
| --- | --- |
| `tools/Build/Program.cs` | Spectre.Console CLI registration for `reaparr-build desktop ...`. Uses strict parsing, case-insensitive commands, examples, and exception handling. |
| `tools/Build/Arguments/DesktopCommandSettings.cs` | Shared options for every desktop command. Keep argument names stable unless the README and tests are updated. |
| `tools/Build/Commands/*Command.cs` | Thin command adapters. They should delegate to `DesktopBuildWorkflow` and avoid embedding workflow logic. |
| `tools/Build/Workflows/DesktopBuildWorkflow.cs` | Top-level orchestration for publish, package, run, dry-run launch skipping, and Windows artifact export for `run --skip-package`. |
| `tools/Build/Workflows/DesktopPublishWorkflow.cs` | Version validation, frontend generation, restore, AppHost publish, and frontend output copy into `wwwroot`. |
| `tools/Build/Workflows/DesktopPackageWorkflow.cs` | Velopack packaging, artifact directory resolution/cleanup, default channel calculation. |
| `tools/Build/Workflows/DesktopLaunchWorkflow.cs` | Launches published executables, Linux AppImage mode, and Wine fallback for Windows builds on non-Windows hosts. |
| `tools/Build/_Shared/DesktopRuntimeCatalog.cs` | Supported desktop RIDs, publish profiles, and main executable names. |
| `tools/Build/_Shared/BuildPaths.cs` | Repository-root discovery and default AppHost, ClientApp, publish, and artifact paths. |
| `tools/Build/_Shared/DesktopCommandRunner.cs` | External command execution, dry-run behavior, PATH command discovery, and command logging. |
| `tools/Build/_Shared/FileSystemTasks.cs` | Recursive directory copy and artifact cleanup helpers. |
| `tools/Build/README.md` | User-facing command and argument reference. Keep it in sync with behavior. |

## Supported Commands

All commands live under `desktop` and share `DesktopCommandSettings`:

| Command | Alias | Expected behavior |
| --- | --- | --- |
| `desktop publish` | none | Generate/copy frontend assets and publish AppHost for a RID. |
| `desktop package` | `desktop pack` | Publish, then run Velopack packaging. |
| `desktop ci-package` | none | Force `SkipFrontend=true`, `SkipRestore=false`, `SkipPackage=false`; publish and package from pre-generated frontend assets. |
| `desktop run` | none | Publish/package as requested, then launch unless dry-run is enabled. |

## Critical Invariants

Preserve these unless the user explicitly asks to change behavior and the related code, documentation, and tests are updated in the same work:

1. `--version` and `--informational-version` are required before publish work starts.
2. `--rid` must resolve through `DesktopRuntimeCatalog`; arbitrary RIDs are unsupported.
3. `desktop ci-package` must force frontend skipping, restore enabled, and package enabled even if incoming settings differ.
4. `--dry-run` must log planned commands but avoid external execution, frontend copy, artifact cleanup, and launch execution.
5. Publish must use `--no-restore`; explicit restore is controlled only by `--skip-restore`.
6. Frontend generation is separate from frontend copy. `--skip-frontend` does not suppress copying pre-generated public assets.
7. Artifact cleanup happens before Velopack packaging unless `--preserve-existing-artifacts` or `--dry-run` is set.
8. Default Velopack channels are `<rid>-dev` when informational version contains `dev` case-insensitively, otherwise `<rid>-stable`.
9. Linux `--launch-mode packaged` must select the newest top-level `*.AppImage`, `chmod +x`, then execute it.
10. Windows runs on non-Windows hosts should use Wine when available and otherwise warn and exit `0`.
11. Windows `run --skip-package` should export the executable and full publish directory into the artifact directory.

## Argument Reference

| Option | Key behavior to preserve |
| --- | --- |
| `-r|--rid <RID>` | Selects runtime catalog entry, publish profile, executable name, publish path, artifact path, channel prefix, and launch behavior. |
| `--version <VERSION>` | Required; passed to MSBuild `Version` and Velopack `--packVersion`. |
| `--informational-version <VERSION>` | Required; passed to MSBuild `InformationalVersion`; influences default dev/stable channel. |
| `--channel <CHANNEL>` | Overrides default Velopack channel. |
| `--skip-frontend` | Skips Bun install/generate only; copied assets must still exist unless dry-run. |
| `--skip-restore` | Skips explicit `dotnet restore ... --runtime <rid>`; publish still uses `--no-restore`. |
| `--skip-package` | Only meaningful for `desktop run`; publishes and launches without Velopack packaging. |
| `--dry-run` | Logs commands and returns successful command exits without external side effects. |
| `--frontend-public-dir <PATH>` | Overrides copied public asset source; relative paths resolve from repo root. |
| `--artifact-dir <PATH>` | Overrides Velopack output/search/export directory; relative paths resolve from repo root. |
| `--preserve-existing-artifacts` | Disables pre-package artifact cleanup. |
| `--launch-mode <MODE>` | Linux packaged runs support `published` and `packaged`; default is `published`. |

## Change Workflow

1. Load `reaparr-mcp-tools`, then this skill.
2. Inspect `tools/Build/README.md` before behavior changes.
3. Use native repository reads, searches, diagnostics, and project commands first.
4. Keep command classes thin; put behavior in workflows or shared helpers.
5. If adding an option, update `DesktopCommandSettings`, workflow behavior, README, and unit tests.
6. If adding a RID, update runtime catalog, AppHost publish profiles, README, and packaging/launch tests.
7. Re-read changed files after edits.
8. Run the relevant Build unit tests through the Reaparr MCP test routing before claiming completion.

## Common Mistakes

- Loading only generic `.NET` skills and missing desktop-specific invariants.
- Treating `--skip-frontend` as if it also skips frontend asset copy.
- Making dry-run create/delete/copy files.
- Allowing stale artifacts to remain by default before packaging.
- Adding external command dependencies to unit tests.
- Changing CLI options without updating `README.md` and affected tests.
- Adding a runtime identifier without adding the matching publish profile and tests.
- Running package or publish commands as a substitute for unit tests.
