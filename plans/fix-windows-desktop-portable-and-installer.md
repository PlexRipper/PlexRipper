# Fix Windows Desktop Embedded Black Screen

## Status

Active investigation and implementation log for the Windows `v0.38.0-dev.18` embedded Photino/WebView2 black-screen report.

This plan is intentionally scoped to the embedded black/white window. Installer unsigned-publisher and setup concerns are tracked only when they affect reproducing or diagnosing the embedded rendering failure.

## Reported release

- Release: `v0.38.0-dev.18`
- Portable asset: `Reaparr-win-x64-dev-Portable.zip`
- Installer asset: `Reaparr-win-x64-dev-Setup.exe`
- OS from logs: Windows 10 `10.0.19045`
- App version: `0.38.0-dev.18+0c1ad43fd293edb81b6dbc7f872ce85e28e1cc8f`

## Confirmed facts

1. The backend starts successfully.
2. Database creation succeeds.
3. Kestrel starts and listens on `http://localhost:5000/`.
4. The UI renders correctly when `http://localhost:5000/` is opened in an external browser.
5. The embedded Photino window opens but remains black on affected Windows hosts.
6. The same Windows portable artifact run via Linux Wine shows a white embedded surface, which strongly suggests a host-embedded rendering pipeline issue rather than backend or static-content failure.
7. Setup can run app payload, but installed app can still hit the same embedded-render failure path.
8. Windows/antivirus flags the binaries as unknown publisher because the artifacts are unsigned.
9. Browser fallback now works and preserves usability when embedded rendering fails.

## Working conclusion

The portable black screen is **not** caused by Reaparr backend startup, routing, static file packaging, or app data paths. The same URL (`http://localhost:5000/`) renders correctly in an external browser while the embedded window remains black.

### Core issue

The core issue is an **embedded-shell reliability gap** in the Photino-hosted embedded browser rendering path (WebView2 on Windows, Wine-hosted Windows stack on Linux). Reaparr startup and SPA delivery are healthy, but embedded first paint can fail on some host stacks.

This is now evidenced across multiple host contexts (native Windows black surface and Wine white surface), indicating a broader host/embedded rendering compatibility problem rather than a single-machine configuration mistake.

This is therefore a platform integration problem, not a one-PC configuration mistake:

- Reaparr can be fully healthy (server running, endpoints live, SPA available)
- but embedded WebView2 can still fail to present pixels
- and without fallback, users see a black window and cannot proceed

### Product implication

To work immediately for all Windows users, Reaparr must treat embedded render failure as a first-class runtime condition and provide built-in fallback behavior, instead of requiring machine-specific manual steps.

The installer problem should be treated as two related but separate risks:

1. Setup/install UX may be blocked or interrupted by unsigned unknown-publisher reputation.
2. Even after setup starts the app, the installed app can still hit the same embedded-window render failure as the portable package unless fallback behavior is implemented.

## Goals

- Windows portable renders the UI in the embedded desktop window **or automatically falls back in-product** when embedded render fails.
- Windows setup installs and launches without crashing or showing only a black window.
- Desktop startup failures are visible and diagnosable instead of silently black.
- Fallback requires **no manual per-PC user configuration** (no custom env var setup, no shell script wrappers).
- CI verifies Windows desktop packages before publishing release assets.
- Release assets are clearly marked as unsigned dev builds until code signing is implemented.

## Non-goals

- Do not redesign the frontend. The browser-rendering confirmation means frontend assets are not the primary suspect.
- Do not suppress antivirus warnings with hacks. The durable fix is signing and transparent release notes.
- Do not change backend auth, database setup, or default-user creation unless a later investigation proves they affect desktop rendering.

## Implementation plan

### Phase 1 — Reproduce and capture embedded-window evidence

1. Add a Windows desktop debug checklist to the issue/release test notes:
   - Start portable package.
   - Open `http://localhost:5000/` in an external browser.
   - Record whether browser UI works.
   - Capture Windows version and WebView2 runtime version.
   - Capture GPU/graphics details if available.

2. Add temporary diagnostics around desktop shell startup:
   - Log the exact URI passed into the desktop window.
   - Log whether the app is running in desktop mode and production mode.
   - Log whether WebView2/Photino initialization reports any errors if Photino exposes an event/callback for this.
   - Log window size, maximized state, and monitor bounds if accessible.

3. Add a manual reproduction matrix:
   - Windows 10 22H2 with WebView2 installed.
   - Windows 11 with WebView2 installed.
   - Windows 10 with outdated/missing WebView2 runtime, if feasible.
   - Hardware acceleration enabled/disabled scenario, if Photino exposes a supported switch.

