# Media Overview Paged Virtual Scroll + Sort Navigation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task. For Reaparr frontend work, always load `reaparr-frontend-components`; for Pinia store changes, also load `reaparr-pinia-store`. For Reaparr backend work, always load `reaparr-backend` before narrower backend skills such as `fast-endpoints`, `entity-framework-core`, and `reaparr-backend-unit-tests`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Media Overview’s load-all behavior with server-backed pagination, sparse client caching, seamless virtual scrolling, and accurate sort-aware navigation labels across very large libraries.

**Architecture:** TanStack Virtual remains responsible only for viewport virtualization, placeholder rendering, range observation, and `scrollToIndex`. The Reaparr backend becomes authoritative for active-query filtering, search, sorting, total matching count, page windows, and navigation label-to-global-index anchors. The frontend store keeps a sparse page cache keyed by global item index/page and loads missing ranges on demand, including a centered range around navigation jumps.

**Tech Stack:** ASP.NET Core / FastEndpoints, EF Core, C#, Vue 3, Nuxt SPA, Pinia, RxJS, `@tanstack/vue-virtual`, Quasar.

---

## Critical Findings From Investigation

Files already inspected during planning/investigation:

- `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaOverview.vue`
- `src/AppHost/ClientApp/src/components/Navigation/AlphabetNavigation.vue`
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaTable.vue`
- `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/PosterTable.vue`
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaOverviewSearchBar.vue`
- `src/AppHost/ClientApp/src/types/enums/mediaSortField.ts`
- `src/AppHost/ClientApp/src/types/api/generated/PlexLibrary.ts`
- `src/Application/PlexLibraries/GetMetadata/GetLibraryMediaMetadata.cs`
- `src/Application/PlexLibraries/GetMedia/GetPlexLibraryMediaEndpoint.cs`
- `src/Application/PlexMedia/GetAll/GetAllMediaByTypeEndpoint.cs`
- `src/Application/_Shared/BaseRequests/PlexMediaFilterQueryRequest.cs`
- `src/Data.Contracts/DTO/MediaQueryFilter.cs`
- `src/Data.Contracts/Extensions/DbContext/DbContextExtensions.PlexMedia.cs`
- `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaMetadataDTO.cs`
- `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaStatisticsDTO.cs`
- `src/Application.Contracts/_Shared/Mappings/PlexMedia/PlexMediaSlimDTOMapper.cs`

### Current frontend behavior

Files already inspected in the planning session:

- `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaOverview.vue`
- `src/AppHost/ClientApp/src/components/Navigation/AlphabetNavigation.vue`
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaTable.vue`
- `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/PosterTable.vue`

Current flow:

1. `mediaOverviewStore.requestMedia()` uses `page = 0` and `size = 0`.
2. It calls either `refreshAllLibraryMediaByType(page, size)` or `refreshLibraryMedia(page, size)`.
3. `setMedia(data)` stores all returned media in `state.items = Object.freeze(data.mediaList)`.
4. `AlphabetNavigation.vue` renders `mediaOverviewStore.scrollDict`.
5. `scrollDict` is generated from currently loaded items in `setMediaIndexNavigationOptions()`.
6. `sortMedia()` sorts the full loaded list client-side into `state.sortedItems`.
7. `getMediaItems` filters locally using `state.filterQuery`.
8. `MediaTable.vue` and `PosterTable.vue` receive an index through the event bus and call TanStack Virtual `scrollToIndex`.

This works only because all items are loaded. With true pagination, search, sort, and navigation labels must be server-backed. `scrollDict` can no longer be derived from loaded client items.

### Existing backend metadata endpoint

`src/Application/PlexLibraries/GetMetadata/GetLibraryMediaMetadata.cs` currently returns `PlexMediaMetadataDTO` containing media/filter metadata including `MediaCount`, role/country/genre/quality counts, and filter lists. This endpoint is still the right place to return navigation metadata because it represents the active media overview context.

However, the navigation contract must be generic, not alphabet-only. The frontend sort mode is always representable as a displayed `label => first global index` mapping. Replace the original alphabet-only idea with a generic property such as:

```csharp
public Dictionary<string, int> NavigationIndexes { get; init; } = [];
```

Examples:

- Title sort: `# => 0`, `A => 3`, `B => 20`
- Year sort: `2024 => 0`, `2023 => 18`
- Quality sort: `1080p => 0`, `720p => 150`
- Duration sort: `0–10 min => 0`, `10–20 min => 30`
- Media size sort: `0–1 GB => 0`, `1–2 GB => 84`
- Date sort: `Jan 2024 => 0`, `Dec 2023 => 44`

### Current backend paging contract gap

The existing paged endpoints are:

- `src/Application/PlexLibraries/GetMedia/GetPlexLibraryMediaEndpoint.cs`
- `src/Application/PlexMedia/GetAll/GetAllMediaByTypeEndpoint.cs`

