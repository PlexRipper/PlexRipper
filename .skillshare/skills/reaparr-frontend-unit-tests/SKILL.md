---
name: reaparr-frontend-unit-tests
description: Use when creating or updating Vitest unit tests for the Reaparr frontend (Nuxt/Vue/Pinia/RxJS stores), especially for store setup, actions, getters, and RxJS observable flows that must follow the project's boilerplate, path alias, mock data, and naming conventions.
---

# Reaparr Frontend Unit Tests

## Native Tool Requirement

Use native repository tools (`read`, `edit`, `write`, `glob`, `lsp`, and short `bash` commands) for all frontend file operations, searches, symbol inspection, refactors, and diagnostics.

Retry a failed native tool once with a narrower request before changing approach.
---

## Overview

Use this skill to write frontend unit tests that match Reaparr conventions exactly.

**Working directory:** The frontend lives at `src/AppHost/ClientApp/` from the repo root. All paths in this skill are relative to that directory (e.g. `tests/nuxt/` means `src/AppHost/ClientApp/tests/nuxt/`).

Tests live under `tests/nuxt/` and run in the `nuxt` Vitest environment with a global auth setup file that pre-mocks common API endpoints. Always use `baseSetup`, `baseVars`, `getAxiosMock`, and `subscribeSpyTo` from `@services-test-base`. Store methods return RxJS Observables — always use `subscribeSpyTo` to interact with them.

## When to Use

Use this skill when:
- Writing or modifying TypeScript tests under `tests/nuxt/`.
- The subject is a Pinia store action, getter, or setup flow.
- The test involves mocking HTTP endpoints with `axios-mock-adapter`.
- You need to assert on RxJS observable emissions or completion.

Do not use this skill for backend C# tests (`tests/UnitTests/`) or Cypress E2E tests.

## File Placement

```
tests/nuxt/stores/<store-name>/<method-or-behavior>.test.ts
```

- One concern per file. `setup.test.ts` tests only the setup flow. Additional behaviors get their own file (e.g., `get-servers.test.ts`, `filter-media.test.ts`).
- Mirror the store name from `src/store/` (e.g., `serverStore` → `server-store/`).

## Test Quality Gate

Do not write tests merely to reach a requested count. A number like "add 20 tests" is a budget or lower bound, not the success criterion. First map the code under test, identify high-risk behavior, and choose tests that would catch meaningful regressions. If the requested count would force low-value tests, stop and report the highest-value test plan instead of padding.

Before adding tests, inspect existing tests for the same store/component and explicitly avoid duplicate coverage. Prefer behavior that crosses boundaries or encodes contracts:
- API request parameter contracts and omitted/default parameters
- RxJS success, failure, cancellation, and finalization paths
- cache invalidation, deduplication, pagination, and query-hash behavior
- interactions with other stores, settings, route query state, and generated API DTOs
- edge cases that previously failed or could plausibly regress

Reject weak tests such as:
- default value assertions that do not protect a behavior contract
- direct setter/getter tests with no observable consequence
- duplicating existing tests with different wording
- assertions that only prove mocks were configured
- broad "kitchen sink" tests added to inflate count

Every new test must earn its place by answering: "What bug would this fail for?" If the answer is unclear, replace it with a stronger test or do not add it.

## Required Boilerplate

Every test file must follow this exact shape:

```ts
import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
// Additional imports as needed:
// import { generateResultDTO, generatePlexServers } from '@mock';
// import { SomePaths } from '@api-urls';
// import { useXxxStore } from '@store';
// import { StoreNames, type ISetupResult } from '@interfaces';

describe('XxxStore.methodName()', () => {
  let { mock, config } = baseVars();

  beforeAll(() => {
    baseSetup();
  });

  beforeEach(() => {
    mock = getAxiosMock();
    setActivePinia(createPinia());
  });

  test('Should <expected outcome> when <condition>', async () => {
    // Arrange

    // Act

    // Assert
  });
});
```

**Rules:**
- Always destructure from `baseVars()` at describe scope. Reassign in `beforeEach`.
- Always call `baseSetup()` in `beforeAll`.
- Always call `getAxiosMock()` and `setActivePinia(createPinia())` in `beforeEach` — fresh mock and fresh Pinia per test.
- Always use `// Arrange`, `// Act`, `// Assert` comments.

## RxJS Observables (subscribeSpyTo)

Store methods return RxJS `Observable`. Never subscribe manually. Use `subscribeSpyTo` from `@services-test-base` (re-exported from `@hirez_io/observer-spy`):

```ts
// Await until observable completes
const result = subscribeSpyTo(store.someAction());
await result.onComplete();

// Read emissions
result.getFirstValue()    // first emitted value
result.getLastValue()     // last emitted value
result.getValues()        // all emitted values (array)
result.receivedComplete() // true if observable completed
```

**Always `await result.onComplete()` before asserting** unless you are testing intermediate emissions or synchronous observables.

## Store Setup Pattern

Before testing any store behavior, always initialise the store:

```ts
await subscribeSpyTo(store.setup()).onComplete();
```

For a `setup.test.ts` that just verifies the store initialises correctly:

```ts
const store = useXxxStore();
const setupResult: ISetupResult = {
  isSuccess: true,
  name: StoreNames.XxxStore,
};

const result = subscribeSpyTo(store.setup());
await result.onComplete();

expect(result.getFirstValue()).toEqual(setupResult);
expect(result.receivedComplete()).toEqual(true);
```

## HTTP Mocking

Use `mock` (axios-mock-adapter) assigned from `getAxiosMock()` in `beforeEach`. Wrap response values with `generateResultDTO`.

