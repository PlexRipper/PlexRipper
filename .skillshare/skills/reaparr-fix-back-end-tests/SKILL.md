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

## Primary Test Runner

Use **dotnet-test-mcp** to discover and run tests. Do not rely on Rider's `rider_get_test_results` if it reports no session; that endpoint only receives runs tracked by its IntelliJ test-runner integration.

From `/mnt/PROJECTS/Reaparr`:

1. Discover the scope:
   - `dotnet-test-mcp_list_test_projects`
   - `dotnet-test-mcp_list_tests_summary`
2. Run the entire solution:
   - `dotnet-test-mcp_run_all_tests` with `workingDirectory: "/mnt/PROJECTS/Reaparr"` and `includeStackTrace: true`.
3. If a whole-solution run exceeds the MCP timeout, run each unit-test project independently with:
   - `dotnet-test-mcp_run_all_tests_for_project`
   - `workingDirectory: "/mnt/PROJECTS/Reaparr"`
   - `includeStackTrace: true`

Include integration-test projects in the failure inventory and final verification. A test-project run that times out does **not** establish that its tests passed; narrow the scope to classes or individual failures, or obtain the runner output through supported MCP tooling.

## Workflow

### 1. Capture the Failure Inventory

- Run all backend unit and integration tests (or every backend test project if required by timeout constraints).
- Record every failing test with its project, fully qualified name, assertion/exception, and stack trace.
- Group failures by common test fixture, production class, fake-data configuration, shared database utility, or other likely common root cause.
- Do not modify code until the complete failure inventory is understood, unless a single obvious common root cause prevents all useful execution.

### 2. Diagnose Using Rider MCP

For each failure group:

- Use Rider MCP to locate the failing test, its test base class, the system under test, and relevant fake-data or database helpers.
- Read the full test method and enough neighboring implementation to understand setup, act, and assertions.
- Trace data creation and persistence boundaries. In particular, verify test setup remains isolated under parallel execution and that entity data is actually seeded before queried.
- Inspect usages before changing shared helpers. A shared BaseTests change requires checking all affected test projects.
- Prefer test-specific deterministic setup when the production behavior is correct. Change production code only when the test exposes a real product defect.

### 3. Apply Focused Fixes

- Make the smallest focused diff that corrects the root cause.
- Preserve existing public APIs unless the user explicitly authorizes an API change.
- Do not use arbitrary waits, test-order dependencies, global mutable state, random data, retries, ignored tests, or broad exception swallowing.
- Keep existing test conventions from the applicable Reaparr unit-test or integration-test skill. For unit tests, this includes:
  - TUnit `[Test]`, `async Task`
  - Shouldly assertions
  - Moq verification where applicable
  - `// Arrange`, `// Act`, and `// Assert` markers in every modified test method
  - mock setups as the last operation in Arrange, immediately before Act

### 4. Validate Iteratively

After each logical fix:

1. Run the affected failing test class or test with dotnet-test-mcp where supported.
2. Run the complete affected unit-test or integration-test project.
3. Check changed-file diagnostics with Rider MCP.

When no failures remain in affected projects:

1. Run every backend unit-test and integration-test project, or run the entire solution if it completes within the MCP timeout.
2. Report precise outcomes: test/project counts, passed/failed counts, and any test runs not verifiably completed.
3. Use Rider MCP to inspect local VCS changes and report the exact files changed.

## Completion Criteria

Do not claim completion until:

- Every discovered backend unit-test and integration-test project has completed successfully with zero failed tests; and
- Rider diagnostics show no errors in changed files; and
- The final response names the root cause(s), the files changed, and exact verification results.

Do not commit changes unless the user explicitly asks.