Both call `_dbContext.GetMediaByType(...)`, which returns only the requested page/list as `List<PlexMediaSlimDTO>`. The response is currently built through `ToStatisticsDTO()`, and `ToStatisticsDTO()` calculates `MediaCount` by iterating only the returned list. Therefore, after paging is enabled, `PlexMediaStatisticsDTO.MediaCount` means **returned page count**, not **total matching active-query count**.

Virtualizer `count` must never depend on this ambiguous page count. Add an explicit `TotalCount` property to the paged media response and make the frontend use `totalCount` for virtualization.

### Current all-media sorting bug for deep paging

`GetMediaByType(...)` currently applies DB `Skip/Take`, then for all-media mode (`plexLibraryId == 0`) sorts only the returned page with:

```csharp
plexMediaSlimDtos = plexMediaSlimDtos.OrderByNatural(x => x.SearchTitle).ToList();
```

That is not globally sorted pagination. Correct order must be:

```text
base query -> filters -> search -> sort -> total count -> skip/take -> DTO projection
```

---

## Target UX

### Infinite scroll

- Initial page loads quickly.
- Virtualizer `count` equals total matching media count, not loaded item count.
- Visible unloaded rows render as placeholders/skeletons.
- As the user scrolls, the store fetches missing pages around the visible range.
- Loaded pages are inserted into the local sparse cache.

### Sort-aware navigation jump

When the user clicks a navigation label:

1. `AlphabetNavigation` sends a jump command for the backend-provided global index.
2. Store requests a range around the index, for example 50 items before and after.
3. Store inserts returned pages/items into the sparse cache.
4. Component calls TanStack Virtual `scrollToIndex(globalIndex)`.
5. Row/poster appears with minimal delay and is highlighted.

The component may keep its existing name `AlphabetNavigation.vue` during migration, but the data is now generic sort navigation, not alphabet-only data. A later cleanup may rename the component.

### TanStack Virtual conclusion

`@tanstack/vue-virtual` supports the needed viewport mechanics: large `count`, `scrollToIndex`, range observation through `onChange`, placeholders for unloaded indexes, and fixed or dynamic item sizes. It does not provide remote pagination, query anchors, or server-side indexes. Those belong in Reaparr’s API and Pinia store.

---

# Phase 1: Backend query contract foundation

## Task 1: Add explicit active-query total count to paged media response

**Files:**

- Modify: `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaStatisticsDTO.cs`
- Modify: `src/Application.Contracts/_Shared/Mappings/PlexMedia/PlexMediaSlimDTOMapper.cs`
- Test: existing Application unit tests covering media endpoints, or add focused tests in `tests/UnitTests/Application.UnitTests/`

- [ ] **Step 1: Add `TotalCount` to `PlexMediaStatisticsDTO`**

Add an explicit active-query total count separate from the page statistics:

```csharp
public class PlexMediaStatisticsDTO
{
    /// <summary>
    /// Gets or sets the total number of media items matching the active query before paging.
    /// This value is used by the frontend virtualizer.
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the count of media items in this returned page/list.
    /// </summary>
    public required int MediaCount { get; set; }

    public required int MovieCount { get; set; }

    public required int TvShowCount { get; set; }

    public required int SeasonCount { get; set; }

    public required int EpisodeCount { get; set; }

    public required long MediaSize { get; set; }

    public required List<PlexMediaSlimDTO> MediaList { get; init; }
}
```

- [ ] **Step 2: Update `ToStatisticsDTO` to accept an optional total count**

Change the mapper signature to avoid ambiguous totals:

```csharp
public static PlexMediaStatisticsDTO ToStatisticsDTO(this List<PlexMediaSlimDTO> source, int? totalCount = null)
{
    var stats = new PlexMediaStatisticsDTO
    {
        TotalCount = totalCount ?? source.Count,
        MovieCount = 0,
        TvShowCount = 0,
        SeasonCount = 0,
        EpisodeCount = 0,
        MediaSize = 0,
        MediaCount = 0,
        MediaList = source,
    };

    // Existing page statistics calculation remains the same.
}
```

- [ ] **Step 3: Update all object initializers and call sites**

Search for `new PlexMediaStatisticsDTO` and `ToStatisticsDTO(`. Ensure every initializer sets `TotalCount`, and every paged endpoint passes the active query total once available. During this task, legacy call sites may use `source.Count` via the mapper default.

- [ ] **Step 4: Run Rider diagnostics for changed backend files**

Expected: no errors.

---

## Task 2: Extend query request/filter contracts with search and server-side sort

**Files:**

- Modify: `src/Application/_Shared/BaseRequests/PlexMediaFilterQueryRequest.cs`
- Modify: `src/Data.Contracts/DTO/MediaQueryFilter.cs`
- Modify: endpoint request call sites using `MediaQueryFilter`
- Test: relevant Application endpoint validator tests

- [ ] **Step 1: Extend `PlexMediaFilterQueryRequest`**

Add query parameters that reflect the frontend query context. Use existing frontend sort values from `src/AppHost/ClientApp/src/types/enums/mediaSortField.ts`:

```csharp
[QueryParam, BindFrom("search")]
[DefaultValue("")]
public string Search { get; init; } = string.Empty;

[QueryParam, BindFrom("sortField")]
[DefaultValue("sortIndex")]
public string SortField { get; init; } = "sortIndex";

[QueryParam, BindFrom("sortDirection")]
[DefaultValue("asc")]
public string SortDirection { get; init; } = "asc";
```

Update the protected constructor to accept and assign these values so generated TypeScript treats them as query params.

- [ ] **Step 2: Extend `MediaQueryFilter`**

Add:

```csharp
public required string Search { get; init; }

public required string SortField { get; init; }

public required string SortDirection { get; init; }
```

- [ ] **Step 3: Pass query fields from endpoints into `MediaQueryFilter`**

Both `GetPlexLibraryMediaEndpoint` and `GetAllMediaByTypeEndpoint` must set:

```csharp
Search = req.Search,
SortField = req.SortField,
SortDirection = req.SortDirection,
```

- [ ] **Step 4: Validate supported sort direction and sort fields**

Supported `SortDirection` values are `asc` and `desc`. Supported `SortField` values initially match frontend `MediaSortField` values:

```text
sortIndex
year
addedAt
updatedAt
duration
mediaSize
quality
```

- [ ] **Step 5: Run Rider diagnostics**

Expected: no errors.

---

## Task 3: Refactor `GetMediaByType` to return page plus active total count

**Files:**

- Modify: `src/Data.Contracts/Extensions/DbContext/DbContextExtensions.PlexMedia.cs`
- Optional create: `src/Data.Contracts/DTO/PagedMediaQueryResult.cs` if no suitable result type exists
- Test: endpoint tests for page count vs total count

- [ ] **Step 1: Introduce a result type for paged media query output**

Create a small DTO/record in `src/Data.Contracts/DTO/` if one does not already exist:

```csharp
namespace Reaparr.Data.Contracts;

public record PagedMediaQueryResult
{
    public required List<PlexMediaSlimDTO> Items { get; init; }

    public required int TotalCount { get; init; }
}
```

Prefer the existing `DTO` folder to avoid new folder registration.

- [ ] **Step 2: Change `GetMediaByType` return type**

Change:

```csharp
public static async Task<Result<List<PlexMediaSlimDTO>>> GetMediaByType(...)
```

to:

```csharp
public static async Task<Result<PagedMediaQueryResult>> GetMediaByType(...)
```

- [ ] **Step 3: Apply filter/search/sort before count and paging**

The implementation order must be:

```text
base query -> allowed libraries -> metadata filters -> search -> sort -> CountAsync -> Skip/Take -> DTO projection
```

Do not sort after `Skip/Take`. This is especially important for all-media mode.

- [ ] **Step 4: Keep first implementation simple but bounded**

Prefer EF-translatable ordering and filtering. If natural sorting cannot be translated, use deterministic database-side ordering by `SearchTitle`/`SortIndex` for now and document natural-sort parity as a follow-up. Do not load 100k full DTOs solely to sort in memory.

- [ ] **Step 5: Return page items and total count**

Return:

```csharp
return Result.Ok(new PagedMediaQueryResult
{
    Items = plexMediaSlimDtos,
    TotalCount = totalCount,
});
```

- [ ] **Step 6: Update endpoints to pass total count into `ToStatisticsDTO`**

Use:

```csharp
await SendFluentResult(
    Result.Ok(mediaListResult.Value.Items.ToStatisticsDTO(mediaListResult.Value.TotalCount)),
    ct
);
```

- [ ] **Step 7: Run Rider diagnostics**

Expected: no errors.

---

# Phase 2: Backend generic navigation indexes

## Task 4: Replace alphabet-only metadata with generic navigation indexes

**Files:**

- Modify: `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaMetadataDTO.cs`
- Modify: `src/Application/PlexLibraries/GetMetadata/GetLibraryMediaMetadata.cs`
- Test: metadata endpoint tests

- [ ] **Step 1: Add `NavigationIndexes` to `PlexMediaMetadataDTO`**

Add:

```csharp
/// <summary>
/// Gets sort-aware navigation labels mapped to their first global index in the active filtered/searched/sorted result set.
/// </summary>
public Dictionary<string, int> NavigationIndexes { get; init; } = [];
```

Do not add `AlphabetIndexes`; this feature is not alphabet-only.

- [ ] **Step 2: Extend `GetLibraryMediaMetadataRequest` with active query context**

The metadata endpoint must accept enough query params to match the media page endpoint: `search`, `countryId`, `genreId`, `roleId`, `quality`, `filterOfflineMedia`, `filterOwnedMedia`, `sortField`, and `sortDirection`. Use the same bind names as the page endpoints.

- [ ] **Step 3: Build a helper for `label => first global index`**

Add a helper conceptually shaped like:

```csharp
private async Task<Dictionary<string, int>> GetNavigationIndexes(
    GetLibraryMediaMetadataRequest req,
    CancellationToken ct
)
{
    // Build the same active query as the paged media endpoint.
    // Apply filters/search/sort before indexing.
    // Project only the fields required to calculate the label.
    // Return the first global index for each displayed label.
}
```