```ts
// Single reply
mock.onGet(PlexServerPaths.getAllPlexServersEndpoint())
  .reply(200, generateResultDTO(servers));

// One-time reply then a permanent reply (for setup + refresh flows)
mock.onGet(PlexServerPaths.getAllPlexServersEndpoint())
  .replyOnce(200, generateResultDTO([]))
  .onGet(PlexServerPaths.getAllPlexServersEndpoint())
  .reply(200, generateResultDTO(servers));

// Regex URL match
mock.onGet(new RegExp('/api/PlexMedia')).reply(200, generateResultDTO(data));
```

`generateResultDTO(value)` returns `{ value, isSuccess: true, statusCode: 200, errors: [], successes: [] }`.

## Pre-Mocked Endpoints (global-auth-setup.ts)

These are registered globally — do not re-mock them unless you need different behaviour:

| Endpoint | Default response |
|---|---|
| `GET /api/Authentication/status` | `{ isLoggedIn: true, userName: 'test-user', claims: [] }` |
| `GET /api/BackgroundJobs` | `[]` |
| `GET /api/PlexAccount` | `[]` |
| `GET /api/Download` | `[]` |
| `GET /api/FolderPath` | `[]` |
| `GET /api/PlexLibrary` | `[]` |
| `GET /api/PlexLibrary/sync-status` | `[]` |
| `GET /api/Notification` | `[]` |
| `GET /api/PlexServerConnection` | `[]` |
| `GET /api/PlexServer` | `[]` |
| `GET /api/Settings` | full default `SettingsModelDTO` |

Any endpoint **not** in this list that your test triggers must be explicitly mocked. The mock adapter is configured with `{ onNoMatch: 'throwException' }` — unmocked requests throw.

## Mock Data and Factories

Use `@mock` (barrel re-exporting factories, helpers, and interfaces) for all test data:

```ts
import { generateResultDTO, generatePlexServers, generateSettingsModel, Seed } from '@mock';
import { generateJobStatusUpdate } from '@factories';
```

Use a deterministic `config` seed so tests never produce random data:

```ts
config = {
  seed: 263,
  plexServerCount: 3,
  plexMovieLibraryCount: 2,
};
const seed = new Seed(config.seed!);
const plexServers = generatePlexServers({ config });
```

Available factory functions (non-exhaustive):

| Factory | Produces |
|---|---|
| `generatePlexServers({ config })` | `PlexServerDTO[]` |
| `generatePlexAccount({ id, plexServers, plexLibraries, config })` | `PlexAccountDTO` |
| `generatePlexLibrariesFromPlexServers({ seed, plexServers, config })` | `PlexLibraryDTO[]` |
| `generateSettingsModel({ config })` | `SettingsModelDTO` |
| `generateJobStatusUpdate({ jobType, jobStatus, data })` | `JobStatusUpdateDTO` |
| `generatePlexMediaSlims({ config, partialData })` | `PlexMediaSlimDTO[]` |
| `generateResultDTO(value)` | `ResultDTO<T>` |
| `generateFailedResultDTO(partial?)` | `BaseResultDTO` (failure) |

## Path Aliases

| Alias | Resolves to |
|---|---|
| `@services-test-base` | `tests/_base/base.ts` |
| `@store` | `src/store/index.ts` |
| `@mock` | `src/mock-data/index.ts` |
| `@factories` | `src/mock-data/factories/index.ts` |
| `@dto` | `src/types/api/generated/data-contracts.ts` |
| `@api-urls` | `src/types/api/api-paths.ts` |
| `@api/*` | `src/types/api/*` |
| `@interfaces` | `src/types/interfaces/index.ts` |
| `@class/*` | `src/types/class/*` |
| `@const/*` | `src/types/const/*` |
| `@enums/*` | `src/types/enums/*` |

Prefer `@api-urls` over `@api/api-paths` — they resolve to the same file.

## Naming Rules

- File: `<method-or-behavior>.test.ts` (kebab-case), e.g., `get-servers.test.ts`
- `describe`: `'StoreName.methodName()'` or `'StoreName - Behavior Group'`
- `test`: `'Should <verb phrase> when <condition>'`

## Commands

Run all frontend unit tests:

```bash
bun --cwd src/AppHost/ClientApp test
```

Run a specific test file:

```bash
bun --cwd src/AppHost/ClientApp vitest run tests/nuxt/stores/server-store/get-servers.test.ts
```

## Complete Example

```ts
import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PlexServerPaths } from '@api-urls';
import { useServerStore } from '@store';

describe('ServerStore.getServers()', () => {
  let { mock, config } = baseVars();

  beforeAll(() => {
    baseSetup();
  });

  beforeEach(() => {
    mock = getAxiosMock();
    setActivePinia(createPinia());
  });

  test('Should return all servers when servers are set in the store', async () => {
    // Arrange
    config = { plexServerCount: 3 };
    const serverStore = useServerStore();
    const servers = generatePlexServers({ config });
    mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO(servers));

    // Act
    await subscribeSpyTo(serverStore.setup()).onComplete();
    const result = serverStore.getServers();

    // Assert
    expect(result).toEqual(servers);
  });
});
```

## Common Mistakes

- Forgetting `await result.onComplete()` before asserting — emissions may not have arrived yet.
- Calling `getAxiosMock()` in `beforeAll` instead of `beforeEach` — mocks leak between tests.
- Not calling `serverStore.setup()` (or equivalent) before testing behavior that depends on loaded state.
- Using a non-deterministic seed (omitting `seed` from `config`) — tests become flaky across runs.
- Mocking a globally pre-mocked endpoint unnecessarily — leads to double-registration confusion.
- Leaving an endpoint unmocked that the store's setup flow calls — the `onNoMatch: throwException` adapter will throw during `setup()`, not your action under test.
- Placing tests under `tests/unit/` instead of `tests/nuxt/` — only the `nuxt` project matches `tests/nuxt/**`.