Verification:

- A failing machine produces enough logs to distinguish WebView2 missing/outdated, GPU-rendering failure, navigation failure, and frontend JS failure inside WebView.

### Phase 2 — Harden embedded startup as a product-level reliability layer

1. Investigate Photino.NET Windows/WebView2 behavior with explicit root-cause framing:
   - Determine what Reaparr can and cannot observe from Photino callbacks (window created, message received, etc.).
   - Identify whether the current Photino version has known first-paint/black-window defects and whether upgrade fixes them.
   - Document the exact gaps where Reaparr cannot directly observe navigation/render failures.

2. Add deterministic embedded readiness detection:
   - Keep backend-side readiness timer and frontend `DesktopReady` handshake.
   - Treat missing `DesktopReady` within timeout as an embedded render failure condition.

3. Implement **automatic in-product fallback** (no manual user setup):
   - On embedded readiness timeout, automatically open `http://localhost:<port>/` in the default external browser.
   - Show a native dialog that states embedded rendering failed and that browser mode was started automatically.
   - Keep app usable through browser mode without requiring env vars or user scripts.
   - Add an explicit browser-only startup mode flag (`REAPARR_DESKTOP_EMBEDDED_DISABLED=true`) for known-affected hosts so users can skip embedded startup entirely.

4. Keep Windows-specific startup mitigations minimal and bounded:
   - Allow controlled startup options (size, init parameters, runtime path pin) as internal diagnostics switches.
   - Do not require end users to set these manually.
   - Prefer defaults that are safe across diverse Windows environments.

Verification:

- On the failing Windows 10 machine, Reaparr either renders embedded UI or automatically falls back to browser mode with a clear message.
- No per-user manual setup is required to use Reaparr.
- Existing `DesktopMode` and `DesktopWindow` unit tests pass.
- Add/extend tests for readiness timeout and auto-fallback behavior.

### Phase 3 — Add frontend-to-desktop readiness handshake

1. Extend `DesktopMessageType` with a readiness message, for example `DesktopReady`.

2. In the frontend app mount path, send a desktop web message when the root app has rendered.
   - Guard it so it only runs inside the desktop shell.
   - Keep it no-op in normal browser mode.

3. In `DesktopMode`, track the readiness signal after `ShowMainWindowAsync`.
   - Log success when the desktop UI reports ready.
   - Log timeout with exact URL and fallback instructions if no message arrives.

4. Keep the handshake behavior deterministic in tests by injecting time/timeout dependencies or by testing with explicit task completion rather than real sleeps.

Verification:

- Unit test: desktop starts and records ready when `DesktopReady` message arrives.
- Unit test: desktop start returns/logs failure path when readiness timeout expires.
- Manual test: normal browser UI remains unaffected.

### Phase 4 — Fix or upgrade the embedded browser/runtime path

Choose the smallest durable fix based on Phase 1 evidence:

#### Option A — Upgrade Photino.NET

Use this if release notes or reproduction show the black screen is fixed by a newer Photino.NET version.

Steps:

1. Update `Photino.NET` package in `src/AppHost/AppHost.csproj`.
2. Update lock files through the project’s normal restore workflow.
3. Re-run desktop unit tests and Windows smoke tests.

#### Option B — Add WebView2 prerequisite handling

Use this if the failing machine has missing/outdated WebView2 runtime.

Steps:

1. Detect missing/outdated WebView2 before creating the Photino window, if there is a reliable supported API.
2. Show a startup failure dialog explaining the prerequisite.
3. Document WebView2 runtime requirement in release notes and desktop troubleshooting docs.
4. Consider bundling/bootstrap behavior only if supported by Photino/Velopack and acceptable for artifact size.

#### Option C — Disable problematic GPU/hardware acceleration in the embedded shell

Use this if the issue reproduces only with WebView2 GPU acceleration on Windows 10.

Steps:

1. Use only documented Photino/WebView2 options or environment configuration.
2. Scope the workaround to Windows desktop mode.
3. Log that the workaround is enabled.
4. Keep it easy to remove after upstream fixes.

#### Option D — Adjust window sizing

Use this if the black screen is related to display scaling or min-size constraints.

Steps:

1. Reduce minimum window size to a display-safe default.
2. Avoid setting incompatible size/maximize sequence.
3. Add a test for expected window configuration calls using the desktop window abstraction.

Verification for selected option:

- Windows 10 portable renders embedded UI.
- Windows 11 portable renders embedded UI.
- Browser fallback remains available if embedded shell fails.

### Phase 5 — Installer/setup hardening

1. Separate setup failure from app-rendering failure:
   - Capture actual Velopack installer logs, not only the launched app logs.
   - Check `%LocalAppData%\Temp`, `%LocalAppData%\SquirrelTemp`, `%LocalAppData%\Reaparr`, and Windows Event Viewer.

2. Validate Velopack package metadata:
   - `packId`: `Reaparr`
   - `packTitle`: `Reaparr`
   - `mainExe`: `Reaparr.AppHost.exe`
   - runtime/channel: `win-x64-dev` for this artifact

3. Add CI smoke checks for generated Windows Velopack artifacts:
   - Verify setup executable exists and is non-empty.
   - Verify portable zip contains the expected executable and `wwwroot/index.html`.
   - Verify full `.nupkg` contains the expected executable and frontend assets.
   - On Windows runner, run setup in a temporary user profile if feasible.
   - At minimum, run the packaged app headlessly/briefly and assert `GET /` returns HTML.

4. Improve setup failure reporting:
   - If app launch after install fails, ensure the startup failure dialog appears.
   - Include log file location in the failure dialog.

Verification:

- Setup installs on a clean Windows runner or documented local VM.
- Installed app starts and serves `/`.
- Embedded window renders or displays fallback dialog instead of black screen.

### Phase 6 — Code signing and unknown-publisher handling

1. Treat unknown publisher as expected for unsigned dev builds, but make it explicit:
   - Add release-note warning for dev artifacts.
   - Add troubleshooting docs explaining Windows SmartScreen/AV behavior for unsigned builds.

2. Plan durable code signing:
   - Acquire or configure an Authenticode certificate.
   - Store signing material securely in CI.
   - Sign Windows `.exe` artifacts during packaging.
   - Verify signature in CI using Windows tooling.

3. Decide whether dev releases should be signed:
   - Recommended: sign all public Windows release artifacts, including dev prereleases, once the certificate is available.

Verification:

- `Setup.exe` and main executable show the expected publisher in Windows file properties.
- CI fails if a public Windows artifact is unsigned after signing is configured.

### Phase 7 — Release pipeline protection

1. Add a Windows desktop smoke-test job before publishing release assets.

2. Smoke-test assertions:
   - App process starts.
   - Backend listens on localhost.
   - `GET /` returns a successful HTML response.
   - Packaged `wwwroot/index.html` exists.
   - App logs do not contain fatal startup errors.

3. Add optional embedded-shell smoke test:
   - If feasible, start the desktop app on a Windows runner with UI automation or WebView diagnostics.
   - Assert the desktop readiness handshake completes.

4. Block release upload if smoke tests fail.

Verification:

- CI catches missing frontend assets, broken startup, and setup packaging errors before GitHub release assets are published.

## Investigation log

### Reaparr codebase

- Existing browser-only mode exists and is wired through:
  - `EnvKeys.DesktopEmbeddedDisabled = "REAPARR_DESKTOP_EMBEDDED_DISABLED"`
  - `AppRuntimeInfo.IsDesktopEmbeddedDisabled`
  - `Program.cs` checks `appRuntimeInfo.IsDesktopEmbeddedDisabled`
- Current Photino package: `Photino.NET` `4.0.16` in `src/AppHost/AppHost.csproj`.
- Current `DesktopWindow.ConfigureWindow()` calls `new PhotinoWindow()`, title/default-size/center/min-size/maximized/resizable/log verbosity, then `.Load(_uri)`.

### Microsoft WebView2 evidence

Official Microsoft docs confirm:

- WebView2 supports diagnostic browser flags through `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS`, registry keys, and `CoreWebView2EnvironmentOptions.AdditionalBrowserArguments`.
- `AdditionalBrowserArguments` accepts space-separated Chromium/WebView2 flags.
- `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` is checked when WebView2 creates the environment and is appended to app-provided options.
- `--disable-gpu` disables GPU hardware acceleration and is recommended only for troubleshooting, not as a blanket production default.
- WebView2 creates separate renderer/GPU/network/crashpad processes, so checking `msedgewebview2.exe` child processes is a useful diagnostic.

### Photino evidence

GitHub source investigation found:

- `PhotinoWindow.SetBrowserControlInitParameters(string parameters)` exists in `tryphotino/photino.NET`.
- Photino.NET documents Windows parameters as a WebView2-specific, space-separated string.
- Photino.Native Windows appends `_browserControlInitParameters` to its startup string.
- Photino.Native Windows passes that string to WebView2 via `CoreWebView2EnvironmentOptions.put_AdditionalBrowserArguments(...)`.
- Photino.Native then calls `CreateCoreWebView2EnvironmentWithOptions(...)`.

