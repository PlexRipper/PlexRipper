---
name: reaparr-media-comparison
description: Use for all Reaparr cross-library media comparison work — writing/running comparisons, projecting comparison state into browse DTOs, persisting scopes and hit rows, enqueueing comparison jobs, debugging comparison results, or modifying the comparison pipeline in any backend or frontend layer. Covers both Movie and TvShow comparisons (Music stubbed for future).
---

# Reaparr Media Comparison

## Purpose

This skill must be loaded before any work on the Reaparr cross-library media comparison feature. It covers the full backend pipeline (queueing, execution, persistence, projection) and the frontend display (badge rendering on `MediaPoster.vue`). Load `reaparr-backend` first for backend changes; load `reaparr-frontend` and `reaparr-frontend-components` first for frontend changes.

The comparison feature answers: for each item in a remote Plex library, does **any current owned library** have the same title, and is the remote copy a higher-quality upgrade candidate? A remote item is not considered missing just because some owned libraries lack it; one current owned-library hit is enough to make it owned.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         WRITE PATH                              │
│                                                                 │
│  LibrarySyncJob (after sync)                                    │
│  AddOrUpdatePlexLibrariesCommandHandler (ownership change)     │
│  SetServerOwnedEndpoint (OwnedOverride change)                  │
│        │                                                        │
│        ▼                                                        │
│  QueueLibraryComparisonJobsForLibraryCommandHandler             │
│    - Given ONE library ID, finds same-type remote↔owned pairs  │
│    - Uses WhereIsOwned()/WhereIsNotOwned() for ownership       │
│        │                                                        │
│        ▼                                                        │
│  QueueLibraryMediaCompareJobCommandHandler                      │
│    - Upserts LibraryComparisonJobQueues row (deduped by        │
│      RemotePlexLibraryId + OwnedPlexLibraryId + MediaType)     │
│        │                                                        │
│        ▼                                                        │
│  PlexLibraryComparisonJob [DisallowConcurrentExecution]         │
│    - Singleton worker picks next Queued row from DB             │
│    - Marks Processing, dispatches compare command               │
│    - Marks Completed or Failed                                  │
│    - Schedules self if more rows remain                         │
│        │                                                        │
│        ▼                                                        │
│  CompareMoviePlexLibraryCommandHandler                          │
│  CompareTvShowPlexLibraryCommandHandler                         │
│    - Load remote + owned media from DB                          │
│    - Match by GUID waterfall, then title+year                   │
│    - Compare quality via VideoQuality.ToId()                    │
│    - Write hit rows + scope row via ExecuteWithRetryAsync       │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                         READ PATH                               │
│                                                                 │
│  GetMediaByTypeCommandHandler (overview browse)                 │
│    - Single-library requests → ApplyComparisonStateAsync        │
│    - Multi-library / cached → MediaQueryCache (no comparison)   │
│        │                                                        │
│        ▼                                                        │
│  ApplyComparisonStateCommandHandler                             │
│    - Checks library ownership via WhereIsOwned()                │
│    - Routes to one of 4 sub-handlers by media type + direction  │
│        │                                                        │
│        ├── ApplyRemoteMovieComparisonStateCommandHandler        │
│        ├── ApplyRemoteTvShowComparisonStateCommandHandler       │
│        ├── ApplyOwnedMovieComparisonStateCommandHandler         │
│        └── ApplyOwnedTvShowComparisonStateCommandHandler        │
│                                                                 │
│    Each sub-handler:                                            │
│    1. Loads the target library's UpdatedAt                      │
│    2. Loads compatible libraries (owned for remote view,        │
│       remote for owned view) using WhereIsOwned/WhereIsNotOwned │
│    3. Queries PlexComparisonScopes                              │
│    4. Filters to CURRENT scopes only:                           │
│       scope.RemoteLibraryUpdatedAt == remote.UpdatedAt AND      │
│       scope.OwnedLibraryUpdatedAt == owned.UpdatedAt            │
│    5. Joins hit tables against page item IDs                    │
│    6. Sets PlexMediaSlimDTO.ComparisonState per item            │
└─────────────────────────────────────────────────────────────────┘
```

## Key Entities

### PlexComparisonState (scopes table)

One row per `(RemotePlexLibraryId, OwnedPlexLibraryId, MediaType)`. Unique index: `UX_PlexComparisonScopes_RemoteOwnedType`.

| Field | Purpose |
|-------|---------|
| `RemotePlexLibraryId` | Remote library being compared from |
| `OwnedPlexLibraryId` | Owned library being compared to |
| `MediaType` | `Movie`, `TvShow`, etc. |
| `CompletedAt` | When comparison finished |
| `RemoteLibraryUpdatedAt` | Snapshot of remote library's `UpdatedAt` at compare time |
| `OwnedLibraryUpdatedAt` | Snapshot of owned library's `UpdatedAt` at compare time |

**A scope is "current" only when BOTH `UpdatedAt` snapshots match the live library values.** If either snapshot is stale, the scope is ignored and items stay `NotCompared` (or `Pending` if a queue row exists).

### Hit Tables (per media type)

Four tables — one per media type:
- `PlexMovieComparison` — keyed by `(RemotePlexLibraryId, OwnedPlexLibraryId, RemotePlexMediaId, OwnedPlexMediaId)`
- `PlexTvShowComparison` — same pattern for shows
- `PlexSeasonComparison` — for seasons
- `PlexEpisodeComparison` — for episodes

Every hit row has:
- `HitState`: `Matched` (0) or `HigherQuality` (1)
- `RemoteQuality` / `OwnedQuality`: snapshot `VideoQuality` values
- `MatchType`: which waterfall layer produced the match
- `ComparedAt`: timestamp

**CRITICAL**: A `Matched` row is stored when quality is equal or owned is better. **Missing** is derived via anti-join, not stored: when browsing a remote library, a remote media item is `Missing` only if it has NO hit row in ANY current owned-library scope. If at least one current owned library has a hit, the item is `Owned` unless any hit is `HigherQuality`.

### Enums

**`PlexMediaComparisonState`** (set on DTO per media item):
| Value | Meaning |
|-------|---------|
| `NotCompared` (0) | No current scope for this library pair / direction |
| `Owned` (1) | Current scope exists; item matched at least one current owned target at equal-or-better quality |
| `Missing` (2) | Current scope exists; remote item has zero hits across all current owned libraries |
| `HigherQuality` (3) | Current scope exists; at least one hit is an upgrade candidate where remote quality exceeds owned quality |
| `Pending` (4) | Comparison queued/processing, no current scope yet — show spinner |

There is currently **no** backend `MissingAndHigherQuality` enum value. Do not design projection logic around that state unless the enum, generated TypeScript, and frontend badge rendering are deliberately extended together.

**`PlexMediaComparisonHitState`** (stored in hit rows):
| Value | Meaning |
|-------|---------|
| `Matched` (0) | Same-or-better quality on owned side |
| `HigherQuality` (1) | Remote is strictly higher quality — upgrade candidate |

**`PlexMediaComparisonMatchType`**:
`TmdbGuid` → `ImdbGuid` → `TvdbGuid` → `NormalizedTitleYearAndDuration` → `NormalizedTitleAndYear`

## Matching Waterfall

Both movie and TV show matching follow the same waterfall. Each level returns ALL matching candidates (not just the first):

1. **TMDB GUID** — `remote.Guid_TMDB` matches `owned.Guid_TMDB`
2. **IMDB GUID** — `remote.Guid_IMDB` matches `owned.Guid_IMDB`
3. **TVDB GUID** — `remote.Guid_TVDB` matches `owned.Guid_TVDB`
4. **SearchTitle + Year + Duration** — case-insensitive `SearchTitle` + exact `Year` + exact `Duration` (only when `Duration > 0` on both sides)
5. **SearchTitle + Year** — case-insensitive `SearchTitle` + exact `Year` (fallback when duration mismatch or zero)

Quality comparison: `remoteQuality.ToId() > ownedQuality.ToId()` — uses the project's quality ID mapping (not raw enum ordering).

**TV-specific**: Shows are matched first by the above waterfall. Seasons and episodes are only matched within already-matched show and season context, by `SeasonNumber` and `EpisodeNumber`. A season/episode mismatch does not invalidate the show match.

## Concurrency and SQLite

### BusyTimeout
Set to **30 seconds** in `DbContextConnections.cs` to match Quartz's busy_timeout (PRAGMA busy_timeout=30000). This prevents `SQLITE_BUSY` errors when comparison writes overlap with read queries.

### ExecuteWithRetryAsync
`IReaparrDbContext` exposes `ExecuteWithRetryAsync<T>(Func<IReaparrDbContext, Task<T>>, ...)`. Implementation in `ReaparrDbContext.cs` wraps the `EntityFrameworkCore.Sqlite.Concurrency` library extension — casts `Func<IReaparrDbContext, ...>` to `Func<DbContext, ...>` inside the method. Retries on `SQLITE_BUSY` with exponential backoff.

### Write Pattern (in comparison handlers)
```csharp
// Do NOT use BeginTransactionAsync — it bypasses the concurrency library's write queue.
// Do NOT use BulkInsertAsync (EFCore.BulkExtensions) — it does raw SQL.
// Use AddRange + SaveChangesNewAsync instead.

