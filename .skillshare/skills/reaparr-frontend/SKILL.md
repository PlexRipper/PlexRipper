---
name: reaparr-frontend
description: Use when doing any Reaparr frontend work under src/AppHost/ClientApp, including Vue, Nuxt, TypeScript, Pinia, RxJS, Quasar, PrimeVue, Cypress, Vitest, frontend config, frontend reviews, debugging, refactors, and frontend test work. This skill must always load before any narrower frontend skill.
---

# Reaparr Frontend

## Purpose

This skill is mandatory for every Reaparr frontend task. Always load `reaparr-frontend` before reading, searching, editing, reviewing, debugging, planning, or testing frontend code. Do not skip it because a narrower frontend skill appears to match; load this skill first, then load the narrower skill.

It gives the shared frontend map, mandatory tool rules, verification gates, package-script routing, and routing to narrower skills.

Frontend work includes code, tests, configuration, plans, reviews, debugging, and refactors under:
- `src/AppHost/ClientApp/`
- `src/AppHost/ClientApp/src/`
- `src/AppHost/ClientApp/tests/`
- `src/AppHost/ClientApp/cypress/`
- frontend project files such as `package.json`, `nuxt.config.ts`, `vitest.config.ts`, `cypress.config.ts`, `eslint.config.mjs`, `tsconfig.json`, `.editorconfig`, and frontend CI/config files scoped to ClientApp

Do not use this skill for backend-only work under `src/` outside `src/AppHost/ClientApp/`; use the Reaparr backend skills instead.

## Required Tooling

Native repository tooling is mandatory for frontend work. Use these tools directly:
- `read`, `edit`, and `write` for file operations;
- `glob` and repository search tools for discovery;
- `lsp` for symbol inspection, references, refactors, and diagnostics;
- short `bash` commands for Bun, Vitest, Cypress, and factual checks.

Retry a failed native tool once with a narrower request before changing approach. Do not treat an external editor integration as a prerequisite for frontend work.

## Secondary Skill Routing

Load this skill first, then load narrower skills when the task matches.

| Task | Load next |
| --- | --- |
| Vue SFCs, components, pages, composables, styling, UI behavior | `reaparr-frontend-components` |
| Pinia stores, store setup, state, actions, getters, generated API endpoints, SignalR subscriptions, RxJS observable flows | `reaparr-pinia-store` |
| Frontend Vitest unit tests under `src/AppHost/ClientApp/tests/` | `reaparr-frontend-unit-tests` |
| Vue Composition API mechanics | `vue-best-practices` or `vue` |
| Nuxt app structure, routes, plugins, config, SSR/client behavior | `nuxt` |
| Pinia patterns not covered by Reaparr-specific store guidance | `pinia` or `vue-pinia-best-practices` |
| Vue Router behavior | `vue-router-best-practices` |
| Vue/Vitest component tests | `vue-testing-best-practices` or `vitest` |
| Cypress end-to-end tests | `cypress` or `e2e-testing-patterns` |
| Responsive layout or adaptive UI work | `responsive-design` |
| Design polish, visual refresh, frontend UX | `frontend-design` |
| UI/UX/accessibility review | `web-design-guidelines` |

Prefer Reaparr-specific frontend skills over generic Vue/Nuxt/Pinia skills when both apply.

## Frontend Architecture Map

| Area | Role |
| --- | --- |
| `src/AppHost/ClientApp/app.vue` | Nuxt app shell |
| `src/AppHost/ClientApp/nuxt.config.ts` | Nuxt, Quasar, PrimeVue, i18n, auto-import, and build configuration |
| `src/AppHost/ClientApp/src/components/` | Reusable Vue components, organized by domain |
| `src/AppHost/ClientApp/src/pages/` | Nuxt pages/routes |
| `src/AppHost/ClientApp/src/layouts/` | Shared page layout structure |
| `src/AppHost/ClientApp/src/composables/` | Reusable composition functions, auto-imported by Nuxt |
| `src/AppHost/ClientApp/src/store/` | Pinia stores; preferred location for frontend state and orchestration logic |
| `src/AppHost/ClientApp/src/types/` | Shared frontend types, props, interfaces, and generated API-facing types |
| `src/AppHost/ClientApp/src/assets/` | SCSS, images, and frontend assets |
| `src/AppHost/ClientApp/src/plugins/` | Nuxt plugins and app integration points |
| `src/AppHost/ClientApp/tests/` | Vitest unit tests and test utilities |
| `src/AppHost/ClientApp/cypress/` | Cypress end-to-end tests and support code |
| `src/AppHost/ClientApp/.github/` | Frontend-scoped GitHub Actions/workflows when present |