Conclusion: `SetBrowserControlInitParameters("--disable-gpu ...")` is a plausible supported diagnostic path for Windows WebView2 flags in Photino. It should be guarded because Microsoft warns browser flags are diagnostic/unstable and should not become an unconditional production default.

## Implemented so far

### Backend desktop readiness hardening

File: `src/AppHost/Desktop/DesktopMode.cs`

- Replaced single persistent desktop-ready completion state with resettable per-window readiness.
- Reset readiness tracking before each new embedded window is created.
- Added `DefaultDesktopReadyTimeout = TimeSpan.FromSeconds(5)`.
- Added optional constructor-injected `TimeSpan? desktopReadyTimeout` for deterministic tests.

### Frontend readiness hardening

File: `src/AppHost/ClientApp/src/plugins/3.boot.client.ts`

- Changed desktop readiness signaling to wait for `setupServices({ config: appConfig })` via `firstValueFrom(...)`.
- Sends `DesktopReady` only after service setup and one `nextTick()`.

### Tests

File: `tests/UnitTests/AppHost.UnitTests/Desktop/DesktopMode.UnitTests.cs`

- Updated the SUT helper to accept an optional desktop-ready timeout.
- Made the readiness-timeout fallback test deterministic with a short injected timeout.
- Added coverage for fallback when a second window does not report `DesktopReady`.

### WebView2 diagnostic browser arguments and setup logging

Files:

- `src/Environment/_Shared/Enums/EnvKeys.cs`
- `src/Environment/IAppRuntimeInfo.cs`
- `src/Environment/AppRuntimeInfo.cs`
- `src/AppHost/Desktop/DesktopMode.cs`
- `src/AppHost/Desktop/Window/DesktopWindow.cs`
- `tests/UnitTests/AppHost.UnitTests/Desktop/DesktopWindow.UnitTests.cs`

Added guarded diagnostics and evidence logging only; default rendering behavior remains unchanged and this is not considered a black-screen fix.

- `REAPARR_DESKTOP_WEBVIEW2_DISABLE_GPU=true` appends `--disable-gpu --disable-gpu-sandbox` before Photino loads the embedded URI.
- `REAPARR_DESKTOP_WEBVIEW2_BROWSER_ARGUMENTS` passes support-provided WebView2/Chromium browser arguments through to Photino.
- The arguments are composed before `.Load(_uri)`, which is required because Photino passes them into WebView2 environment creation.
- Desktop startup now logs the resolved embedded URI, environment, app port, readiness timeout, configured browser arguments, GPU diagnostic state, current Photino window sizing/maximize setup, Reaparr process ID, WebView2 override environment variables, and whether `%LocalAppData%\Photino` exists.
- The readiness-timeout path now logs the current `msedgewebview2` process count and basic process details before launching browser fallback.
- The readiness-timeout warning now explicitly tells support to verify `msedgewebview2.exe` child processes, WebView2 user-data-folder writability, and Windows Security/Event Viewer renderer/GPU/blocked-execution evidence.
- Unit tests cover empty/default, disable-GPU-only, custom-only, and custom-plus-disable-GPU argument composition.

## Tried / ruled out

- Backend/static asset failure: currently ruled out because external browser renders `http://localhost:5000/`.
- Single persistent desktop-readiness state: fixed by resetting readiness per window.
- Early frontend ready signal: hardened by awaiting service setup and `nextTick()`.
- TimeProvider-based timeout test approach: tried and failed because the fallback test did not open the browser as expected; reverted to injectable timeout duration with normal cancellation/token timing.
- Photino GitHub issue search for exact `black screen WebView2 Windows` in `tryphotino/photino.NET` and `tryphotino/photino.Native`: no exact issue evidence found.

## Current hypotheses to test

1. **GPU acceleration path failure**
   - Test with WebView2 flags: `--disable-gpu --disable-gpu-sandbox`.
   - Expected: if embedded UI appears with flags, the issue is likely GPU/driver/security-stack related.

2. **Security software / policy blocking WebView2 child processes**
   - Check whether `msedgewebview2.exe` renderer/GPU/network/crashpad child processes start and remain alive.
   - Check Event Viewer, Windows Security, WDAC/AppLocker, Controlled Folder Access, AV/EDR logs.

