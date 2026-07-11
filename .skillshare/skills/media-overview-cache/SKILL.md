---
name: media-overview-cache
description: ALWAYS load when working on the Reaparr media overview cache system — includes MediaQueryCache, cache invalidation, warmup, GetAllMediaByTypeEndpoint, library sync/refresh commands that invalidate the cache, the mediaOverviewStore frontend, or the MediaOverview component. Covers the stale-while-revalidate pattern, dirty-mark invalidation, background rebuilds, 503-on-miss behavior, auto-retry, and the key structure.
---

# Media Overview Cache

## When to load this skill

Load this skill for **any** work touching:

- `src/Data/Cache/MediaQueryCache.cs` — the cache singleton
- `src/Data.Contracts/Cache/IMediaQueryCache.cs` — the cache interface
- `src/Data/Cache/DTO/MediaQueryMetadataKey.cs` or `MediaQuerySortedListKey.cs` — cache keys
- `src/Application/PlexMedia/GetAll/GetAllMediaByTypeEndpoint.cs` — the `/api/PlexMedia` endpoint
- `src/Application/PlexMedia/WarmupMediaQueryCacheCommand.cs` — the warmup command
- `src/BackgroundJobs/LibrarySync/Commands/RefreshLibraryMedia/*` — any refresh command that invalidates the cache
- `src/BackgroundJobs/LibrarySync/Commands/SyncPlexMoviesCommand.cs` or `SyncPlexTvShowsCommand.cs` — bulk media sync
- `src/PlexApi/PlexMedia/GetAllMediaByTypeFromPlexAPI/GetAllMediaByTypeFromPlexAPICommandHandler.cs` — Plex API fetching
- `src/FluentResultExtensions/DTO/ResultDTOMapper.cs` — status code mapping (503)
- `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts` — frontend overview store
- `src/AppHost/ClientApp/src/components/MediaOverview/MediaOverview.vue` — overview component
- `src/AppHost/ClientApp/src/lang/*.json` — i18n keys for cache state messages
- `src/AppHost/Boot.cs` — startup sequence that triggers warmup

---

## Architecture overview

The media overview cache sits between the `GET /api/PlexMedia` endpoint and the database. Its job is to avoid running the full media query for every sort-direction change, page navigation, or filter toggle.

```
┌──────────────────────────────────────────────┐
│  Browser: MediaOverview.vue                   │
│  → mediaOverviewStore.refreshMediaData()      │
│  → GET /api/PlexMedia?page=1&size=100&...     │
└──────────────────┬───────────────────────────┘
                   │
┌──────────────────▼───────────────────────────┐
│  Backend: GetAllMediaByTypeEndpoint           │
│  → IMediaQueryCache.GetMediaAsync(filter)     │
└──────────────────┬───────────────────────────┘
                   │
         ┌─────────▼──────────┐
         │ MediaQueryCache    │
         │ (SingleInstance)   │
         │                    │
         │  _metadataSnapshots│ ← keyed by MediaQueryMetadataKey
         │  _sortedListSnaps  │ ← keyed by MediaQuerySortedListKey
         │  _builds (in-flight│ ← ConcurrentDictionary<Lazy<>>
         │  _dirtyKeys        │ ← stale-marker for background refresh
         └─────────┬──────────┘
                   │ (cache miss → background build)
         ┌─────────▼──────────┐
         │ ICommandExecutor   │
         │ GetMediaByTypeCmd  │
         └─────────┬──────────┘
                   │
         ┌─────────▼──────────┐
         │ EF Core + SQLite    │
         │ (real query)       │
         └────────────────────┘
```

## Cache key structure

Two-level keying:

### `MediaQueryMetadataKey`
Identifies what data scope:
- `MediaType` — Movie or TvShow
- `LibraryIds` — normalized sorted list of resolved library IDs (0 → all libraries)
- `FilterOfflineMedia` — exclude libraries on offline servers
- `FilterOwnedMedia` — exclude owned-media libraries

### `MediaQuerySortedListKey`
Adds sort ordering:
- `MetadataKey` — the parent metadata key
- `NormalizedAscendingSortField` — canonical ascending sort field name

Sort normalization rules (see `MediaSortNormalizer`):
| Library count | `sortIndex` field | `SearchTitle` field |
|---------------|-------------------|---------------------|
| 1             | `"sortIndex"`     | `"searchTitle"`     |
| > 1           | `"SearchTitle"`   | `"SearchTitle"`     |

The ascending snapshot in `_sortedListSnapshots` is reversed in-memory when the request asks for descending.

## Cache hit flow (normal, warm)

