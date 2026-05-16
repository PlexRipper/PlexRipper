---
name: reaparr-frontend-integration-tests
description: Use when creating, debugging, or stabilizing Reaparr frontend Cypress integration tests, especially for flaky CI failures, uncaught app exceptions, route/port issues, and deterministic test hardening under src/AppHost/ClientApp/cypress.
---

# Reaparr Frontend Integration Tests

## Overview

Use this skill for Cypress E2E/integration tests in Reaparr front-end.

Scope:
- `src/AppHost/ClientApp/cypress/**`
- CI workflow behavior related to `cypress:ci`

Primary goal: fix root causes (app crash, bad mocks, timing races), not just make assertions weaker.

## Test Runtime Model

Cypress runs against a generated static app:
- `bun run static-server` (internally runs `nuxi generate` + `serve`)
- Then Cypress runs Firefox headless against `http://localhost:$PORT`

If the app crashes in one spec, later specs may fail with `ECONNREFUSED` or wrong port churn.
Always fix earliest uncaught application error first.

## Required Execution Commands

From repo root:

```bash
bun --cwd src/AppHost/ClientApp run cypress:ci
```

Run one spec through the same CI entrypoint (recommended when debugging):

```bash
bun --cwd src/AppHost/ClientApp run cypress:ci --spec cypress/e2e/pages/authentication/sign-in-process.cy.ts
```

Run a focused folder through the same CI entrypoint:

```bash
bun --cwd src/AppHost/ClientApp run cypress:ci --spec "cypress/e2e/pages/dialogs/**/*.cy.ts"
```

## Known Reaparr Failure Patterns

### 1) `can't convert undefined to object` (app-side crash)
Usually caused by runtime assumptions in page/store setup (often missing required i18n keys or missing expected data shape in mocks).

Checklist:
- Verify recent i18n key edits in `src/lang/*.json` did not remove runtime-used keys.
- Verify `basePageSetup` config in spec provides required entities (accounts/servers/libraries/media).
- Verify mock endpoint payload shape matches DTO contract and app expectations.

### 2) 404 on `cy.visit('/empty')` in early spec
`/empty` exists in prerender output but can fail if server startup races.

Fix approach:
- Prefer `beforeEach` deterministic setup (not shared mutable setup in `before`).
- Ensure no prior test leaves app in broken state.

### 3) `ECONNREFUSED 127.0.0.1:3035` in later specs
This is usually a cascade after earlier crash, port conflict, or stale server process still running.

Fix approach:
- Stop stale frontend/static-server processes before re-running tests.
- Ensure only one test server owns `:$PORT` (default 3035) before starting Cypress.
- Fix the earliest failing spec/app crash and rerun from start.
- Do not chase every downstream ECONNREFUSED failure; they are often secondary.

## Stabilization Patterns for Cypress in Reaparr

### A. Replace fixed sleeps with condition-based waits

Bad:
```ts
cy.wait(1000)
```

Good:
```ts
cy.getCy('background-activity-button-badge', { timeout: 10000 }).should('exist')
```

### B. Alias and wait for network calls

```ts
cy.intercept('POST', AuthenticationPaths.appUserLoginEndpoint(), { ... }).as('loginSuccess')
cy.getCy('login-submit-button').click()
cy.wait('@loginSuccess')
```

### C. Guard async UI transitions

```ts
cy.getCy('setup-page-next-button', { timeout: 10000 }).should('not.be.disabled').click()
```

### D. Avoid float/text mismatch in progress assertions

```ts
const received = Math.round(i * (total / steps))
```

### E. Virtualized list assertions

```ts
cy.getCy(`media-table-row-${last}`, { timeout: 20000 })
  .scrollIntoView()
  .should('be.visible')
```

## Fake Data + API Intercept Pipeline (How Reaparr Cypress Works)

### High-level flow

1. A spec calls `cy.basePageSetup(config)`
2. The setup builds deterministic fake domain data (accounts, servers, libraries, media, downloads, settings)
3. Setup registers API interceptors for app endpoints (under `cypress/fixtures/api-mock/**`)
4. The app boots and requests `/api/*`
5. Interceptors return `generateResultDTO(...)` payloads that match generated DTO contracts
6. Test interacts with UI and optionally publishes SignalR/job progress events

### Core building blocks

- **Spec data bootstrap:** `cy.basePageSetup(...)`
- **Mock endpoint modules:** `cypress/fixtures/api-mock/*.mock-api.ts`
- **DTO wrappers/factories:** `@mock` helpers like `generateResultDTO`, factory generators
- **Route helper:** `route(...)` for page URLs
- **SignalR simulation:** `cy.hubPublish(...)` and `cy.hubPublishJobStatusUpdate(...)`

### Why this matters for debugging

If generated data shape and intercepted endpoint contracts diverge, app runtime code can throw (e.g., `can't convert undefined to object`) before assertions run.
Always verify:
- the endpoint requested by app is intercepted
- returned DTO shape matches what that page/store expects
- required translated keys used by components still exist in `src/lang/*.json`

## Mocking Rules

- Use `cy.basePageSetup(...)` as the foundation.
- Add spec-local intercepts only for behavior under test.
- For auth flows, always intercept login endpoint and auth status coherently.
- Keep DTO/result wrappers consistent with app contracts.
- When overriding one endpoint, do not accidentally remove baseline intercept behavior required by page setup.

## Workflow for Fixing CI Failures

1. Kill stale static-server/serve processes, then confirm `:$PORT` is free.
2. Run one failing spec locally via `run cypress:ci --spec ...`.
3. Fix root cause.
4. Re-run same spec until stable.
5. Re-run neighboring specs in same folder.
6. Re-run full `cypress:ci`.
7. If a new earliest failure appears, repeat from step 1.

## Anti-Patterns

- Ignoring first uncaught application exception and fixing later specs first.
- Adding broad `uncaught:exception` suppression for app errors instead of fixing root cause.
- Reliance on `cy.wait(<ms>)` for asynchronous state changes.
- Weakening assertions to hide genuine regressions.

## Done Criteria

A fix is complete when:
- Previously failing spec passes in isolation.
- No uncaught app exception remains at run start.
- Full `cypress:ci` run passes (or remaining failures are unrelated and documented).