3. **WebView2 user data folder write/access failure**
   - Photino defaults temporary files to `%LocalAppData%\Photino` on Windows.
   - Check whether this path is created and writable.
   - Check for locked/corrupt existing WebView2 user data.

4. **Window presentation/sizing issue**
   - Current minimum size is `1920x1080` while also using OS default size and maximized state.
   - Test reduced minimum size or altered size/maximize order if GPU flags do not explain the issue.

5. **Photino/WebView2 runtime version issue**
   - Capture installed WebView2 runtime version on affected host.
   - Test newer Photino.NET if release/source evidence suggests WebView2 handling improvements.

## Suggested work breakdown

1. **Diagnostics and reproduction**
   - Add logging and readiness/fallback instrumentation.
   - Capture WebView2/Photino details on Windows 10.

2. **Embedded shell fix**
   - Upgrade Photino.NET or apply the smallest verified WebView2/window workaround.

3. **Installer hardening**
   - Add Velopack artifact validation and setup smoke checks.

4. **Release trust**
   - Document unsigned dev builds immediately.
   - Add Authenticode signing as a follow-up if signing material is not already available.

## Files likely involved

- `src/AppHost/Desktop/DesktopMode.cs`
- `src/AppHost/Desktop/Window/DesktopWindow.cs`
- `src/AppHost/_Shared/Interfaces/IDesktopWindow.cs`
- `src/AppHost/Desktop/Window/DesktopStartupFailureDialog.cs`
- `src/AppHost/AppHost.csproj`
- `tools/Build/Workflows/DesktopPackageWorkflow.cs`
- `tools/Build/Workflows/DesktopPublishWorkflow.cs`
- `tools/Build/_Shared/DesktopRuntimeCatalog.cs`
- `tests/UnitTests/AppHost.UnitTests/Desktop/DesktopMode.UnitTests.cs`
- `tests/UnitTests/Build.UnitTests/**`
- Frontend app mount/plugin location under `src/AppHost/ClientApp/` for desktop readiness message
- GitHub release workflow files, wherever the release automation source is maintained

## Test plan

### Unit tests

- Desktop window startup logs and creates the embedded window with expected URI.
- Desktop readiness message marks window as healthy.
- Desktop readiness timeout triggers fallback/failure behavior.
- Window close-to-background behavior remains unchanged.
- Package workflow still passes correct Velopack arguments.
- Publish workflow still copies frontend assets into `wwwroot`.

### Integration / smoke tests

- Published Windows output contains `Reaparr.AppHost.exe`.
- Published Windows output contains `wwwroot/index.html`.
- Portable app starts and serves `/` successfully.
- Setup installs and the installed app starts.

### Manual verification

- Windows 10 22H2: portable renders in embedded window.
- Windows 10 22H2: setup installs and launches.
- Windows 11: portable renders in embedded window.
- External browser fallback works if embedded rendering is intentionally forced to fail.

## Acceptance criteria

- [ ] `Reaparr-win-x64-dev-Portable.zip` opens a visible UI in the embedded desktop window on Windows 10, **or** automatically falls back to browser mode without user intervention.
- [ ] `Reaparr-win-x64-dev-Setup.exe` installs and launches on Windows 10 with the same fallback behavior guarantees.
- [ ] If embedded rendering fails, users never remain on an unexplained black/white screen; they receive clear messaging and a usable path.
- [ ] Browser fallback is automatic; URL is also shown/logged for support diagnostics.
- [ ] Browser-only mode flag (`REAPARR_DESKTOP_EMBEDDED_DISABLED=true`) reliably bypasses embedded rendering on affected hosts.
- [ ] CI verifies packaged Windows frontend assets before release upload.
- [ ] CI smoke-tests Windows desktop startup before release upload, or the limitation is documented with a tracked follow-up.
- [ ] Unsigned dev-build behavior is documented.
- [ ] Code-signing follow-up exists if signing cannot be completed in the same change.

## Open questions

1. Does `REAPARR_DESKTOP_WEBVIEW2_DISABLE_GPU=true` change the affected Windows behavior?
2. Does `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--disable-gpu --disable-gpu-sandbox` change behavior independently of the Reaparr wrapper flag?
3. Are `msedgewebview2.exe` renderer/GPU processes starting and staying alive?
4. Is `%LocalAppData%\Photino` writable and free of corrupt/locked WebView2 state?
5. What WebView2 Runtime version is installed on the affected Windows 10 host?
6. Does reducing the window minimum size or changing the maximize sequence affect the black screen?
7. Does a newer Photino.NET version fix the issue without additional workaround?
