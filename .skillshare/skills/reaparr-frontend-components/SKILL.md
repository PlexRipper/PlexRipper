---
name: reaparr-frontend-components
description: "ALWAYS load for ANY frontend work in Reaparr — editing .vue files, composables, pages, stores, or any file under src/AppHost/ClientApp/. Defines mandatory standards: SFC block order, Nuxt auto-imports (what to omit), get/set for refs, useSubscription for RxJS, consola logging, Pinia-first logic, props/emits typing, styling, and cypress data-cy attributes."
---

# Reaparr Frontend Component Standards

## IDE Tool Requirement

**All frontend file operations and diagnostics MUST use WebStorm MCP tools** (`webstorm-official-mcp_*`, `webstorm-index-mcp_*`, `webstorm-index_ide_*`).

Never use Rider MCP tools for any work under `src/AppHost/ClientApp/`.

---

## Overview

Standards for Vue 3 SFC components in the Reaparr frontend (`src/AppHost/ClientApp/`). This is a **pure SPA** (SSR disabled), using Nuxt 4 auto-imports, Quasar + PrimeVue UI, Pinia stores, and RxJS for async data flow.

**Key principle:** Prefer logic and state in the Pinia store layer. Components should be thin orchestration surfaces.

---

## SFC Block Order

**Template FIRST**, then script — this is the project convention (opposite of Vue best-practices default):

```vue
<template>
  <!-- markup here -->
</template>

<script setup lang="ts">
// script here
</script>

<style lang="scss">
/* optional styles */
</style>
```

- Always `<script setup lang="ts">` — never Options API.
- `BaseButton.vue` is a legacy exception using Options API render function — do not copy this pattern.

---

## Auto-Imports — What NOT to Write

Nuxt auto-imports these. **Do not add explicit imports for them:**

| Auto-imported | Source module |
|---|---|
| `ref`, `computed`, `reactive`, `watch`, `watchEffect`, `onMounted`, `onUnmounted`, etc. | Vue 3 (via Nuxt) |
| All Quasar components (`QBtn`, `QRow`, `QCol`, `QForm`, etc.) | `nuxt-quasar-ui` |
| All PrimeVue components (`DataTable`, `TreeSelect`, etc.) | `@primevue/nuxt-module` |
| All VueUse composables (`useLocalStorage`, `useEventListener`, etc.) | `@vueuse/nuxt` |
| All components in `src/components/` | Nuxt component auto-import (pathPrefix: false) |
| `useI18n()`, `useRoute()`, `useRouter()`, `useFetch()`, etc. | Nuxt / `@nuxtjs/i18n` |
| All composables in `src/composables/` | Nuxt composables auto-import |

---

## Explicit Imports — What You MUST Write

Always write explicit imports for:

```ts
// Stores — always via @store alias, never rely on auto-import
import { useAccountStore } from '@store'
import { useSettingsStore } from '@store'

// RxJS operators/types
import { forkJoin, of, switchMap, map, tap, finalize } from 'rxjs'
import type { Observable } from 'rxjs'

// RxJS + VueUse bridge
import { useSubscription } from '@vueuse/rxjs'

// Utility libraries
import { cloneDeep, orderBy } from 'lodash-es'
import dayjs from 'dayjs'

// Logging — always consola, never console.*
import Log from 'consola'
```

---

## Refs — Always Use `get` / `set` from VueUse

In script blocks, **never access `.value` directly**. Use `get()` and `set()` from VueUse (auto-imported):

```ts
// ❌ Wrong
const count = ref(0)
count.value++
console.log(count.value)

// ✅ Correct
const count = ref(0)
set(count, get(count) + 1)
Log.debug(get(count))
```

Template access is fine without `get`/`set` — Vue unwraps refs in templates automatically.

---

## Logging

Always use `consola`. Never use `console.log`, `console.warn`, or `console.error`.

```ts
import Log from 'consola'

Log.debug('Loaded item', item)
Log.info('Download started')
Log.warn('Missing config key', key)
Log.error('Failed to fetch', error)
```

---

## Props and Emits

### Props with defaults

```ts
withDefaults(defineProps<{
  label: string
  disabled?: boolean
  cy?: string
}>(), {
  disabled: false,
  cy: undefined,
})
```

### Emits — call-signature style

```ts
const emit = defineEmits<{
  (e: 'confirm'): void
  (e: 'cancel'): void
  (e: 'update:modelValue', value: string): void
}>()
```

### TypeScript types

- **Inline** for single-use component props.
- **Shared** types go in `src/types/props/` (`@props` alias) or `src/types/interfaces/` (`@interfaces` alias).

```ts
import type { IMediaItem } from '@interfaces'
import type { IDownloadProps } from '@props'
```

---

## State & Logic Architecture

