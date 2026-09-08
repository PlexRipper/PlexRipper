---
name: reaparr-fix-back-end-tests
description: Use when investigating and fixing failing Reaparr backend unit and integration tests across the solution with dotnet-test-mcp, especially when failures may span multiple test projects or shared BaseTests infrastructure.
---

# Fix Reaparr Backend Unit and Integration Tests

## Required First Skills

Before using this skill, load and follow:

1. `reaparr-backend`
2. `reaparr-backend-unit-tests` for work under `tests/UnitTests/`
3. `reaparr-backend-integration-tests` for work under `tests/IntegrationTests/`

Load the test-type-specific skill(s) that match the failures being addressed. These project-specific skills take precedence over this document. In particular, use Rider MCP first for all backend source, test, search, diagnostics, and edit operations.

## Goal

Identify, fix, and verify all failing backend tests under both `tests/UnitTests/` and `tests/IntegrationTests/`. Fix root causes rather than suppressing failures, weakening assertions, adding retries, or making test behavior non-deterministic.

Unless the user narrows the scope, finish and verify **all unit-test projects first**, then run and repair **all integration tests**. Do not switch phases while a known unit failure remains.

## Primary Test Runner

Use **dotnet-test-mcp** to discover and run tests. Do not rely on Rider's `rider_get_test_results` if it reports no session; that endpoint only receives runs tracked by its IntelliJ test-runner integration.

### Run discipline — mandatory

Test execution is for obtaining a failure inventory and validating a specific fix; it is **not** a substitute for diagnosis.

- Start each unconstrained backend-test task with one complete suite run using `dotnet-test-mcp_run_all_tests` unless the user explicitly confirms that a complete current baseline already ran or narrows the scope. A supplied failure list supplements the baseline; it does not replace it.
- Treat the complete-suite result as the baseline failure inventory: record every failure, then group and fix a coherent batch rather than chasing symptoms one at a time.
- After each logical fix batch, run only the tests/classes just fixed. Once that focused scope passes, rerun the complete backend suite before selecting the next failure batch. Repeat until the complete suite is clean.
- Never run test projects concurrently. .NET builds write shared `bin/` and `obj/` outputs in this repository; parallel runs can cause file-lock errors and missing-artifact failures that are infrastructure noise, not product-test failures.
- Run at most one test command at a time. Do not launch another until its result has returned or it has timed out.
- A timed-out complete or project run has no pass/fail value. Do not repeatedly rerun the same timed-out scope. Enumerate that project’s test classes with Rider, then run every class sequentially to obtain verifiable coverage; resume the focused-fix/full-suite loop when the broad scope can complete. Never translate a timeout into a pass.
- Treat dotnet-test-mcp `Outcome`, `TestCount`, and explicit failure details as authoritative. Its `Passed` counter may be inconsistent with `TestCount`; do not invent totals from inconsistent fields.
- After a failure inventory is known, diagnose and fix the current batch before running ad hoc unrelated scopes. The planned complete-suite rerun is allowed only after the focused fixed tests pass.
- Do not edit production or shared-test infrastructure while merely trying to expose more exception text. Read the relevant test, implementation, and registrations first. Diagnostic-only edits must be reverted before any fix is proposed.

From `/mnt/PROJECTS/Reaparr`, for the initial baseline and every planned full-suite rerun unless the user explicitly narrows the scope:

1. Discover the scope:
   - `dotnet-test-mcp_list_test_projects`
   - `dotnet-test-mcp_list_tests_summary`
2. Run the entire solution once:
   - `dotnet-test-mcp_run_all_tests` with `workingDirectory: "/mnt/PROJECTS/Reaparr"` and `includeStackTrace: true`.
3. Record the complete result as the current failure inventory. After focused fixes pass, repeat this same unfiltered run to find regressions and the next batch.
4. If a complete run times out, treat it as unverified. Run its test projects sequentially, then enumerate and run every class in each timed-out project before claiming coverage; do not use repeated broad retries as diagnosis.

Include integration-test projects in every baseline and final verification cycle. A test-project run that times out does **not** establish that its tests passed; narrow the scope to classes or individual failures, or obtain the runner output through supported MCP tooling.

## Workflow

### 1. Capture the Failure Inventory

- Keep any supplied or previously captured failure output as evidence, but establish a complete current baseline unless the user explicitly confirms one already ran or narrows the scope.
- Record every failing test with its project, fully qualified name, assertion/exception, and stack trace in the working notes before editing.
- Group failures by common test fixture, production class, fake-data configuration, shared database utility, or other likely common root cause.
- Do not modify code until the complete failure inventory is understood, unless a single obvious common root cause prevents all useful execution.
- Treat logs as evidence, not assertions. If logs show a transition completed but the assertion reads old state, inspect asynchronous completion and EF tracking before changing production behavior.
- Build failures caused by another simultaneous test/build process (locked `bin/obj` files or missing copied artifacts) are not a test failure inventory. Wait for competing work to stop and then run only the required focused scope.