Do not load full media DTOs. If an initial implementation must calculate labels in memory, only project minimal scalar fields needed for labels and document the performance trade-off in a code comment.

- [ ] **Step 4: Implement label functions that match the current frontend sort labels**

Current frontend `setMediaIndexNavigationOptions()` behavior should move server-side. Use these label semantics:

```text
sortIndex/title: first character bucket, A-Z or #
year: year or #
quality: translated/highest quality label
duration: 10-minute buckets, e.g. 0–10 min
addedAt: MMM yyyy
updatedAt: MMM yyyy
mediaSize: decimal GB buckets, e.g. 0–1 GB
```

- [ ] **Step 5: Set `NavigationIndexes` in both metadata branches**

Both the specific-library branch and all-media branch must set:

```csharp
NavigationIndexes = await GetNavigationIndexes(req, ct),
```

- [ ] **Step 6: Run Rider diagnostics**

Expected: no errors.

---

# Phase 3: Generated frontend API update

## Task 5: Regenerate TypeScript API from backend contracts

**Files:**

- Generated modify: `src/AppHost/ClientApp/src/types/api/generated/*`
- Generated modify: API path helpers if changed

- [ ] **Step 1: Start backend dev run configuration through Rider MCP**

AGENTS.md requires the backend to be running before `generate-ts`. Use the Rider run configuration from `.run/Reaparr Back-End Development.run.xml` if available.

- [ ] **Step 2: Run frontend generation through existing Bun tooling**

Use the project script from `src/AppHost/ClientApp/package.json`. Expected command shape:

```bash
bun --cwd src/AppHost/ClientApp run generate-ts
```

Use MCP run tooling where available. Do not use npm/yarn/pnpm.

- [ ] **Step 3: Confirm generated TypeScript contains new fields**

Verify generated contracts include:

```ts
interface PlexMediaStatisticsDTO {
  totalCount: number;
}

interface PlexMediaMetadataDTO {
  navigationIndexes?: Record<string, number>;
}
```

Exact optional/required syntax may differ depending on generator output.

- [ ] **Step 4: If generation is blocked, stop and report**

Do not silently hand-edit generated API files unless the user approves a temporary generated-file edit.

---

# Phase 4: Frontend store sparse cache

## Task 6: Add sparse paging state to `mediaOverviewStore.ts`

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`

- [ ] **Step 1: Add state fields**

Add to `IMediaOverviewStoreState` and `defaultState`:

```ts
pageSize: number;
totalCount: number;
pageCache: Map<number, Readonly<PlexMediaSlimDTO[]>>;
loadedPages: Set<number>;
loadingPages: Set<number>;
navigationIndexes: Map<string, number>;
pendingScrollIndex: number | null;
```

Recommended defaults:

```ts
pageSize: 100,
totalCount: 0,
pageCache: new Map<number, Readonly<PlexMediaSlimDTO[]>>(),
loadedPages: new Set<number>(),
loadingPages: new Set<number>(),
navigationIndexes: new Map<string, number>(),
pendingScrollIndex: null,
```

- [ ] **Step 2: Keep old `items` temporarily**

Do not remove `items` immediately. Keep it as a compatibility bridge until table/poster/components are migrated.

- [ ] **Step 3: Add cache reset action**

Add:

```ts
resetPagedCache() {
  state.pageCache = new Map<number, Readonly<PlexMediaSlimDTO[]>>();
  state.loadedPages = new Set<number>();
  state.loadingPages = new Set<number>();
  state.totalCount = 0;
  state.navigationIndexes = new Map<string, number>();
  state.scrollDict = new Map<string, number>();
  state.pendingScrollIndex = null;
}
```

Use direct state assignment in the store; this file already uses reactive state assignment.

- [ ] **Step 4: Run frontend type/lint diagnostics through WebStorm MCP if available**

Expected: no TypeScript errors in changed file.

---

## Task 7: Add page insertion and index lookup helpers

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`

- [ ] **Step 1: Add helper to compute page for global index**

```ts
getPageForIndex(index: number): number {
  return Math.max(0, Math.floor(index / state.pageSize));
}
```

- [ ] **Step 2: Add helper to insert a page**

```ts
setPage(page: number, data: PlexMediaStatisticsDTO | null) {
  if (!data) {
    return;
  }

  state.pageCache.set(page, Object.freeze(data.mediaList));
  state.loadedPages.add(page);
  state.totalCount = data.totalCount;
  state.itemsLength = data.totalCount;

  state.allMovieCount = data.movieCount;
  state.allTvShowCount = data.tvShowCount;
  state.allSeasonCount = data.seasonCount;
  state.allEpisodeCount = data.episodeCount;
  state.allFileSize = data.mediaSize;
}
```

- [ ] **Step 3: Add helper to read item by global index**

```ts
getItemByIndex(index: number): PlexMediaSlimDTO | null {
  if (index < 0 || index >= state.totalCount) {
    return null;
  }

  const page = actions.getPageForIndex(index);
  const pageItems = state.pageCache.get(page);
  if (!pageItems) {
    return null;
  }

  return pageItems[index - page * state.pageSize] ?? null;
}
```

