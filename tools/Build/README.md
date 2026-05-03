# Reaparr Build Tool

`tools/Build` contains the Reaparr desktop build CLI. It wraps the project-specific desktop publish, package, CI package, and run workflows behind a single Spectre.Console command application named `reaparr-build`.

The build tool is intended to be run from anywhere inside the Reaparr repository. At startup it walks up from the current working directory until it finds `Reaparr.sln`; that directory becomes the repository root used for all relative paths and default outputs.

## Command shape

Run the project with `dotnet run` and pass build-tool arguments after `--`:

```bash
dotnet run --project tools/Build -- desktop <command> [options]
```

When published as a standalone tool or executable, the configured application name is:

```bash
reaparr-build desktop <command> [options]
```

Parsing is strict and command names are case-insensitive. Unknown commands or options fail instead of being ignored.

## Desktop commands

All commands live under the `desktop` branch and share the same option set.

| Command | Alias | Purpose |
| --- | --- | --- |
| `desktop publish` | none | Generate frontend assets, restore the AppHost for a RID, publish the desktop AppHost, and copy frontend output into the published `wwwroot`. |
| `desktop package` | `desktop pack` | Run the full publish workflow and then create Velopack artifacts for the RID. |
| `desktop ci-package` | none | CI-focused package workflow. It assumes frontend assets were generated earlier, forces `--skip-frontend`, keeps restore/package enabled, then publishes and packages. |
| `desktop run` | none | Publish or package, then launch the desktop app when supported. |

## Required metadata

Every desktop command validates these two options before publishing:

- `--version <VERSION>`
- `--informational-version <VERSION>`

If either value is missing or blank, the command throws an error before running `dotnet publish`.

These values are passed to the AppHost publish as MSBuild properties:

```text
-p:Version=<VERSION>
-p:InformationalVersion=<VERSION>
```

## Supported runtime identifiers

`--rid` must be one of the RIDs known by `DesktopRuntimeCatalog`. Matching is case-insensitive.

| RID | Publish profile | Main executable |
| --- | --- | --- |
| `linux-x64` | `Desktop-linux-x64` | `Reaparr.AppHost` |
| `linux-arm64` | `Desktop-linux-arm64` | `Reaparr.AppHost` |
| `win-x64` | `Desktop-win-x64` | `Reaparr.AppHost.exe` |
| `win-arm64` | `Desktop-win-arm64` | `Reaparr.AppHost.exe` |
| `osx-x64` | `Desktop-osx-x64` | `Reaparr.AppHost` |
| `osx-arm64` | `Desktop-osx-arm64` | `Reaparr.AppHost` |

Unsupported RIDs fail with a message listing the supported values.

## Options and feature arguments

### `-r|--rid <RID>`

Selects the desktop runtime identifier. This controls:

- the AppHost publish profile, via `-p:PublishProfile=<profile>`
- the publish output directory
- the Velopack `--runtime` value
- the expected main executable name
- the default artifact directory
- the default Velopack channel name
- launch behavior for Linux and Windows outputs

This option is effectively required because the workflow immediately resolves it through `DesktopRuntimeCatalog.Get(...)`. An empty or unsupported value fails before publishing.

Example:

```bash
dotnet run --project tools/Build -- desktop publish --rid linux-x64 --version 0.0.1 --informational-version 0.0.1-local
```

### `--version <VERSION>`

Sets the package/application version. The value is passed to `dotnet publish` as `-p:Version=<VERSION>` and to Velopack as `--packVersion <VERSION>`.

This option is required for all desktop commands because `desktop package`, `desktop ci-package`, and `desktop run` all include the publish workflow, and `desktop publish` validates it directly.

Example:

```bash
--version 1.2.3
```

### `--informational-version <VERSION>`

Sets the informational version passed to `dotnet publish` as `-p:InformationalVersion=<VERSION>`. It is also used to infer the default Velopack channel when `--channel` is not supplied.

Default channel inference:

- if the informational version contains `dev` using case-insensitive matching, the channel is `<rid>-dev`
- otherwise, the channel is `<rid>-stable`

Examples:

```bash
--informational-version 1.2.3+abc123      # default channel: <rid>-stable
--informational-version 1.2.3-dev.4       # default channel: <rid>-dev
```

### `--channel <CHANNEL>`

Overrides the Velopack channel. When omitted, the build tool derives a RID-specific channel from `--informational-version`:

- stable release: `<rid>-stable`
- dev release: `<rid>-dev`

Examples:

```bash
--channel linux-x64-stable
--channel nightly
```

The channel is passed to `vpk pack` as `--channel <CHANNEL>`.

### `--skip-frontend`

Skips frontend generation. When this flag is not set, the publish workflow:

1. verifies `bun` is available, unless `--dry-run` is set
2. installs frontend dependencies with `bun install --frozen-lockfile` when `src/AppHost/ClientApp/bun.lock` exists and `src/AppHost/ClientApp/node_modules` does not exist
3. runs `bun run generate --fail-on-error` in `src/AppHost/ClientApp`