1. `GetMediaAsync` resolves library IDs from the filter, normalizes the sort field.
2. Builds `metadataKey` + `sortedListKey`.
3. Looks up both in `_metadataSnapshots` and `_sortedListSnapshots`.
4. **Hit:** if key is dirty, queues a background refresh. Returns the existing snapshot as `Result.Ok`.
5. **Miss:** queues a background build. Returns `Result.Fail("...")` with `503 Service Unavailable`.

## Cache miss flow (cold or post-invalidation)

1. `GetMediaAsync` returns 503 immediately — **no synchronous blocking**.
2. `QueueSnapshotRefresh` creates a `Lazy<Task<...>>` for the key and launches a fire-and-forget `ObserveBuildAsync`.
3. `ObserveBuildAsync` calls `BuildAndStoreSnapshotAsync`, then on success stores the metadata + sorted list snapshots and removes the dirty marker.
4. On the next request that hits this key, the cache returns it in < 1ms.

## Invalidation flow

Previously: `InvalidateLibraries` removed metadata snapshots, sorted-list snapshots, AND cancelled in-flight builds (ConcurrentDictionary version-bumping). This was **destructive** — a single library refresh nuked the all-libraries snapshot for "All movies".

Now: `InvalidateLibraries` **marks keys as dirty** (unless `SuppressInvalidation` is active):
1. Finds all metadata + sorted-list keys that contain the affected library IDs.
2. Sets `_dirtyKeys[key] = true`.
3. Queues background refresh for affected sorted-list keys.
4. Old snapshots stay in place — requests serve stale data while rebuilds run.

On successful rebuild, `BuildAndStoreSnapshotAsync` atomically swaps the new snapshot and removes the dirty flag.

## Warmup flow (BuildCache)

Called at startup (`Boot.cs`) and after sync quiesce (`WarmupMediaQueryCacheCommand`):

1. Resolves **all** Movie and TvShow library IDs.
2. Builds 16 snapshots: 8 sort fields × 2 media types.
   - Sort fields: `SortIndex`, `SearchTitle`, `Year`, `AddedAt`, `UpdatedAt`, `Duration`, `MediaSize`, `quality`
3. Each snapshot runs `GetMediaByTypeCommand` via `ICommandExecutor` with `plexLibraryId=0`, no page/size limits.
4. Results are stored in `_metadataSnapshots` and `_sortedListSnapshots`.
5. `Has201CreatedRequestSuccess` on the command result is stripped so the status code doesn't leak.

## Bypass conditions

The cache is **bypassed** (query sent directly to DB) when:
- `filter.Parameters.Query` is non-empty (search text)
- `filter.Parameters.Filter` is non-empty (FlexQuery filter DSL)

These are full DB queries; the cache only stores unfiltered overviews.

## Frontend error handling

`mediaOverviewStore.ts`:
- `state.serverError` — set to `true` when `addMediaPage` receives null data (503, 504, network error).
- `state.cacheRetrySeconds` — countdown timer displayed in the component.
- On null data, starts a 5-second countdown that auto-triggers `refreshMediaData()`.
- `refreshMediaData()` clears the timer and resets `serverError` before each request.

`MediaOverview.vue`:
- Checks `mediaOverviewStore.serverError` **before** `allMediaMode`.
- Shows translated `failed-to-load-media` message with retry countdown.

## i18n keys

| Key | Purpose |
|-----|---------|
| `components.media-overview.failed-to-load-media` | Shown when backend returns 503/504/error |
| `components.media-overview.no-media-items-found` | Shown when cache hit but truly 0 items |

## Service registration

`MediaQueryCache` is registered as `SingleInstance` in `DataModule`:
```csharp
// src/Data/_Shared/Config/Autofac/DataModule.cs
builder.RegisterType<MediaQueryCache>().As<IMediaQueryCache>().SingleInstance();
```

## 503 status code mapping

`ResultDTOMapper.ToStatusCode` maps `Has503ServiceUnavailableError → 503`. The cache returns:
```csharp
Result.Fail(CacheWarmingUpMessage).Add503ServiceUnavailableError();
```

This hits `Send.FluentResult(result, ct)` → `SendFluentResultDTOAsync` → `StatusCode = 503`.

## Library sync → cache interaction

