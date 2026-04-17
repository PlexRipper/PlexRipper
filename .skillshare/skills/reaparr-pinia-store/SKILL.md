---
name: reaparr-pinia-store
description: Use when creating or updating Pinia stores in the Reaparr frontend (Nuxt/Vue/Pinia/RxJS stores). Overrides generic Pinia skills. Use for store setup, state, actions, getters, RxJS Observable flows, generated API endpoints, SignalR subscriptions, and HMR. All stores must follow the project's strict internal order, boilerplate, and naming conventions.
---

# Reaparr Pinia Store

> **This skill overrides generic Pinia skills for this project.**  
> Every store uses Composition API, RxJS Observables exclusively for async, and follows a strict internal declaration order.

---

## Strict Internal Order

Every store **must** declare its internals in this exact order — no exceptions:

1. `defaultState` constant
2. `state = reactive<IState>(cloneDeep(defaultState))`
3. Inter-store dependencies (`use*Store()` calls)
4. `const actions = { setup(), ..., $reset() }`
5. Private helper functions (plain `function` declarations, not inside `actions`)
6. `const getters = { ... }`
7. `return { ...toRefs(state), ...actions, ...getters }`
8. HMR block (`acceptHMRUpdate`)

---

## Canonical Template

```typescript
import Log from 'consola';
import { defineStore, acceptHMRUpdate } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { switchMap, tap, map, catchError } from 'rxjs/operators';
import type { MyDTO } from '@dto';
import { StoreNames, type ISetupResult } from '@interfaces';
import { myApi } from '@api';
import { RefreshDataType } from '@dto';
import { cloneDeep } from 'lodash-es';
import { useSignalrStore } from '@store';

interface IMyStoreState {
  items: MyDTO[];
}

export const useMyStore = defineStore(StoreNames.MyStore, () => {
  // 1. defaultState
  const defaultState: IMyStoreState = { items: [] };

  // 2. state
  const state = reactive<IMyStoreState>(cloneDeep(defaultState));

  // 3. Inter-store deps
  const signalRStore = useSignalrStore();

  // 4. Actions
  const actions = {
    setup(): Observable<ISetupResult> {
      signalRStore
        .getRefreshNotification(RefreshDataType.MyEntity)
        .pipe(switchMap(() => actions.refresh()))
        .subscribe();

      return fetchAndSetItems().pipe(
        map((result) => ({ name: StoreNames.MyStore, isSuccess: !!result?.isSuccess })),
        catchError((err) => {
          Log.error(err);
          return of({ name: StoreNames.MyStore, isSuccess: false });
        }),
      );
    },

    refresh(): Observable<MyDTO[]> {
      return fetchAndSetItems().pipe(map(() => [...state.items]));
    },

    $reset() {
      Object.assign(state, cloneDeep(defaultState));
    },
  };

  // 5. Private helpers
  function fetchAndSetItems() {
    return myApi.getAllEndpoint().pipe(
      tap((result) => {
        if (result.isSuccess) {
          state.items = result.value ?? [];
        }
      }),
    );
  }

  // 6. Getters
  const getters = {
    // Non-reactive lookup — plain function
    getItem: (id: number): MyDTO | null =>
      state.items.find((x) => x.id === id) ?? null,

    // Reactive computed getter
    getVisibleItems: computed((): MyDTO[] => state.items.filter((x) => x.isVisible)),
  };

  // 7. Return
  return { ...toRefs(state), ...actions, ...getters };
});

// 8. HMR
if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useMyStore, import.meta.hot));
}
```

---

## API Endpoints (Generated, Observable-First)

All API endpoints are **auto-generated** in `src/types/api/generated/` and **always return `Observable<T>`** — never Promises.

**Never** instantiate generated classes directly. Import the pre-instantiated singleton from `@api`:

```typescript
// ✅ Correct
import { plexServerApi, downloadApi } from '@api';

// ❌ Wrong — never do this
import { PlexServerApi } from '@api/generated/PlexServer';
const api = new PlexServerApi();
```

Every response shape is `{ isSuccess: boolean, value?: T }`. Always guard on `isSuccess`:

```typescript
myApi.getSomething(id).pipe(
  tap((result) => {
    if (result.isSuccess && result.value) {
      // safe to use result.value
    }
  }),
)
```

---

## RxJS Patterns

Reaparr stores are **heavily RxJS**. Use operators instead of async/await:

| Pattern | Use |
|---|---|
| Transform result | `map((r) => r.value)` |
| Side-effect (state update) | `tap((r) => { if (r.isSuccess) state.items = r.value; })` |
| Chain calls | `switchMap((r) => r.isSuccess ? otherApi.call() : of(r))` |
| Error recovery | `catchError((e) => { Log.error(e); return of(fallback); })` |
| Conditional chain | `switchMap((r) => r.isSuccess ? nextCall() : of(r))` |

**Never** subscribe inside actions — callers subscribe. The exception is `setup()`, which subscribes to SignalR notifications internally.

---

## State Mutation Rules

| Mutation | Pattern |
|---|---|
| Replace array | `state.items = newArray` |
| Update one element | `state.items.splice(idx, 1, updated)` — never `state.items[idx] = ...` |
| Add element | `state.items.push(item)` |
| Remove element | `state.items = state.items.filter(...)` |
| Reset | `Object.assign(state, cloneDeep(defaultState))` |

---

## Getters: Plain vs Computed

```typescript
const getters = {
  // Plain function — non-reactive, parameterized lookup
  getItem: (id: number): MyDTO | null =>
    state.items.find((x) => x.id === id) ?? null,

  // computed() — reactive, no parameters, used in templates
  getVisibleItems: computed((): MyDTO[] =>
    state.items.filter((x) => x.isVisible)
  ),
};
```

Use `computed()` only when the getter is reactive and used directly in templates without arguments.

---

## Naming & Registration

- Store export: `use<Name>Store` (e.g., `useServerStore`)
- Store ID: entry in `StoreNames` enum from `@interfaces` (e.g., `StoreNames.ServerStore`)
- File name: `<name>Store.ts` in `src/store/`
- Barrel export: add `export * from './<name>Store';` to `src/store/index.ts`

**Add `StoreNames` entry first** — the enum is in `@interfaces`, not auto-generated.

---

## Path Aliases

| Alias | Resolves to |
|---|---|
| `@api` | `src/types/api/` (generated API singletons at `index.ts`) |
| `@dto` | DTOs |
| `@interfaces` | Interfaces, enums (`StoreNames`, `ISetupResult`) |
| `@store` | Store barrel (`src/store/index.ts`) |

---

## Logging & Utilities

### Logging — always use `consola`

```typescript
import Log from 'consola'; // MUST be the first import in the file
```

Never use `console.log`, `console.error`, `console.warn`, or any other `console.*`. Use `Log.log`, `Log.error`, `Log.warn`, `Log.info`, etc.

### Utility functions — always prefer `lodash-es`

Before writing any custom utility (sort, clone, group, deduplicate, merge, pick, omit, …), check `lodash-es` first. Import only what you need:

```typescript
import { cloneDeep, orderBy, groupBy, uniqBy, merge, pick, omit } from 'lodash-es';
```

Never hand-roll implementations that lodash-es already provides.

---



| Mistake | Fix |
|---|---|
| Using `async/await` in actions | Replace with RxJS operators |
| Using `console.log` / `console.error` | Use `import Log from 'consola'` and `Log.log` / `Log.error` |
| Re-implementing utility functions | Prefer `lodash-es` — `cloneDeep`, `orderBy`, `groupBy`, `uniqBy`, `merge`, etc. |
| Calling `subscribe()` inside non-setup actions | Return the Observable; caller subscribes |
| `state.items[i] = x` for array element update | Use `state.items.splice(i, 1, x)` |
| Importing generated API class and newing it | Import singleton from `@api` |
| Missing `$reset()` | Always include; uses `Object.assign(state, cloneDeep(defaultState))` |
| Missing HMR block | Always add `acceptHMRUpdate` at file end |
| Wrong internal order | Follow the 8-step order above exactly |
| Forgetting `StoreNames` entry | Add to enum in `@interfaces` before wiring store |