Important: skipping frontend generation does not skip the later frontend copy step. Unless `--dry-run` is set, the workflow still copies frontend public assets into the published `wwwroot`. Use `--frontend-public-dir` to point at pre-generated assets when using `--skip-frontend`.

`desktop ci-package` always forces this option to `true`, regardless of the command-line value.

### `--skip-restore`

Skips the explicit RID-specific restore step. When not skipped, publish runs:

```bash
dotnet restore src/AppHost/AppHost.csproj --runtime <rid>
```

The later publish command always includes `--no-restore`, so use `--skip-restore` only when the RID-specific restore has already been performed or the build environment otherwise guarantees restored assets.

`desktop ci-package` forces restore back on (`SkipRestore = false`).

### `--skip-package`

Affects `desktop run`. When set, `desktop run` publishes only and launches the published output directly. It does not run Velopack packaging.

For Windows RIDs, `desktop run --skip-package` also exports convenience artifacts after publish:

- copies the published executable into the artifact directory
- copies the full publish directory into `<artifact-dir>/publish`

This option has no effect on `desktop publish`, because that command never packages. It is not honored by `desktop package` or `desktop ci-package`; those commands call the package workflow directly after publish.

### `--dry-run`

Logs the commands that would be run without executing external commands or performing file-copy side effects that require build outputs.

Dry-run behavior includes:

- skips checking that `dotnet`, `bun`, and `vpk` exist
- `RunCommandAsync(...)` logs the command and returns without executing it
- `ExecuteCommandAsync(...)` logs the command and returns exit code `0`
- frontend output is not copied into the publish directory
- artifact cleanup is skipped
- launch is skipped by `desktop run` after publish/package planning

Dry-run still performs argument validation and still builds the command argument lists, making it useful for checking what the workflow would do.

### `--frontend-public-dir <PATH>`

Overrides the directory copied into the published `wwwroot`. The default is:

```text
src/AppHost/ClientApp/.output/public
```

Relative paths are resolved from the repository root, not from the shell working directory.

This option is especially important for CI and `--skip-frontend`, where frontend generation may happen in a separate job or earlier step. The source directory must exist unless `--dry-run` is set.

Example:

```bash
--frontend-public-dir .tmp/frontend-public
```

### `--artifact-dir <PATH>`

Overrides the directory used for Velopack artifacts. The default is RID-specific:

```text
.artifacts/<rid>
```

Relative paths are resolved from the repository root.

The directory is used as:

- the Velopack `--outputDir`
- the search location for Linux AppImage launch mode
- the Windows export location for `desktop run --skip-package`

Example:

```bash
--artifact-dir Releases
```

### `--preserve-existing-artifacts`

Controls cleanup of the artifact directory before packaging.

Default behavior is to clear the artifact directory before running `vpk pack`. With this flag set, the tool creates the directory if needed but preserves existing files.

This only applies when packaging is actually executed and `--dry-run` is not set.

Use this when a release process intentionally accumulates artifacts in the same output directory. Avoid it when you need a clean package output.

### `--launch-mode <MODE>`

Controls how `desktop run` launches Linux builds after packaging. The default is:

```text
published
```

Supported values for Linux packaged runs are:

| Value | Behavior |
| --- | --- |
| `published` | Launch the executable from the publish directory. |
| `packaged` | Find the newest `*.AppImage` in the artifact directory, run `chmod +x`, and execute the AppImage. |

Validation of `--launch-mode` is only strict for Linux RIDs when packaging was not skipped. In that case, any value other than `published` or `packaged` fails.

For non-Linux RIDs, and for Linux runs with `--skip-package`, the workflow launches the published executable path and logs the requested launch mode.

Example:

```bash
dotnet run --project tools/Build -- desktop run --rid linux-x64 --version 1.2.3 --informational-version 1.2.3-dev.1 --launch-mode packaged
```

## Workflow details

### Publish workflow

`desktop publish` performs these steps:

1. validate `--version` and `--informational-version`
2. require `dotnet`, unless `--dry-run` is set
3. generate frontend assets unless `--skip-frontend` is set
4. restore AppHost for the selected RID unless `--skip-restore` is set
5. publish AppHost with the selected publish profile
6. copy frontend public output into `<publish-dir>/wwwroot`, unless `--dry-run` is set

The publish command run by the tool is:

```bash
dotnet publish src/AppHost/AppHost.csproj \
  -p:PublishProfile=<profile> \
  -p:Version=<version> \
  -p:InformationalVersion=<informational-version> \
  -p:CSharpier_Bypass=true \
  --no-restore
```

Default publish directory:

```text
src/AppHost/bin/Publish/Desktop/<rid>
```

### Package workflow

`desktop package` and `desktop ci-package` run the publish workflow, then the package workflow.

When `--dry-run` is not set, package first requires `vpk` on `PATH`. It then clears the artifact directory unless `--preserve-existing-artifacts` is set.

Velopack is invoked as:

```bash
vpk pack \
  --packId Reaparr \
  --packTitle Reaparr \
  --packVersion <version> \
  --packDir <publish-dir> \
  --mainExe <main-executable> \
  --runtime <rid> \
  --channel <channel> \
  --outputDir <artifact-dir>
```