```
RefreshLibraryMediaCommand
  → GetLibraryMediaFromPlexApiCommand     (fetch from Plex API)
  → InsertMediaMetaDataCommand            (insert genres/countries/actors)
  → SyncPlexLibraryMediaMetaDataCommand   (sync metadata relations)
  → RefreshPlexMovieLibraryCommand        (movie-specific)
  │   → SyncPlexMoviesCommand             (delete + reinsert)
  │   → mediaQueryCache.InvalidateLibrary(plexLibraryId)
  │
  → RefreshPlexTvShowLibraryCommand       (TV-specific)
  │   → SyncPlexTvShowsCommand            (delete + reinsert)
  │   → mediaQueryCache.InvalidateLibrary(plexLibraryId)
```

**Critical:** `GetAllMediaByTypeFromPlexAPICommandHandler` must return an **error** (not empty success) on Plex API timeout. A timeout returning 0 media causes the library sync to delete all existing DB media and treat the refresh as "successful empty library."

## Common pitfalls

### 1. Key mismatch between warmup and request
`BuildCache` must pass the **actual** resolved library count to `MediaSortNormalizer.Normalize`, not 0. With `libraryCount=0`, single-library warmups normalize `SortIndex → SearchTitle` but single-library requests normalize `sortIndex → "sortIndex"`. The keys never match, and the warmup is useless.

### 2. Competing Normalize extension method
`FlexQuery.NET.dll` also defines a `Normalize` extension on `string`. Always use `MediaSortNormalizer.Normalize(filter.Parameters.Sort, count)` fully qualified. Without qualification, the compiler resolves to the wrong overload.

### 3. Singleton state leaks
`MediaQueryCache` is a singleton. All dictionaries (`_metadataSnapshots`, `_sortedListSnapshots`, `_builds`, `_dirtyKeys`) persist across requests. Integration tests must clear or reset these between test runs.

### 4. Sync delete-then-reinsert is a point of no return
`SyncPlexMoviesCommand.RemoveMedia` uses `CancellationToken.None` and deletes all movies before reinserting. If the reinsert fails, the data is gone. Always verify Plex API success before reaching this point.

### 5. Plex response missing TotalSize
`GetAllMediaByTypeFromPlexAPICommandHandler.GetLibraryMediaTotalCount` defaults `TotalSize` to 0 when missing. A missing `TotalSize` with valid metadata is indistinguishable from an empty library.

### 6. SuppressInvalidation is reset in finally block
Phase 2 of the startup warmup wraps the suppression flag in try/finally: the flag is always reset regardless of cancellation or exception. Do not remove the finally block — if `SuppressInvalidation` stays `true`, all library sync invalidation becomes permanently disabled for the lifetime of the container.

### 7. SyncQuietPeriod is protected virtual with setter
Tests override it via reflection. If the property signature changes (e.g. back to expression-bodied), tests break with "Property set method not found." Keep the `{ get; set; }` form.

## Related ADRs / issues

- GitHub issue (SignalR cache-warmup events): tracks replacing the frontend polling retry with real-time SignalR notifications when cache warmup starts/completes.

---

## Startup phased warmup (anti-doom-loop)

`WarmupMediaQueryCacheCommand` is called from `Boot.cs` at startup. It runs in three phases to avoid cache-doom loops during the initial library sync storm:

```
Phase 1: BuildCache() immediately from existing DB data
  → Users see media instantly on container restart
  → If DB is empty (fresh install), builds empty cache quickly

Phase 2: SuppressInvalidation = true
  → Library sync storm runs freely
  → InvalidateLibrary / InvalidateLibraries are no-ops
  → Cache serves stale (phase-1) data — no 503, no doom loops
  → Wait for all LibrarySyncJobQueues to settle + 5 min quiet period

Phase 3: SuppressInvalidation = false, then BuildCache()
  → One clean rebuild after sync storm ends
  → Future invalidations work normally (dirty-mark + background refresh)
```

**`SuppressInvalidation`** is a boolean property on `IMediaQueryCache` / `MediaQueryCache`:
- Default `false`
- When `true`, `InvalidateLibraries` logs a debug message and returns without marking keys dirty
- Only set during Phase 2 of the startup warmup
- Normal operation (user-triggered ownership/access changes) always has it `false`

### SyncQuietPeriod

The quiet period defaults to 5 minutes and is overridable for tests via `protected virtual TimeSpan SyncQuietPeriod { get; set; }`.

After the quiet period elapses, the handler re-checks for newly queued sync jobs. If any are found, it waits again. This handles sync chains where completing one library refresh queues the next.

## i18n keys

| Key | Purpose |
|-----|---------|
| `components.media-overview.failed-to-load-media` | Shown when backend returns 503/504/error |
| `components.media-overview.retrying-in-seconds` | Shown with countdown during auto-retry (e.g. "Retrying in 5s...") |
| `components.media-overview.no-media-items-available` | Shown when cache hit but truly 0 items in all-media mode |