- [ ] **Step 4: Add loaded-range helper**

```ts
isIndexLoaded(index: number): boolean {
  return actions.getItemByIndex(index) !== null;
}
```

- [ ] **Step 5: Run WebStorm diagnostics**

Expected: no TypeScript errors.

---

## Task 8: Replace first load with first page load

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`

- [ ] **Step 1: Add `requestMediaFirstPage`**

```ts
requestMediaFirstPage(): Observable<PlexMediaStatisticsDTO | null> {
  if (state.loading) {
    Log.debug('Initial media request already in progress, skipping');
    return of(null);
  }

  actions.resetPagedCache();

  const page = 0;
  const size = state.pageSize;

  state.loading = true;
  Log.debug('Starting initial paged media request', {
    libraryId: state.libraryId,
    mediaType: get(getters.getMediaType),
    page,
    size,
  });

  return forkJoin([
    actions.refreshMetaData(),
    defer(() =>
      state.libraryId > 0
        ? libraryStore.refreshLibrary(state.libraryId)
        : of(null),
    ).pipe(takeUntil(cancelSubject$)),
    defer(() =>
      state.libraryId === 0
        ? actions.refreshAllLibraryMediaByType(page, size)
        : actions.refreshLibraryMedia(page, size),
    ).pipe(
      tap((data) => {
        actions.setPage(page, data);
      }),
    ),
  ]).pipe(
    takeUntil(cancelSubject$),
    map(([_, __, media]) => media),
    tap({
      next: () => {
        state.loading = false;
        Log.debug('Initial paged media request completed successfully');
      },
      error: (err) => {
        state.loading = false;
        Log.error('Initial paged media request failed', err);
      },
      complete: () => {
        if (state.loading) {
          state.loading = false;
          Log.debug('Initial paged media request was cancelled');
        }
      },
    }),
  );
}
```

- [ ] **Step 2: Make existing `requestMedia()` delegate to first page**

Temporarily replace `requestMedia()` body with:

```ts
return actions.requestMediaFirstPage();
```

This preserves current callers.

- [ ] **Step 3: Ensure metadata anchors are copied into store**

When `refreshMetaData()` receives `result.value`, also set:

```ts
state.navigationIndexes = new Map(Object.entries(result.value.navigationIndexes ?? {}));
state.scrollDict = new Map(Object.entries(result.value.navigationIndexes ?? {}));
```

Match exact generated casing. It should be `navigationIndexes` in TypeScript when generated from `NavigationIndexes`.

- [ ] **Step 4: Run WebStorm diagnostics**

Expected: no TypeScript errors.

---

## Task 9: Add page and range loading actions

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`

- [ ] **Step 1: Add `requestMediaPage`**

```ts
requestMediaPage(page: number): Observable<PlexMediaStatisticsDTO | null> {
  if (page < 0) {
    return of(null);
  }

  if (state.loadedPages.has(page) || state.loadingPages.has(page)) {
    return of(null);
  }

  state.loadingPages.add(page);

  return defer(() =>
    state.libraryId === 0
      ? actions.refreshAllLibraryMediaByType(page, state.pageSize)
      : actions.refreshLibraryMedia(page, state.pageSize),
  ).pipe(
    takeUntil(cancelSubject$),
    tap((data) => actions.setPage(page, data)),
    tap({
      next: () => state.loadingPages.delete(page),
      error: (err) => {
        state.loadingPages.delete(page);
        Log.error('Paged media request failed', { page, err });
      },
      complete: () => state.loadingPages.delete(page),
    }),
  );
}
```

- [ ] **Step 2: Add `ensureRangeLoaded`**

```ts
ensureRangeLoaded(startIndex: number, endIndex: number): Observable<(PlexMediaStatisticsDTO | null)[]> {
  if (state.totalCount <= 0) {
    return of([]);
  }

  const safeStart = Math.max(0, startIndex);
  const safeEnd = Math.min(state.totalCount - 1, endIndex);

  if (safeEnd < safeStart) {
    return of([]);
  }

  const startPage = actions.getPageForIndex(safeStart);
  const endPage = actions.getPageForIndex(safeEnd);
  const requests: Observable<PlexMediaStatisticsDTO | null>[] = [];

  for (let page = startPage; page <= endPage; page++) {
    if (!state.loadedPages.has(page) && !state.loadingPages.has(page)) {
      requests.push(actions.requestMediaPage(page));
    }
  }

  return requests.length ? forkJoin(requests) : of([]);
}
```

- [ ] **Step 3: Add `prefetchAroundIndex`**

```ts
prefetchAroundIndex(index: number, radius: number = 50): Observable<(PlexMediaStatisticsDTO | null)[]> {
  return actions.ensureRangeLoaded(index - radius, index + radius);
}
```

- [ ] **Step 4: Run WebStorm diagnostics**

Expected: no TypeScript errors.

---

# Phase 5: Frontend sort navigation wiring