await _dbContext.ExecuteWithRetryAsync(async dbContext =>
{
    // 1. Delete old hit rows
    await dbContext.PlexMovieComparisons
        .Where(x => x.RemotePlexLibraryId == remoteId && x.OwnedPlexLibraryId == ownedId)
        .ExecuteDeleteAsync(ct);

    // 2. Insert new hit rows (standard EF, not BulkInsert)
    dbContext.PlexMovieComparisons.AddRange(hitRows);
    await dbContext.SaveChangesNewAsync(ct);

    // 3. Update scope LAST — makes new hits visible atomically.
    //    Existing scope rows MUST be queried with AsTracking() because the
    //    Reaparr DbContext defaults to QueryTrackingBehavior.NoTracking.
    var state = await dbContext.PlexComparisonScopes
        .AsTracking()
        .SingleOrDefaultAsync(x =>
            x.RemotePlexLibraryId == remoteId
            && x.OwnedPlexLibraryId == ownedId
            && x.MediaType == mediaType,
            ct);

    //    ...create or update PlexComparisonState row...
    await dbContext.SaveChangesNewAsync(ct);
    
    return 0;
}, cancellationToken: ct);
```

If an existing `PlexComparisonScopes` row is updated without `AsTracking()`, the timestamp assignments are silently lost under the no-tracking DbContext. Symptoms: hit rows update, jobs complete, but scope snapshots stay stale and browse projection keeps returning `NotCompared` or old states.

## Ownership Detection

Use the extension methods in `DbSetExtensions.PlexLibrary.cs`:
- `WhereIsOwned()` — library belonging to an owned server
- `WhereIsNotOwned()` — library NOT belonging to an owned server

These check three sources, matching the existing `DbSetExtensions.PlexServer.cs` pattern:
1. `PlexServer.OwnedOverride == true`
2. `PlexAccountLibraries.Any(y => y.IsLibraryOwned)`
3. `PlexServer.PlexAccountServers.Any(y => y.IsServerOwned)`

## Queue Model

### LibraryComparisonJobQueue
Table: `BackgroundJobLibraryComparisonJobQueues`. Composite key: `(RemotePlexLibraryId, OwnedPlexLibraryId, MediaType)`.

| Field | Purpose |
|-------|---------|
| `Priority` | Higher runs first. Movie=1, TvShow=2 |
| `Status` | `Queued`, `Processing`, `Completed`, `Failed` (reuses `LibrarySyncJobStatus`) |
| `Attempts` | Incremented on each try |
| `ErrorMessage` | Set on failure, cleared on requeue |
| `CreatedAt` / `StartedAt` / `CompletedAt` | Timestamps |

### Startup flow
`SchedulerService.SetupLibraryComparisonJob`:
1. `CleanupLibraryComparisonJobQueueCommand` — resets `Processing` rows to `Queued`, deletes `Completed`
2. `CheckQueuedLibraryComparisonJobCommand` — starts the singleton worker if rows are pending

### Enqueue hooks
- `LibrarySyncJob` — after successful sync, sends `QueueLibraryComparisonJobsForLibraryCommand` (logs and continues on failure)
- `AddOrUpdatePlexLibrariesCommandHandler` — after access/ownership changes
- `SetServerOwnedEndpoint` — after `OwnedOverride` change

## Frontend

### TypeScript types
`PlexMediaSlimDTO` and `PlexMediaDTO` in `data-contracts.ts` include:
- `comparisonState: PlexMediaComparisonState` — the enum
- `isComparisonPending: boolean` — set by projection handlers when comparison is queued but not yet computed

**Must run `bun run generate-ts`** with backend running after ANY change to `PlexMediaSlimDTO`, comparison state enums, or comparison DTO fields.

### Badge rendering
`MediaPoster.vue` has a `comparisonBadge` computed property that reads `props.mediaItem.comparisonState` (with both enum and raw-number fallback) and returns icon/label/color for each state. When `props.mediaItem.isComparisonPending` is true, shows a loading spinner chip regardless of `comparisonState`.

## Debug Endpoints

### GET /api/Debug/movie-library-comparison?remoteLibraryId=X&ownedLibraryId=Y
Runs `CompareMoviePlexLibraryCommand` **synchronously** (no queuing), then returns:
- `Matched[]` — hit rows with `HitState == Matched`
- `HigherQuality[]` — hit rows with `HitState == HigherQuality`
- `MissingCount` — anti-join count

### GET /api/Debug/tv-show-library-comparison?remoteLibraryId=X&ownedLibraryId=Y
Same pattern but returns three sections (`Shows`, `Seasons`, `Episodes`), each with Matched, HigherQuality, and MissingCount.

## Key Source Files

### Backend — Write Path
| File | Role |
|------|------|
| `src/Application.Contracts/PlexMedia/Comparison/CompareMoviePlexLibraryCommand.cs` | Movie compare command contract |
| `src/Application.Contracts/PlexMedia/Comparison/CompareTvShowPlexLibraryCommand.cs` | TV compare command contract |
| `src/Application/PlexLibraries/Comparison/Movie/CompareMoviePlexLibraryCommandHandler.cs` | Movie matching, quality comparison, persistence |
| `src/Application/PlexLibraries/Comparison/TvShow/CompareTvShowPlexLibraryCommandHandler.cs` | TV matching, season/episode context matching |
| `src/BackgroundJobs/LibraryComparison/PlexLibraryComparisonJob.cs` | Singleton queue worker |
| `src/BackgroundJobs/LibraryComparison/QueueLibraryMediaCompareJobCommandHandler.cs` | Upserts queue rows |
| `src/BackgroundJobs/LibraryComparison/QueueLibraryComparisonJobsForLibraryCommandHandler.cs` | Pair discovery from one library ID |
| `src/BackgroundJobs/LibraryComparison/CheckQueuedLibraryComparisonJobCommandHandler.cs` | Worker startup scheduling |
| `src/BackgroundJobs/LibraryComparison/CleanupLibraryComparisonJobQueueCommandHandler.cs` | Reset after shutdown |
| `src/Domain/Entities/BackgroundJobs/LibraryComparisonJobQueue.cs` | Queue entity |
| `src/Domain/Entities/Comparison/PlexComparisonState.cs` | Scope entity |
| `src/Domain/Entities/Comparison/PlexMovieComparison.cs` | Movie hit entity |
| `src/Domain/Entities/Comparison/PlexTvShowComparison.cs` | TV show hit entity |
| `src/Domain/Entities/Comparison/PlexSeasonComparison.cs` | Season hit entity |
| `src/Domain/Entities/Comparison/PlexEpisodeComparison.cs` | Episode hit entity |
| `src/Domain/_Shared/Enums/PlexMediaComparisonState.cs` | DTO state enum |
| `src/Domain/_Shared/Enums/PlexMediaComparisonHitState.cs` | Hit state enum |
| `src/Domain/_Shared/Enums/PlexMediaComparisonMatchType.cs` | Match type enum |

### Backend — Read Path
| File | Role |
|------|------|
| `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaSlimDTO.cs` | DTO with `ComparisonState` and `IsComparisonPending` |
| `src/Application.Contracts/PlexMedia/Comparison/ApplyComparisonStateCommand.cs` | Single-entry projection command |
| `src/Application/PlexLibraries/Comparison/State/ApplyComparisonStateCommandHandler.cs` | Ownership routing coordinator |
| `src/Application/PlexLibraries/Comparison/State/ApplyRemoteMovieComparisonStateCommand.cs` | Remote movie state projection (command + handler in one file) |
| `src/Application/PlexLibraries/Comparison/State/ApplyRemoteTvShowComparisonStateCommand.cs` | Remote TV state projection |
| `src/Application/PlexLibraries/Comparison/State/ApplyOwnedMovieComparisonStateCommand.cs` | Owned movie HigherQuality projection |
| `src/Application/PlexLibraries/Comparison/State/ApplyOwnedTvShowComparisonStateCommand.cs` | Owned TV HigherQuality projection |
| `src/Data/Queries/GetMediaByTypeCommandHandler.cs` | Overview handler — dispatches ApplyComparisonStateCommand |
| `src/Data.Contracts/Extensions/DbSet/DbSetExtensions.PlexLibrary.cs` | `WhereIsOwned` / `WhereIsNotOwned` |

### Backend — Infrastructure
| File | Role |
|------|------|
| `src/Data/ReaparrDbContext.cs` | `ExecuteWithRetryAsync` wrapper, `SaveChangesNewAsync` (→ `SaveChangesSerializedAsync`) |
| `src/Data.Contracts/Interfaces/IReaparrDbContext.cs` | Interface with `ExecuteWithRetryAsync` declaration |
| `src/Data.Contracts/Extensions/DbContextConnections.cs` | BusyTimeout=30s config |
| `src/Data/Configurations/PlexComparisonStateConfiguration.cs` | Scope EF config |
| `src/Data/Configurations/LibraryComparisonQueueConfiguration.cs` | Queue EF config |
| `src/Application/BackgroundServices/SchedulerService.cs` | `SetupLibraryComparisonJob` |

### Debug Endpoints
| File | Route |
|------|-------|
| `src/Application/PlexLibraries/Comparison/Movie/GetMovieLibraryComparisonDebugEndpoint.cs` | `GET /api/Debug/movie-library-comparison` |
| `src/Application/PlexLibraries/Comparison/TvShow/GetTvShowLibraryComparisonDebugEndpoint.cs` | `GET /api/Debug/tv-show-library-comparison` |

### Frontend
| File | Role |
|------|------|
| `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/MediaPoster.vue` | `comparisonBadge` computed, badge rendering |
| `src/AppHost/ClientApp/src/types/api/generated/data-contracts.ts` | Generated `PlexMediaComparisonState`, `PlexMediaSlimDTO.comparisonState` |

## Troubleshooting Comparison Badges

### First check the live API payload

Use Chrome DevTools Network before blaming Vue/Pinia. The relevant browse request is usually:

```text
GET /api/PlexMedia?q=&page=1&size=100&sort=sortIndex:asc&mediaType=Movie&plexLibraryId=34&filterOwnedMedia=true&filterOfflineMedia=true
```

Inspect the target item in `mediaList[]` and note `id`, `plexLibraryId`, `plexServerId`, `plexApiRatingKey`, `plexApiMetaDataKey`, `title`, `year`, and `comparisonState`. If the payload already has the wrong state, the bug is backend projection or comparison data, not `MediaPoster.vue`.

### "Everything Shows NotCompared"

1. **Check scope exists**: `PlexComparisonScopes` row for the pair with matching string `MediaType` such as `Movie` or `TvShow`.
2. **Check scope freshness**: `RemoteLibraryUpdatedAt` and `OwnedLibraryUpdatedAt` must match the live `PlexLibrary.UpdatedAt` values. If the library was refreshed after comparison, the scope is stale.
3. **Check scope persistence**: existing scope rows must be loaded with `.AsTracking()` before assigning `CompletedAt`, `RemoteLibraryUpdatedAt`, or `OwnedLibraryUpdatedAt`; otherwise no-tracking queries discard the update.
4. **Check projection path**: single-library overview requests go through `GetMediaByTypeCommandHandler` → `ApplyComparisonStateAsync`. Multi-library cached requests bypass comparison projection — `MediaQueryCache` does not project comparison state.
5. **Check `isComparisonPending` vs `comparisonState`**: The frontend checks `isComparisonPending` first. If it's `true`, the pending spinner shows regardless of `comparisonState`.

### "Item Exists In One Owned Library But Shows Missing"

Expected remote-library semantics: `Owned` if any current owned library has a same-or-better hit; `HigherQuality` if any current owned-library hit is `HigherQuality`; `Missing` only if zero current owned libraries have hits.

Debug with SQL/data checks:
1. Find the remote and owned media rows in `PlexMovie` / `PlexTvShow` by `Title`, `SearchTitle`, library ID, or Plex keys.
2. Check `PlexMovieComparisons` / `PlexTvShowComparisons` for rows matching `RemotePlexMediaId`, `OwnedPlexMediaId`, `RemotePlexLibraryId`, and `OwnedPlexLibraryId`.
3. Remember `PlexMediaComparisonHitState.Matched = 0` and `HigherQuality = 1`; `Matched` means owned/equal quality, not a failure.
4. Aggregate across all current owned library scopes. Do not mark `Missing` just because one of several owned libraries lacks the item.

### "Everything Shows Missing"

1. **Run the debug endpoint** to confirm comparison produces correct hit rows for the library pair.
2. **Check hit rows**: missing is an anti-join result; there are no stored "missing" hit rows.
3. **Check projection aggregation**: remote projection must group hits by remote media ID and only set `Missing` when that group is absent.
4. **Check generate-ts**: `bun run generate-ts` must have been run after any `PlexMediaSlimDTO`, enum, or comparison DTO changes. Backend must be running.

## Future Work (Not Yet Implemented)

- **Comparison filters**: `GetAllMediaByTypeEndpoint.BuildFilter` does not yet map comparison state filters (Missing, HigherQuality, etc.)
- **Comparison column in table view**: `MediaQTable.vue` does not yet have a Comparison column
- **TV season/episode detail comparison**: `MediaList.vue` does not yet show per-episode comparison
- **`QueueUpgradeDownloadCommand`**: Not yet implemented — the endpoint and auto-selector for upgrades
- **SignalR `ComparisonCompleted` event**: Not yet wired
- **Music comparison**: Stubbed; `PlexMediaType.Music/Artist/Album/Song` support not implemented
- **Cypress tests**: No comparison-specific E2E tests exist yet
- **Comparison in MediaQueryCache**: Cached all-library snapshots do not project comparison state

## Common Mistakes

- Calling `BeginTransactionAsync` directly — bypasses the concurrency library write queue. Use `ExecuteWithRetryAsync` instead.
- Using `BulkInsertAsync` from EFCore.BulkExtensions — does raw SQL that bypasses the write queue. Use `AddRange` + `SaveChangesNewAsync`.
- Using the raw `VideoQuality` enum for comparison — use `VideoQuality.ToId()`.
- Forgetting scope freshness checks — a scope whose `UpdatedAt` snapshots don't match live library values must be treated as stale/not-current.
- Not deriving Missing via anti-join — there is no "Missing" hit row. Remote-library Missing = remote media with no matching hit in any current owned-library scope.
- Aggregating Missing per owned target — wrong for remote browse. Any current owned-library hit prevents Missing; `HigherQuality` still wins over `Owned` when any hit is an upgrade candidate.
- Updating scope before inserting hit rows — scope goes LAST for atomic visibility.
- Updating an existing scope row without `.AsTracking()` — no-tracking DbContexts silently drop the timestamp update.
- Running comparison during normal browse — comparison must be background-only (except debug endpoints).
- Trusting the UI before checking the `/api/PlexMedia` payload — inspect the backend payload first with Chrome DevTools.
- In tests, adding entities to one `IDbContext` instance and calling `SaveChangesNewAsync` on another — `BaseUnitTest.IDbContext` returns fresh contexts.
- In tests, creating "current" scopes with null or stale `PlexLibrary.UpdatedAt` snapshots — projection exits early and leaves items `NotCompared`.
- Not running `bun run generate-ts` after DTO changes.