## Core Frontend Patterns

### Vue and Nuxt

- This is a Nuxt 4 SPA-style frontend; follow the existing Nuxt configuration and project conventions.
- Use Vue 3 Composition API with `<script setup lang="ts">`.
- Reaparr SFC convention is `<template>` first, then `<script setup lang="ts">`, then optional `<style lang="scss">`.
- Prefer Nuxt auto-imports for Vue, Nuxt, VueUse, Quasar, PrimeVue, components, and composables as documented in `reaparr-frontend-components`.
- Keep components thin; move orchestration, shared state, API flows, and RxJS streams into Pinia stores when they are not purely presentational.

### State and Stores

- Pinia is the default place for frontend state and business-facing UI orchestration.
- Import Reaparr stores explicitly from `@store`; do not rely on store auto-imports.
- Never import Reaparr stores from `#imports`. Nuxt auto-imports are acceptable for framework composables/components, but store imports must stay explicit so they do not trigger Nuxt imports-plugin warnings.

```ts
// ✅ Correct
import { useMediaStore, useSettingsStore } from '@store'

// ❌ Wrong
import { useMediaStore, useSettingsStore } from '#imports'
```

- Destructure store refs/actions according to existing store conventions.
- Do not wrap store state in redundant local `computed` values.
- Keep async flows deterministic and testable.

### RxJS

- Use RxJS where the existing store/component pattern uses observables.
- Components must use `useSubscription` from `@vueuse/rxjs` for subscriptions.
- Do not manually hold and dispose subscriptions in components.
- Keep observable chains readable; prefer small named helpers when chains become hard to scan.

### Refs

- In frontend script blocks, use VueUse `get()` and `set()` for refs instead of direct `.value` access unless an existing local exception clearly requires otherwise.
- Template ref unwrapping is fine.

### Utility Functions

Before writing custom browser, DOM, event, timing, array, object, clone, sort, grouping, debounce, throttle, or state helper code, check whether VueUse or `lodash-es` already provides the needed utility. Prefer those existing functions over bespoke handlers because they are tested, typed, familiar in this codebase, and often handle cleanup/reactivity edge cases.

Use VueUse first for Vue/browser/reactivity concerns such as event listeners, click-outside behavior, keyboard shortcuts, debouncing/throttling, timers, breakpoints, element size/visibility, storage, async state, and ref helpers. Use `lodash-es` for data transformations such as `cloneDeep`, `orderBy`, `groupBy`, `uniqBy`, `keyBy`, `debounce`, and `throttle` when a VueUse composable is not the better fit.

Do not introduce hand-rolled handlers for common patterns unless the existing libraries do not cover the need or the custom code is clearly simpler than the abstraction. When custom helper code is necessary, keep it small, named by intent, and colocated in the component/store/composable that owns the behavior unless it is genuinely reusable.

### Logging

- Use `consola` for frontend logging.
- Do not introduce `console.log`, `console.warn`, or `console.error`.

### Styling

- Prefer Quasar utilities for layout and spacing.
- Use Quasar first for common UI controls; use PrimeVue for specific gaps already represented in the project.
- Use `<style lang="scss">` for custom SFC styles.
- Import project SCSS variables explicitly in each style block when using project variables:

```scss
@use '@/assets/scss/variables.scss' as *;
```

### Testability

- Add stable `data-cy` attributes for UI paths that Cypress needs to target.
- Components that need configurable targeting should expose an optional `cy` prop and bind it to `data-cy`.
- Keep tests deterministic; avoid timer, network, filesystem, or store-state leakage between tests.

## Frontend Workflow