## Task 10: Keep `AlphabetNavigation.vue` simple but source global navigation indexes

**Files:**

- Modify: `src/AppHost/ClientApp/src/components/Navigation/AlphabetNavigation.vue`

- [ ] **Step 1: Keep current template shape**

The current template can remain conceptually the same:

```vue
<q-btn
  v-for="[displayValue, scrollIndex] in mediaOverviewStore.scrollDict"
  :key="displayValue"
  class="navigation-btn"
  :label="displayValue"
  flat
  square
  no-wrap
  :data-cy="`letter-${displayValue}-alphabet-navigation-btn`"
  @click="sendMediaOverviewScrollToCommand(scrollIndex)" />
```

The important change is not here: `scrollDict` must now be populated from backend `navigationIndexes`, not local loaded items.

- [ ] **Step 2: Hide when no anchors exist**

Wrap root with:

```vue
<div
  v-if="mediaOverviewStore.scrollDict.size > 0"
  class="alphabet-navigation-container">
```

This hides sort navigation when the backend returns no navigation labels.

- [ ] **Step 3: Run WebStorm diagnostics**

Expected: no Vue/TypeScript errors.

---

## Task 11: Load target range before scroll highlight

**Files:**

- Modify: `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaTable.vue`
- Modify: `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/PosterTable.vue`

- [ ] **Step 1: Update event listener in `MediaTable.vue`**

Replace direct listener body:

```ts
listenMediaOverviewScrollToCommand((scrollIndex) => {
  scrollToIndex(scrollIndex);
});
```

with:

```ts
listenMediaOverviewScrollToCommand((scrollIndex) => {
  useSubscription(
    mediaOverviewStore.prefetchAroundIndex(scrollIndex, 50).pipe(
      tap(() => scrollToIndex(scrollIndex)),
    ).subscribe(),
  );
});
```

Add required imports:

```ts
import { tap } from 'rxjs';
import { useSubscription } from '@vueuse/rxjs';
```

Respect existing Reaparr frontend standards: use `useSubscription`, not unmanaged subscriptions.

- [ ] **Step 2: Update event listener in `PosterTable.vue` similarly**

Replace direct scroll call with prefetch + scroll.

- [ ] **Step 3: Handle bounds after global count migration**

In `PosterTable.vue`, replace checks against `props.items.length` with `mediaOverviewStore.totalCount`.

- [ ] **Step 4: Run WebStorm diagnostics**

Expected: no Vue/TypeScript errors.

---

# Phase 6: Virtualizer count and placeholders

## Task 12: Convert `MediaTable.vue` from loaded rows to global count

**Files:**

- Modify: `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaTable.vue`

- [ ] **Step 1: Change virtualizer count**

Replace:

```ts
count: props.rows.length,
```

with:

```ts
count: mediaOverviewStore.totalCount,
```

- [ ] **Step 2: Read row by global index**

In template, replace direct `rows[virtualRow.index]!` usage with a local helper:

```ts
function getRow(index: number): PlexMediaSlimDTO | null {
  return mediaOverviewStore.getItemByIndex(index);
}
```

- [ ] **Step 3: Render placeholder row when unloaded**

In the virtual row body:

```vue
<MediaTableRow
  v-if="getRow(virtualRow.index)"
  :index="virtualRow.index"
  :data-cy="`media-table-row-${virtualRow.index}`"
  :columns="mediaTableColumns"
  :row="getRow(virtualRow.index)!"
  selectable
  :selected="isSelected(getRow(virtualRow.index)!.id)"
  :disable-highlight="disableHighlight"
  :disable-hover-click="disableHoverClick"
  @selected="updateSelectedRow(getRow(virtualRow.index)!.id, $event)" />
<div
  v-else
  class="media-table-row-placeholder"
  :data-cy="`media-table-row-placeholder-${virtualRow.index}`">
  <q-skeleton type="text" />
</div>
```

- [ ] **Step 4: Add placeholder style**

```scss
.media-table-row-placeholder {
  height: $media-table-row-height;
  display: flex;
  align-items: center;
  padding: 0 16px;
}
```

- [ ] **Step 5: Trigger prefetch from virtualizer range**

In `onChange`, inspect current virtual items and call `ensureRangeLoaded` around visible indexes. Debounce if needed.

Conceptual code:

```ts
const virtualItems = _instance.getVirtualItems();
const first = virtualItems[0]?.index;
const last = virtualItems[virtualItems.length - 1]?.index;
if (first !== undefined && last !== undefined) {
  useSubscription(mediaOverviewStore.ensureRangeLoaded(first - 20, last + 20).subscribe());
}
```

If `onChange` cannot safely call `useSubscription` repeatedly, move this to a watcher on `rowVirtualizer.getVirtualItems()` or a small debounced function.

- [ ] **Step 6: Run WebStorm diagnostics**

Expected: no Vue/TypeScript errors.

---

## Task 13: Convert `PosterTable.vue` from loaded items to global count

**Files:**

- Modify: `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/PosterTable.vue`

- [ ] **Step 1: Change row count**

