---
name: reaparr-frontend-integration-tests
description: Use when creating, debugging, or stabilizing Reaparr frontend Cypress integration tests under src/AppHost/ClientApp/cypress, especially when tests cover user workflows, API state transitions, SignalR progress, validation, or flaky CI behavior.
---

# Reaparr Frontend Integration Tests

Use this skill for Reaparr Cypress E2E/integration tests under:

- `src/AppHost/ClientApp/cypress/**`
- frontend CI behavior related to `cypress:ci`

## Core test model

Cypress tests are complete user workflows. Prefer a small number of meaningful journeys over a large matrix of tests that repeat the same setup and assert one implementation state each.

A good workflow proves:

1. The user starts from a deterministic page state.
2. The user performs the real UI action.
3. The browser sends the expected request.
4. The mocked backend returns a contract-shaped response.
5. The UI renders the intermediate and final outcome.
6. A refresh or subsequent read observes the changed state.

Keep the user actions visible in the test. Small assertion or fixture helpers are acceptable when they do not hide the workflow; do not hide an entire journey behind a shared helper.

## Workflow sizing

- Keep fewer total workflows than individual state cases.
- Combine related states in one ordered workflow when they belong to the same user journey.
- Do not create one test per enum value, progress stage, or identical error branch.
- Put failure and recovery variants into the complete workflow when they are part of the feature contract.
- Split files when independent domains or products have genuinely different journeys; do not duplicate a generic state matrix merely to increase test count.
- Keep every workflow independently bootstrappable through `beforeEach` setup.

## Arrange deterministic state

1. Start with `cy.basePageSetup(...)`.
2. Register spec-local intercepts only for the behavior under test.
3. Use generated DTO/result factories such as `generateResultDTO(...)` so responses match the current contracts.
4. Use typed/generated route helpers where available.
5. Model writes and refreshes as stateful intercepts. A successful create, update, or setup request must change the data returned by the next list/detail request.
6. Assert the initial state before changing it when that state is part of the workflow, such as an unconfigured item, an empty list, or disabled actions.

A static GET response that always returns the original fixture is insufficient for a persistence workflow: it can let a test pass even when the application never updates or refreshes its state.

## UI-first workflow rules

- Trigger create, edit, test-connection, save, setup, and close actions through the UI.
- Do not call stores directly or mutate application state to advance a workflow.
- Prefer stable `data-cy` selectors and user-visible text where appropriate.
- Test validation on the actual controls: make the input invalid through the UI, blur or select as a user would, assert the field message, and assert the relevant action remains disabled.
- Before a valid save, prove that no persistence request was sent; after the valid action, assert exactly one expected request.
- Assert request method, important request fields, response status/data, and the resulting visible UI state.
- For a connection check, assert the request and visible result before continuing to save.
- For a mutation, assert the refreshed list/card/detail state rather than stopping at the network response.

Avoid assuming that every `data-cy` target is a native `<input>` or that every control supports `.clear()`/`.focus()`. Use the control's real interaction path and assert observable validation state without depending on private component DOM structure.

## SignalR and backend progress

When a workflow depends on backend SignalR updates, use the project's hub-publishing commands—especially `cy.hubPublish(...)`—to simulate those backend messages. Do not mutate the store, component state, or DOM directly.

The workflow must start the real UI action that subscribes to the backend progress first. Then:

1. Publish a typed backend progress payload.
2. Wait through a retryable visible assertion for the expected UI state.
3. Assert the relevant status and user-visible text.
4. Continue with the next backend event.

Do not use arbitrary sleeps as a SignalR synchronization mechanism. A visible state assertion is the synchronization point.

### Ordered progress workflows

Keep progress coverage in one ordered flow when the stages form one user journey:

- Exercise meaningful failure/recovery transitions in sequence instead of creating one test per stage.
- For a recoverable stage, publish failure, assert its error status/text, assert earlier stages remain successful and later stages remain pending, then publish success and assert recovery.
- Treat terminal completion as the terminal success milestone unless the product contract explicitly supports terminal failure/retry.
- Assert intermediate status and meaningful visible text, then assert the final request and final page/card state.
- Foreign-event or unknown-stage filtering is separate regression coverage, not a default reason to multiply the main workflow. Add it only when explicitly required by the feature or bug.

## Async and synchronization

Use Cypress retryability and named aliases:

```ts
cy.intercept('POST', endpoint, response).as('saveItem')
cy.getCy('save-button').should('not.be.disabled').click()
cy.wait('@saveItem')
cy.getCy('item-card').should('contain.text', 'Saved')
```

Prefer:

- network aliases for requests the UI must make;
- visible state assertions after queued hub messages;
- `should()` assertions for retryability;
- explicit readiness assertions before clicks.

Avoid fixed sleeps, promise/`async` control flow mixed into Cypress commands, and assertions that can pass before an asynchronous backend message is processed.

## Reaparr runtime model

The CI flow generates a static app and runs Firefox headless:

- `bun run static-server` runs `nuxi generate` and serves the generated output.
- Cypress runs against `http://localhost:$PORT`.
- The frontend package manager is Bun.

Use the configured WebStorm Cypress run configuration first. The equivalent focused command from the repository root is:

```bash
bun --cwd src/AppHost/ClientApp run cypress:ci --spec cypress/e2e/path/to/spec.cy.ts
```

For a broader check:

```bash
bun --cwd src/AppHost/ClientApp run cypress:ci
```

## Failure diagnosis

Fix the earliest real failure first.

### App-side crash

For errors such as `can't convert undefined to object`:

- inspect the first uncaught application error;
- verify required `basePageSetup` data exists;
- verify intercepted DTO shapes match generated contracts;
- verify required translation keys still exist.

Do not suppress application failures with broad `uncaught:exception` handlers.

### Port or server cascade

For `ECONNREFUSED` or changing-port failures:

- check for stale static-server processes;
- ensure one test server owns the configured port;
- repair the earliest failing spec before chasing later failures.

### Timing failure

Replace sleeps with a request alias or visible UI condition. If a hub message is queued, publish it and wait for the UI state it must produce before asserting dependent state.

## Required verification

After editing Cypress files:

1. Re-read every changed spec.
2. Run WebStorm `get_file_problems` for every changed spec.
3. Run WebStorm lint/inspection checks and fix errors.
4. Run each changed spec independently through the same `cypress:ci` path used by CI.
5. Run the wider Cypress scope when the change affects shared fixtures, support commands, or multiple neighboring workflows.
6. Report unrelated pre-existing failures separately from failures caused by the change.

## Completion criteria

A Cypress integration-test change is complete only when:

- the intended user workflows pass in isolation;
- the workflow count remains focused rather than one-test-per-state;
- validation, request/response, refresh, and final visible state are asserted where relevant;
- SignalR updates are simulated through hub-publishing commands;
- no arbitrary sleeps or broad exception suppression were added;
- WebStorm diagnostics/lint report no errors for changed specs;
- duplicate or obsolete specs are removed when the workflow has been consolidated.