1. Load `reaparr-frontend`; this is not optional for frontend work.
2. Confirm the task is frontend-scoped and load any narrower matching skills.
3. Use native search and symbol tools to find existing precedent.
4. Identify whether the change belongs in a component, composable, store, page, plugin, config file, unit test, or Cypress test.
5. Make the smallest maintainable change that fixes the root cause.
6. Preserve frontend architecture boundaries; prefer stores for orchestration and components for rendering.
7. Keep behavior deterministic, especially in tests and RxJS flows.
8. Re-read changed files after edits to confirm the intended changes landed.
9. Use native diagnostics and language-server analysis to find errors that need fixing before claiming completion. Do not run a package build as the first error-discovery mechanism.
10. For user-visible frontend implementation changes, verify the behavior in a browser with Chrome DevTools MCP or Playwright MCP before claiming completion.

## Browser Verification

For frontend implementation work that changes user-visible behavior, layout, routing, forms, dialogs, state-driven UI, network behavior, or browser-only runtime behavior, verify in a real browser with MCP after code-level checks.

Prefer `chrome-devtools` for interactive inspection, console errors, network requests, DOM state, screenshots, and performance/debugging. Use `playwright` when the verification is best expressed as deterministic navigation, clicking, form input, assertions, screenshots, or an end-to-end flow.

Browser verification should be targeted and proportional:
- Use Chrome DevTools MCP for exploratory checks, visual inspection, console/network diagnostics, and debugging runtime issues.
- Use Playwright MCP for repeatable flows and clear pass/fail UI behavior.
- Capture the smallest useful evidence: page reached, visible element state, console/network errors, screenshot, or flow result.
- If the app cannot be launched or authenticated in the current environment, state what browser verification was attempted and why it could not be completed.
- Do not replace unit, typecheck, lint, or Cypress verification with browser inspection; use browser MCP as the final user-visible behavior check when applicable.

Pure skill/documentation-only changes do not require browser verification.

## Verification Gates

Required error-checking flow:
1. Run native `lsp` diagnostics for each changed frontend file when a language server is available.
2. If diagnostics are incomplete or fail, retry with narrower file ranges and repository-native checks.
3. Fix all relevant reported errors.
4. For user-visible frontend implementation changes, perform targeted browser verification with Chrome DevTools MCP or Playwright MCP and report the evidence.

Test routing:
- Component/page/composable changes: native diagnostics first, then targeted Vitest tests when available; otherwise lint/typecheck or explain the missing targeted verifier.
- Store changes: load `reaparr-pinia-store`; diagnostics first, then relevant store unit tests from `reaparr-frontend-unit-tests`.
- Unit test changes: load `reaparr-frontend-unit-tests`; run the relevant Vitest target.
- Cypress test changes: run targeted Cypress spec when possible; otherwise explain why it could not be run.
- Config/package changes: inspect `package.json` scripts, then run the smallest relevant config validation, lint, typecheck, or test command.
- Pure skill/documentation-only changes: re-read the changed skill file; no frontend runtime or browser test is required unless the skill content changes executable project behavior.

Do not claim success unless native diagnostics and required tests passed. If a language server is unavailable, state that explicitly and use the strongest available native checks.

## Common Mistakes

- Skipping this umbrella skill and loading only a narrow frontend skill.
- Using external editor integrations for frontend files.
- Switching to broad filesystem commands after one native-tool hiccup instead of retrying with a narrower request.
- Running package builds as the first way to discover errors instead of using native diagnostics.
- Importing Vue, Nuxt, Quasar, PrimeVue, VueUse, components, or composables that Nuxt already auto-imports.
- Importing Reaparr stores from `#imports` instead of `@store`.
- Accessing refs with `.value` in script blocks instead of `get()`/`set()`.
- Adding `console.*` logging instead of `consola`.
- Placing orchestration-heavy logic in components instead of Pinia stores.
- Creating new event bus composables instead of using Pinia actions.
- Writing custom DOM, event, debounce, throttle, clone, sort, group, or array/object transformation handlers when VueUse or `lodash-es` already provides a suitable function.
- Manually managing RxJS subscriptions in components.
- Mixing Quasar and PrimeVue for the same UI pattern.
- Omitting stable `data-cy` hooks for Cypress-relevant UI.
- Weakening tests or assertions to force green.
- Running backend package managers or dotnet commands for frontend-only work.
- Claiming a user-visible frontend implementation is complete without targeted Chrome DevTools MCP or Playwright MCP verification, unless browser verification was not applicable or could not be run and that limitation is reported.