Replace:

```ts
const rowCount = computed(() => Math.ceil(props.items.length / get(gridItems)));
```

with:

```ts
const rowCount = computed(() => Math.ceil(mediaOverviewStore.totalCount / get(gridItems)));
```

- [ ] **Step 2: Change row item lookup to sparse lookup**

Replace `getRowItems` with logic that returns loaded item or placeholder metadata.

Suggested local type:

```ts
type PosterGridItem = {
  index: number;
  item: PlexMediaSlimDTO | null;
};
```

Suggested helper:

```ts
function getRowItems(rowIndex: number): PosterGridItem[] {
  const cols = get(gridItems);
  const result: PosterGridItem[] = [];

  for (let col = 0; col < cols; col++) {
    const index = rowIndex * cols + col;
    if (index >= mediaOverviewStore.totalCount) {
      break;
    }

    result.push({
      index,
      item: mediaOverviewStore.getItemByIndex(index),
    });
  }

  return result;
}
```

- [ ] **Step 3: Render poster placeholder**

Template concept:

```vue
<template
  v-for="entry in getRowItems(virtualRow.index)"
  :key="entry.item?.id ?? `placeholder-${entry.index}`">
  <MediaPoster
    v-if="entry.item"
    :media-item="entry.item"
    :active="true"
    :data-scroll-index="entry.index"
    @download="sendMediaOverviewDownloadCommand($event)"
    @open-media-details="onOpenMediaDetails" />
  <div
    v-else
    class="poster-placeholder"
    :data-scroll-index="entry.index">
    <q-skeleton type="rect" />
  </div>
</template>
```

- [ ] **Step 4: Trigger range prefetch from visible virtual rows**

Convert row range to flat item range:

```ts
const firstFlatIndex = firstRow * get(gridItems);
const lastFlatIndex = ((lastRow + 1) * get(gridItems)) - 1;
```

Then call:

```ts
mediaOverviewStore.ensureRangeLoaded(firstFlatIndex - 50, lastFlatIndex + 50)
```

- [ ] **Step 5: Run WebStorm diagnostics**

Expected: no Vue/TypeScript errors.

---

# Phase 7: Search/filter/sort invalidation

## Task 14: Ensure every query context change resets paging

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`
- Inspect/modify components that set search/filter/sort

- [ ] **Step 1: Metadata filters already call `requestMedia()`**

Existing methods like `setMetaData`, `unsetMetaData`, `changeAllMediaOverviewType`, and `onOptionsClosed` call `requestMedia()`. Since `requestMedia()` delegates to `requestMediaFirstPage()`, they should reset paging automatically.

- [ ] **Step 2: Update sort behavior**

Current `sortMedia` sorts client-side. For paged mode, sorting must become server-side. Update sort toggles to call `requestMediaFirstPage()` instead of sorting loaded items.

Recommended transition:

```ts
toggleSortMedia(field: MediaSortField) {
  if (state.sortedState.field === field) {
    state.sortedState.sort = state.sortedState.sort === SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;
  } else {
    state.sortedState = { field, sort: SortDirection.Asc };
  }

  useSubscription(actions.requestMediaFirstPage().subscribe());
}
```

- [ ] **Step 3: Update search behavior**

Current `getMediaItems` filters locally using `filterQuery`. For paged mode, search should request first page from server. Locate search bar component and ensure query changes call `requestMediaFirstPage()` with debounce.

- [ ] **Step 4: Run WebStorm diagnostics**

Expected: no TypeScript errors.

---

# Phase 8: Selection behavior

## Task 15: Make selection explicit-ID based for paged mode

**Files:**

- Modify: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`
- Modify: `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaTable.vue` if needed

- [ ] **Step 1: Keep row selection as explicit loaded IDs**

Current per-row selection remains valid because it uses media IDs.

- [ ] **Step 2: Adjust root select behavior**

Current root select maps all `state.items`, which will no longer contain every item. For initial paged implementation, root select should only select loaded items or be disabled until query-wide selection is designed.

Recommended initial behavior:

```ts
setRootSelected(value: boolean) {
  const loadedIds = [...state.pageCache.values()]
    .flatMap((items) => items.map((x) => x.id));

  actions.setSelection({
    indexKey: state.selection?.indexKey ?? 0,
    keys: value ? loadedIds : [],
    allSelected: false,
  } as ISelection);
}
```

- [ ] **Step 3: Avoid showing all-selected for unloaded total count**

`isRootSelected` should compare selected IDs against loaded IDs, not `itemsLength`, unless query-wide select is later implemented.

- [ ] **Step 4: Run WebStorm diagnostics**

Expected: no TypeScript errors.

---

# Phase 9: Tests and verification

## Task 16: Backend tests for total count and navigation indexes

**Files:**

- Test: Application unit/integration tests depending on existing endpoint test conventions

- [ ] **Step 1: Add test data with titles**

Use titles that cover buckets:

```text
# -> "1 Documentary"
A -> "Alien"
B -> "Blade Runner"
M -> "Mad Max"
Z -> "Zodiac"
```