**Push logic and state into the Pinia store layer.** Components are thin: they read store state, call store actions, and render.

```ts
// ✅ Correct — import store explicitly, read state directly
import { useDownloadStore } from '@store'

const { items, isLoading } = useDownloadStore()

function onConfirm() {
  useDownloadStore().startDownload(get(selectedId))
}

// ❌ Wrong — wrapping store state in local computed
const isLoading = computed(() => useDownloadStore().isLoading)
```

Stores expose reactive refs via `toRefs(state)` — destructure them directly, no local wrapping needed.

---

## Pinia Store Import Pattern

Always import stores explicitly from `@store`. Do not rely on auto-import:

```ts
import { useAccountStore } from '@store'
import { useMediaStore } from '@store'

const { username, isAuthenticated } = useAccountStore()
const { mediaItems } = useMediaStore()
```

---

## RxJS Subscriptions — Always `useSubscription`

Use `useSubscription` from `@vueuse/rxjs` for all Observable subscriptions in components. It automatically disposes on component unmount — never manage subscription cleanup manually.

```ts
import { useSubscription } from '@vueuse/rxjs'
import { useDownloadStore } from '@store'

const { downloadProgress$ } = useDownloadStore()

useSubscription(
  downloadProgress$.pipe(
    tap(progress => set(localProgress, progress)),
  ).subscribe(),
)
```

Never use `.subscribe()` and store the subscription manually in a component.

---

## i18n

`useI18n()` is auto-imported. No import needed:

```ts
const { t } = useI18n()
```

---

## Styling

- Prefer Quasar utility classes (`q-pa-md`, `text-primary`, `row`, `col`, etc.) for layout and spacing.
- Use `<style lang="scss">` for custom styles.
- Quasar CSS variables (`$primary`, `$secondary`, etc.) are globally injected.
- Project SCSS variables **must be explicitly imported** in each `<style>` block:

```vue
<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.my-component {
  color: $custom-color;
}
</style>
```

---

## UI Library Priority

1. **Quasar first** — buttons, forms, dialogs, layout, lists.
2. **PrimeVue** for specific gaps — `DataTable`, `TreeSelect`, charts, etc.

Do not mix both for the same UI pattern.

---

## Form Validation

Use Quasar's built-in approach — `QForm` with `:rules` on inputs:

```vue
<QForm @submit="onSubmit">
  <QInput
    v-model="email"
    :rules="[val => !!val || t('validation.required')]"
  />
</QForm>
```

---

## Cross-Component Communication

**New code:** use Pinia store actions only.

Event bus composables (e.g., `useMediaOverviewBarDownloadCommandBus`) are **legacy** — do not copy or introduce new event buses.

---

## Cypress Testing Attributes

- Expose an optional `cy` prop on components that need targeting.
- Bind it to `data-cy` on the root element:

```vue
<template>
  <div :data-cy="cy">
    <!-- content -->
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  cy?: string
}>(), {
  cy: undefined,
})
</script>
```

- If a component has no `cy` prop, add a static `data-cy` on the root element.

---

## Shared Composables

Place reusable composable logic in `src/composables/useXxx.ts`. Nuxt auto-imports all composables from this directory.

```ts
// src/composables/useDownloadManager.ts
export function useDownloadManager() {
  // ...
  return { start, cancel, progress }
}
```

Use in components without importing:

```ts
const { start, cancel, progress } = useDownloadManager()
```

---

## Component Splitting

Split a component when **any** condition is true:

- 3+ distinct UI sections.
- Repeated template blocks that could be a reusable component.
- Mixes orchestration/state with substantial presentational markup.
- More than one clear responsibility.

---

## File Naming

- PascalCase `.vue` files: `ConfirmationDialog.vue`, `MediaOverview.vue`.
- Organized by domain under `src/components/` subdirectories.

---

## Common Mistakes

| Mistake | Correct approach |
|---|---|
| Writing `import { ref, computed } from 'vue'` | Omit — auto-imported |
| Writing `import { useI18n } from 'vue-i18n'` | Omit — auto-imported by `@nuxtjs/i18n` |
| `import { useAccountStore } from '@/store/accountStore'` | `import { useAccountStore } from '@store'` |
| Accessing `ref.value` in script | Use `get(ref)` and `set(ref, value)` |
| `console.log(...)` | `import Log from 'consola'` + `Log.debug(...)` |
| Manual subscription: `const sub = obs$.subscribe(...)` | `useSubscription(obs$.subscribe(...))` |
| Local computed wrapping store state | Destructure store refs directly |
| New event bus composables | Pinia store actions only |
| `@use '@/assets/scss/variables.scss'` omitted in `<style>` | Must explicitly add `@use '@/assets/scss/variables.scss' as *;` |
| `<script setup>` before `<template>` | `<template>` block goes FIRST |