### 2. Diagnose Using Rider MCP

For each failure group:

- Use Rider MCP to locate the failing test, its test base class, the system under test, and relevant fake-data or database helpers.
- Read the full test method and enough neighboring implementation to understand setup, act, and assertions.
- Trace data creation and persistence boundaries. In particular, verify test setup remains isolated under parallel execution and that entity data is actually seeded before queried.
- Inspect usages before changing shared helpers. A shared BaseTests change requires checking all affected test projects.
- Prefer test-specific deterministic setup when the production behavior is correct. Change production code only when the test exposes a real product defect.
- For Quartz failures, inspect the ownership boundary before changing assertions:
  - job implementations commonly convert exceptions into `BackgroundJobResult` instead of rethrowing;
  - listeners may own queue transitions previously performed by jobs;
  - `MergedJobDataMap` is the runtime payload source and must be configured in job/endpoint tests;
  - JSON arrays and objects in `JobDataMap` must round-trip as JSON values, not quoted strings.
- For integration-host boot failures, inspect the first boot exception. A later disposed Autofac scope is often fallout from host termination, not the root cause.
- Keep integration infrastructure internally consistent:
  - register `QuartzModule.TestQuartzConfiguration()` so integration tests use `RAMJobStore`, never production persistent Quartz tables;
  - configure the active `WebApplicationFactory` through `ConfigureWebHost`; do not discard the derived factory returned by `WithWebHostBuilder`;
  - apply `OverrideAppBuildInfo` and runtime overrides during pre-host database/filesystem setup as well as host registration, so both phases resolve identical paths.
- Replacing application-wide `ICommandExecutor` is high risk because boot, shutdown, and endpoint commands share it. Prefer the real executor with a narrowly mocked downstream dependency. If replacement is unavoidable, intercept only the target command and verify boot remains real and successful.

### 3. Apply Focused Fixes

- Make the smallest focused diff that corrects the root cause.
- Preserve existing public APIs unless the user explicitly authorizes an API change.
- Do not use arbitrary waits, test-order dependencies, global mutable state, random data, retries, ignored tests, or broad exception swallowing.
- Mock the exact cancellation token supplied by the runtime context. Do not silently verify `CancellationToken.None` when the fixture provides another token.
- For strict mocks, configure every interaction in the current production path, including status notifications and scheduler existence/active-key checks. Remove expectations only after tracing confirms ownership moved elsewhere.
- For Quartz contexts whose result is asserted, use a property-capable mock (`SetupProperty`) so production `SetResult(...)` calls are observable.
- Keep existing test conventions from the applicable Reaparr unit-test or integration-test skill. For unit tests, this includes:
  - TUnit `[Test]`, `async Task`
  - Shouldly assertions
  - Moq verification where applicable
  - `// Arrange`, `// Act`, and `// Assert` markers in every modified test method
  - mock setups as the last operation in Arrange, immediately before Act

### 4. Validate Iteratively

After each logical fix batch:

1. Run only the affected failing test(s) or class(es) with dotnet-test-mcp where supported.
2. For asynchronous jobs, prove completion through the contract’s observable terminal signal (for example, a terminal patch/result), then query persistence through a newly resolved DbContext. Scheduler idleness alone does not prove all queued status persistence completed, and a long-lived EF context may return a stale tracked entity.
3. Once the focused scope passes, rerun the complete unfiltered backend suite — sequentially through the supported MCP runner — before selecting the next failure batch. A focused pass is not an overall pass.
4. Check every edited code file with Rider `get_file_problems`.
5. Run Rider `build_solution` after compilation-affecting changes. A shell build is not a substitute.

When no failures remain:

1. Require a final unfiltered complete backend suite run after the last fix. If it cannot complete, enumerate and pass every class in each timed-out project before claiming coverage.
2. Report precise outcomes: test/project counts, passed/failed counts, and any test runs not verifiably completed.
3. Use Rider MCP to inspect local VCS changes and report the exact files changed.

If failure output is truncated, retrieve the saved tool output or rerun the single failing test with stack traces. Do not guess at shared infrastructure. Ask the user only when supported tooling cannot recover the missing evidence.

## Completion Criteria

Do not claim completion until:

- Every discovered backend unit-test and integration-test project has completed successfully with zero failed tests, **or** every class in a project that cannot complete within the MCP timeout has been individually enumerated and passed;
- No task is marked complete while its required verification is failing, partial, or timed out;
- Rider diagnostics show no errors in every changed code file;
- Rider solution build succeeds after compilation-affecting changes;
- Diagnostic-only edits and rejected hypotheses are reverted;
- The final response distinguishes project-level passes from class-level verification and names the root causes, exact files changed, exact verified counts, timeouts, and unverified scope.

A focused passing test proves only that focused scope. A successful build proves compilation only. Neither permits an “all tests pass” claim.

Do not commit changes unless the user explicitly asks.