- [ ] **Step 2: Assert returned navigation indexes**

Expected example for ascending title order:

```text
# = 0
A = 1
B = 2
M = 3
Z = 4
```

- [ ] **Step 3: Assert filters affect navigation indexes**

Apply a filter/search that removes `Alien`; assert `A` is absent. Also add at least one non-title sort navigation test, preferably `year`, because it is scalar and easy to assert.

- [ ] **Step 4: Run targeted backend tests**

Before running dotnet commands, check for THE FINALS process per Reaparr backend skill. Then run the smallest relevant test project.

---

## Task 17: Frontend store tests for sparse cache and query resets

**Files:**

- Test: frontend unit test location matching existing store test conventions

- [ ] **Step 1: Test page insertion**

Arrange page size 100, insert page 4, assert global index 400 resolves to first item on that page.

- [ ] **Step 2: Test unloaded index returns null**

Assert `getItemByIndex(999)` returns `null` when page is absent.

- [ ] **Step 3: Test `ensureRangeLoaded` page calculation**

For range `95..205` with page size 100, assert pages 0, 1, and 2 are requested.

- [ ] **Step 4: Test navigation jump prefetch**

Given `navigationIndexes = new Map([["M", 41234]])`, assert prefetch is called for around index 41234.

- [ ] **Step 5: Test search/sort reset first page**

Changing search or sort should clear the sparse cache and request page 0 with the active query params.

- [ ] **Step 6: Run targeted frontend tests**

Use the project’s existing Bun/Vitest script via MCP/WebStorm run tooling where available.

---

## Task 18: Manual QA checklist

- [ ] Open a large movie or TV library.
- [ ] Confirm first render does not request `size = 0`.
- [ ] Confirm first render does not download all media.
- [ ] Confirm virtualizer count uses active `totalCount`.
- [ ] Scroll down slowly; placeholders should resolve into rows/posters.
- [ ] Scroll quickly deep into the list; no blank permanent gaps.
- [ ] Click a title label such as `M`; list jumps near M and fills around target.
- [ ] Change sort to year; navigation labels change to years and jump correctly.
- [ ] Change sort to quality; navigation labels change to quality labels if supported.
- [ ] Search for a title; list, count, and navigation labels reset to the searched result set.
- [ ] Change metadata filter; list, count, and navigation labels reset.
- [ ] Select individual loaded rows; download button behavior remains correct.
- [ ] Root select selects loaded rows only and does not claim query-wide selection.

---

# Risks and Follow-up Decisions

## Risk 1: Accurate global index calculation

Calculating first global index per label is hardest when labels are derived from non-trivial fields such as quality or date buckets. Prefer database-side ordering/counting where possible. If an initial implementation projects minimal scalar fields into memory to compute indexes, keep it isolated and add a follow-up optimization issue.

## Risk 2: `MediaCount` semantics

`MediaCount` currently means count of returned items in the DTO mapper. Virtualization must use explicit `TotalCount`. Do not reuse `MediaCount` as total matching count.

## Risk 3: Client-side sorting/filtering must end for paged mode

For 100k media, client-side sorting/filtering of loaded pages gives incorrect global order and incorrect navigation indexes. The final implementation must push sort/search/filter to the backend.

## Risk 4: Query-wide selection

Selecting all matching media across unloaded pages is a separate feature. Initial implementation should make root selection loaded-only or disable root select for paged mode.

## Risk 5: Natural sort parity

Existing all-media behavior uses natural sorting after fetching data, which is incompatible with correct deep paging. The first server-side implementation may use deterministic DB ordering by `SearchTitle`/`SortIndex`. Document any visible sort parity differences and add a follow-up if exact natural sorting is required.

## Risk 6: Generated API workflow

The frontend generated API must be updated from backend Swagger. If backend dev server or generation fails, stop and report rather than silently committing hand-edited generated files.

---

# Recommended commit sequence

1. `feat(api): add active media total count to paged response`
2. `feat(api): support media overview search and server sort query`
3. `feat(api): add sort-aware media navigation indexes`
4. `chore(frontend): regenerate media overview api contracts`
5. `feat(frontend): add sparse media overview page cache`
6. `feat(frontend): load media overview pages by visible range`
7. `feat(frontend): support sort navigation jump prefetch`
8. `feat(frontend): render placeholders for unloaded media rows`
9. `test: cover media overview paging totals and navigation cache`

---

# Completion Criteria

Implementation is complete only when:

- Media overview no longer requests `size = 0` for normal browsing.
- Paged media responses expose `totalCount` for the active query.
- Virtualizer count is based on active `totalCount`.
- Backend applies filters/search/sort before count and page.
- All-media mode no longer sorts only inside an already-paged result.
- Missing items render as placeholders.
- Scrolling loads visible ranges without duplicate page requests.
- Navigation buttons are based on backend-provided `label => global index` values.
- Jumping to a navigation label fetches a local range around the target and scrolls there.
- Metadata/filter/sort/search changes reset sparse cache and navigation labels.
- Targeted backend and frontend tests pass.