### CI package workflow

`desktop ci-package` is a specialized wrapper around the package workflow. It copies the supplied settings but forces:

```text
SkipFrontend = true
SkipRestore = false
SkipPackage = false
```

Use it when CI has already generated frontend output, but each RID job still needs to run restore, publish, and Velopack packaging.

Typical CI command:

```bash
dotnet run --project tools/Build -- desktop ci-package \
  --rid linux-x64 \
  --version 0.0.1 \
  --informational-version 0.0.1-dev.1 \
  --frontend-public-dir .tmp/frontend-public \
  --artifact-dir Releases
```

### Run workflow

`desktop run` performs publish/package work and then launches the output unless `--dry-run` is set.

Default behavior:

1. publish
2. package
3. launch published executable

With `--skip-package`:

1. publish
2. for Windows RIDs, export executable and full publish directory to the artifact directory
3. launch published executable

Linux packaged launch:

```bash
--launch-mode packaged
```

For Linux RIDs, when packaging is not skipped and launch mode is `packaged`, the tool finds the newest `*.AppImage` in the artifact directory, marks it executable with `chmod +x`, and launches it.

Windows launch on non-Windows hosts:

- if `wine` is available, the tool launches the published `.exe` through `wine`
- if `wine` is not available, the tool logs the publish/artifact locations, warns that the Windows build was not launched, and exits successfully with `0`

## Examples

Publish Linux x64:

```bash
dotnet run --project tools/Build -- desktop publish \
  --rid linux-x64 \
  --version 0.0.1 \
  --informational-version 0.0.1-local
```

Package macOS ARM64 without modifying artifacts:

```bash
dotnet run --project tools/Build -- desktop package \
  --rid osx-arm64 \
  --version 0.0.1 \
  --informational-version 0.0.1-local \
  --dry-run
```

Use the `pack` alias:

```bash
dotnet run --project tools/Build -- desktop pack \
  --rid win-x64 \
  --version 1.0.0 \
  --informational-version 1.0.0 \
  --channel win-x64-stable
```

Run Windows x64 from a non-Windows host with Wine when available:

```bash
dotnet run --project tools/Build -- desktop run \
  --rid win-x64 \
  --version 0.0.1 \
  --informational-version 0.0.1-local
```

Run Linux AppImage after packaging:

```bash
dotnet run --project tools/Build -- desktop run \
  --rid linux-x64 \
  --version 0.0.1 \
  --informational-version 0.0.1-dev.1 \
  --launch-mode packaged
```

Package in CI from pre-generated frontend assets:

```bash
dotnet run --project tools/Build -- desktop ci-package \
  --rid linux-x64 \
  --version 0.0.1 \
  --informational-version 0.0.1-dev.1 \
  --frontend-public-dir .tmp/frontend-public \
  --artifact-dir Releases
```

## External tool requirements

Depending on options and command, the build tool expects these executables on `PATH`:

| Tool | Required when |
| --- | --- |
| `dotnet` | Any non-dry-run publish workflow. |
| `bun` | Frontend generation is not skipped and this is not a dry run. |
| `vpk` | Packaging is performed and this is not a dry run. |
| `chmod` | Linux `desktop run --launch-mode packaged` executes an AppImage. |
| `wine` | Optional for launching Windows builds from non-Windows hosts. If missing, the run command logs a warning and exits `0`. |

## Default paths

All relative override paths are resolved from the repository root.

| Purpose | Default path |
| --- | --- |
| AppHost project | `src/AppHost/AppHost.csproj` |
| Client app directory | `src/AppHost/ClientApp` |
| Frontend public output | `src/AppHost/ClientApp/.output/public` |
| Publish output | `src/AppHost/bin/Publish/Desktop/<rid>` |
| Artifact output | `.artifacts/<rid>` |

## Output and cleanup behavior

- Frontend output is copied recursively into `<publish-dir>/wwwroot`, overwriting files with the same names.
- Packaging clears the artifact directory by default before invoking Velopack.
- `--preserve-existing-artifacts` disables artifact cleanup.
- Windows `desktop run --skip-package` exports the published executable and a full `publish` directory under the artifact directory.
- `--dry-run` avoids command execution, frontend copying, artifact cleanup, and launch execution.

## Troubleshooting

### `Unsupported desktop RID`

Use one of the supported RIDs listed above. The RID controls publish profiles and executable names, so arbitrary .NET RIDs are not accepted.

### `A build version is required` or `An informational version is required`

Pass both `--version` and `--informational-version`. They are required even for dry-runs.

### `Generated frontend output directory was not found`

The publish workflow could not find the frontend public directory to copy. Either run without `--skip-frontend` so the tool generates it, or pass `--frontend-public-dir` pointing at pre-generated frontend output.

### `Required command '<tool>' was not found on PATH`

Install the missing tool or update `PATH`. Use `--dry-run` to inspect commands without requiring external tools.

### No Linux AppImage was found

`desktop run --launch-mode packaged` looks for `*.AppImage` directly inside the artifact directory. Confirm packaging succeeded and that `--artifact-dir` points to the directory containing the AppImage.
