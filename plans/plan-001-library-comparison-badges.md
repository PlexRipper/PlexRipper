# Plan 001 — Plex library comparison indicators

## Goal

Implement issue [#73](https://github.com/Reaparr/Reaparr/issues/73): compare owned and remote Plex libraries so users can see what is missing, what can be upgraded, where those differences are, and act on them from both owned and remote library views.

This feature is media-type agnostic. It should work for any hierarchical media family:

- top-level media, e.g. movie, TV show, album,
- mid-level media, e.g. season, disc,
- leaf-level media, e.g. episode, track.

The first implementation targets movies, TV shows, seasons, and episodes, while keeping the model compatible with future music support.

## Feature principles

1. **Owned and remote views both matter**  
   Remote libraries answer “what am I missing or what is better remotely?” Owned libraries answer “what can I upgrade or fill in locally?”.

2. **Efficient and high-performance**  
   Large libraries can contain 100,000+ items. Comparison must be calculated during library/account changes and queried efficiently during browsing. Do not compare large libraries in the frontend.

3. **Compatible with current features**  
   Keep existing browsing, filtering, selection, and download flows. Add comparison indicators, filters, and upgrade actions without replacing the current media overview behavior.

4. **Universal comparison model**  
   Use the same comparison language across movies, TV shows, seasons, episodes, and later music. Avoid movie-only or TV-only concepts where a hierarchy-based model works.

5. **Icons first, text second**  
   Use media-type icons wherever possible. Text explains counts and tooltips; it should not be the only signal.

6. **Confidence-aware matching**  
   GUID matches are strongest. Title/year fallback is useful, but store which waterfall layer caused the match so confidence can be exposed or filtered later.

## Primary user questions

The feature should answer these directly:

- **What am I missing?** Show missing media when browsing remote libraries.
- **What can I upgrade?** Show higher-quality remote alternatives when browsing owned libraries.
- **Where is it missing or better?** Show the owned server/library target when multiple owned libraries exist.
- **How much is missing?** Distinguish full missing mid-level items, e.g. seasons, from loose leaf-level items, e.g. episodes.
- **What quality would I gain?** Compare current owned best quality to the remote quality.
- **Can I act on it quickly?** Use upgrade/download actions from the existing selection/download flow.
- **Can I filter by it?** Add comparison filters to the existing media filter system.
- **How confident is the match?** Keep this internal initially, but store enough information to expose it later.

## Desired user experience

### Remote library browsing

Remote library rows/cards should show whether the remote media is:

- missing from owned libraries,
- higher quality than owned media,
- already owned at same-or-better quality.

Examples:

| Media level | Missing indicator | Higher-quality indicator |
|---|---|---|
| Top-level | media icon + `Missing` | media icon + quality-up icon |
| Mid-level | mid-level icon + count | mid-level or leaf-level icon + quality-up count |
| Leaf-level | leaf icon + `Missing` | leaf icon + quality-up icon |

For TV shows, prefer structured counts:

```text
2 seasons + 5 episodes missing
18 episodes higher quality
```

If space is constrained on poster cards, collapse to icons/counts and use tooltip text for the full wording.

### Owned library browsing

When browsing owned Plex libraries, comparison should invert from “what am I missing remotely?” to “what can I improve locally?”

- Movies: show an upgrade icon instead of the normal download icon when a higher-quality remote match exists.
- TV shows: show an upgrade action when any season/episode is missing or any owned episode has a higher-quality remote match.
- Seasons: upgrade action downloads missing episodes in the season and upgrades lower-quality owned episodes.
- Episodes: upgrade action downloads the better remote episode.

This keeps the action close to the existing download flow while making owned-library maintenance faster.

### Table view

Add a compact **Comparison** column for media overview tables.

| Title | Quality | Comparison | Year | Size | Actions |
|---|---|---|---|---|---|
| Example Movie | 1080p | movie icon + Missing | 2024 | 8 GB | Download |
| Example Show | Mixed | season icon `2` + episode icon `5` missing, episode icon `18` HQ | 2020 | 140 GB | Download |

Do not overload the title column. Comparison is its own concept and needs a dedicated column.

### TV show detail page

The TV show detail page has extra room and table context, so it can be more explicit than poster cards:

- Show aggregate comparison near the show header/poster.
- Show season-level comparison in the season row/header.
- Add an episode-level comparison column to the episode table.
- Episode rows should show single-item signals: missing, higher quality, or no indicator.

### Filter integration

Add comparison filters to the current media filter system:

- Missing from owned libraries.
- Higher quality available.
- Missing or higher quality.
- Owned / no action needed.

The filters should use stored/indexed comparison results, not recalculate comparison in the frontend.

## Comparing algorithm

### Overview

Comparison runs as a waterfall. Each step only runs if the previous stronger match did not produce a confident match.

```text
candidate remote/owned media
  -> same media family and hierarchy level
  -> GUID waterfall
  -> title/year waterfall
  -> structural fallback for child media
  -> no confident match = missing
  -> confident match with lower owned quality = higher quality
```

### Match confidence levels

| Level | Match source | Confidence | Notes |
|---|---|---|---|
| 1 | Exact GUID match | Highest | TMDB, IMDB, TVDB from Plex `guids` |
| 2 | Provider GUID cross-match | High | Same item through another known provider ID |
| 3 | Normalized title + exact year | Medium | Useful for top-level items when GUIDs are missing |
| 4 | Normalized title + exact year + duration | Medium | Better top-level video fallback when runtime is available |
| 5 | Parent match + season/episode numbers | Medium | TV episode fallback after show is matched |
| 6 | Parent match + disc/track numbers | Medium | Future music fallback after album is matched |
| 7 | Title-only fuzzy match | Low | Do not auto-match for now |

Store the waterfall layer that produced the match in the comparison table. Later UI can display match confidence or hide low-confidence matches if users report false positives.

Do not use Plex `ratingKey`, library key, or metadata key across servers. Those are server-local. Reaparr also stores `BasePlexMedia.Guid`, and PlexAPI documents it as a `plex://movie/...` / `plex://show/...` GUID. Treat that full Plex GUID as **unproven across different Plex servers**: only consider it later if real owned/remote samples prove it stable for the same metadata agent. For this plan, use TMDB/IMDB/TVDB and conservative title/year fallback.

### GUID waterfall

For each item, attempt exact identifier matches in this order:

1. `Guid_TMDB`
2. `Guid_IMDB`
3. `Guid_TVDB`
4. Do not use Plex server-local keys. Only revisit full `BasePlexMedia.Guid` after validating cross-server stability with real samples.

Rules:

- Compare only within the same logical media family and hierarchy level.
- Do not let an episode match a movie or a show match a season.
- If multiple owned candidates match, keep each hit as its own comparison row.
- If any owned match is same or better quality, that owned target does not need a higher-quality action.

### Title/year fallback

If no GUID match exists, use normalized metadata fallback:

1. Normalize title:
   - lower-case,
   - trim punctuation and repeated whitespace,
   - ignore common edition suffixes only if Reaparr already stores them separately,
   - avoid aggressive fuzzy matching by default.
2. Match year:
   - exact year only,
   - no one-year tolerance for now because false positives are worse than missed fallback matches.
3. Add duration/runtime when available:
   - useful for top-level video,
   - avoid for top-level TV shows where runtime is less stable.

Fallback examples:

| Remote item | Owned candidate | Result |
|---|---|---|
| Same normalized movie title + same year | same media type | match |
| Same show title + same start year | same media type | match |
| Same title but different year | same media type | no automatic match |
| Same episode title only | episode | no automatic match without parent/season/episode context |

### Child media fallback

For hierarchical media, use parent structure to reduce false matches.

TV:

1. Match the TV show using the GUID waterfall.
2. If no show GUID match exists, match show by normalized title + exact year.
3. For seasons, compare by matched show + `SeasonNumber`.
4. For episodes, compare by matched show + `SeasonNumber` + `EpisodeNumber`.
5. Use episode GUIDs first when present; structural fallback is only for missing episode GUIDs.

Future music:

1. Match album by GUID or normalized artist + album + exact year.
2. Match discs by album + disc number.
3. Match tracks by album + disc number + track number.

Media edge cases to account for:

- Specials / season 0 episodes.
- Multi-part episodes where Plex may split or merge files differently.
- Different TV ordering between DVD, aired, and absolute order.
- Remakes with the same title but different years.
- Movies with editions/cuts where same title/year may not mean same content.
- Anime where season/episode numbering may differ between metadata agents.
- Shows with reused episode titles.
- Duplicate owned copies at different qualities.
- Remote media with optimized versions or multiple media parts.
- Future music: deluxe albums, remasters, multi-disc releases, and duplicate track titles.

### Aggregation rules

- A mid-level item is fully missing when all expected leaf-level children are missing.
- A mid-level item can still contain higher-quality leaf-level items if some children exist locally but at lower quality.
- A top-level item can report both fully missing mid-level items and individual missing leaf-level items.
- Compact poster badges may suppress child counts already represented by full missing parent counts; detail views can show the full breakdown.

## Persistence and query strategy

Comparison can be expensive on large libraries. Do not rely on per-page ad-hoc comparison as the long-term design.

Preferred design:

1. Library media update finishes for a Plex library.
2. A comparison service recalculates comparison results for affected remote libraries against owned libraries of the same media family.
3. Store completed comparison scopes per remote library and owned target library.
4. Store sparse comparison hit rows for matched media per remote-owned library pair, including same-quality and higher-quality hits.
5. Treat missing as an anti-join result only inside a completed, current comparison scope.
6. Browse/detail endpoints project comparison fields from indexed comparison queries into normal media queries.

Benefits:

- Browsing stays fast when backed by indexed comparison tables.
- Higher-quality actions can show exactly which owned media item is upgradeable.
- Missing media can avoid huge “missing row for every absent item” tables while still distinguishing “not compared yet” from “compared and absent”.
- Cache invalidation is clearer: comparison rows update when library media, ownership, access, or algorithm version changes.

### Sparse comparison table design

Use one comparison scope table plus one comparison hit table per media type, starting with `PlexMovieComparison` for movies.

A scope row records that a remote library has been compared against an owned library. Without this scope, absence of hit rows means “not compared yet”, not “missing”.

Suggested scope fields:

| Field | Purpose |
|---|---|
| `RemotePlexLibraryId` | Remote library source |
| `OwnedPlexLibraryId` | Owned library target |
| `MediaType` | Movie, show, season, episode, later music |
| `CompletedAt` | Comparison completed timestamp |
| `AlgorithmVersion` | Match logic version used |
| `RemoteLibraryUpdatedAt` | Remote snapshot used |
| `OwnedLibraryUpdatedAt` | Owned snapshot used |

Candidate hit tables:

| Media type | Table name | Purpose |
|---|---|---|
| Movie | `PlexMovieComparison` | One row per remote movie to owned-library hit |
| TV show | `PlexTvShowComparison` | One row per remote show to owned-library hit/aggregate |
| Season | `PlexSeasonComparison` | One row per remote season to owned-library hit/aggregate |
| Episode | `PlexEpisodeComparison` | One row per remote episode to owned-library hit |

Row semantics:

- A row represents a **hit** between a remote media item and an owned library/media target.
- If five owned libraries contain a matching media item, there are five rows.
- Higher-quality is stored on the row when the remote item can upgrade that owned target.
- Missing is **not** inferred from row absence alone. It is only valid when a current comparison scope exists for the remote library and owned target library, the remote item is in that scope, and no hit row exists for that target.
- For TV aggregation, show/season rows can be derived from episode comparison rows or materialized if query performance requires it.

Suggested row fields:

| Field | Purpose |
|---|---|
| `RemotePlexMediaId` | Remote media item being compared |
| `OwnedPlexLibraryId` | Owned library target |
| `OwnedPlexMediaId` | Matched owned media item |
| `ComparisonState` | Matched / HigherQuality |
| `RemoteQuality` | Quality available remotely |
| `OwnedQuality` | Quality available locally |
| `MatchType` | Waterfall layer enum: TMDB, IMDB, TVDB, TitleYear, TitleYearDuration, ParentStructure, etc. |
| `ComparedAt` | When the row was calculated |
| `AlgorithmVersion` | Allows recalculation after matching logic changes |

Missing-row brainstorm:

- **Sparse-by-default:** store comparison scopes and hit rows only. Missing is computed by anti-joining scoped remote media against hit rows. This avoids huge tables.
- **Explicit missing rows:** easier filters and counts, but can explode in size for large remote libraries.
- **Hybrid:** sparse hit rows plus optional materialized aggregate counts for top-level display if anti-join queries are too slow.

Current preference: sparse-by-default with a comparison scope table, indexed hit rows, and materialized aggregates only if measurements show they are needed.

### Invalidation triggers

Recalculate comparison when:

- a remote library refresh finishes,
- an owned library refresh finishes for the same media family,
- a Plex server changes from owned to non-owned or non-owned to owned,
- Plex access is lost to a previously owned server,
- Plex access is restored to a server,
- a Plex account is added, removed, or relinked,
- a Plex server or library is added, removed, disabled, or re-enabled,
- a Plex library media type changes or is remapped,
- a library is marked unavailable/outdated,
- the comparison algorithm version changes,
- sync detects changed GUID/title/year/quality fields.

Plex/Reaparr media identity fields should usually only change as part of a library refresh or metadata refresh. PlexAPI exposes `updatedAt`, and Reaparr stores `UpdatedAt`, so treat detected metadata changes as a refresh-driven recalculation trigger rather than a separate user action.

### Query behavior

Browse queries should be able to ask for:

- all media with comparison summary,
- only missing items,
- only higher-quality items,
- missing or higher-quality items,
- TV shows with missing seasons/episodes,
- TV detail season/episode comparison rows.

Filtering must use backend queries over stored/indexed comparison data.

## Owned vs remote display rules

The row’s Plex server/library ownership determines how comparison is displayed; no extra “owned media is comparable” marker is needed.

Remote rows:

- show missing indicators only after a current comparison scope exists and no owned target hit exists,
- show higher-quality indicators when remote quality is better than one or more owned matches,
- keep the normal download/select flow.

Owned rows:

- show upgrade indicators/actions when a remote match has higher quality,
- show fill-in actions for missing child media, e.g. missing episodes in an owned show,
- do not compare the row against itself.

## Frontend display guidance

Use existing Reaparr icon language as much as possible. Current frontend media icons are defined in `src/AppHost/ClientApp/src/types/class/Convert.ts` via `Convert.mediaTypeToIcon`:

| Media type | Existing icon |
|---|---|
| TV show | `mdi-television-classic` |
| Season | `mdi-play-box-multiple` |
| Episode | `mdi-movie-open` |
| Movie | `mdi-filmstrip` |
| Music / artist / album / song | `mdi-music` |
| Photos / photo album | `mdi-image` |

Missing and higher-quality need new Material Design Icons from the same `mdi-*` family. The exact icons can be decided later.

Suggested compact labels:

| Situation | Compact display | Tooltip/detail text |
|---|---|---|
| Top-level missing | media icon + `Missing` | `Missing from owned libraries` |
| Top-level higher quality | media icon + quality-up icon | `Higher quality available` |
| Mid-level missing | mid-level icon + count | `Full seasons/discs missing` |
| Leaf-level missing | leaf icon + count | `Episodes/tracks missing` |
| Leaf-level higher quality | leaf icon + quality-up count | `Episodes/tracks available in higher quality` |

Color intent:

- Missing/new content: success/green.
- Higher-quality upgrade: warning/yellow.
- Avoid red; missing content is an opportunity, not an error.

## Research summary

### Existing Reaparr data model

Useful data already exists:

- `BasePlexMedia.Guid_IMDB`
- `BasePlexMedia.Guid_TMDB`
- `BasePlexMedia.Guid_TVDB`
- `BasePlexMedia.Quality`
- `PlexTvShowSeason.SeasonNumber`
- `PlexTvShowEpisode.EpisodeNumber`
- `PlexServer.Owned` / `PlexAccountLibrary.IsLibraryOwned`

Relevant inheritance:

- `PlexMovie : BasePlexMedia`
- `PlexTvShow : BasePlexMedia`
- `PlexTvShowSeason : BasePlexMedia`
- `PlexTvShowEpisode : BasePlexMedia`

Existing identifiers and quality fields should be reused where possible.

### Existing backend flow

Current media overview flow:

```text
GetAllMediaByTypeEndpoint
  -> MediaQueryCache.GetMediaAsync
    -> GetMediaByTypeCommandHandler.ExecuteAsync
      -> PlexMovies / PlexTvShows queries
      -> ToSlimDTO / ToSlimDTOMapper
      -> PlexMediaSlimDTO[]
```

Important files:

- `src/Application/PlexMedia/GetAll/GetAllMediaByTypeEndpoint.cs`
- `src/Data/Cache/MediaQueryCache.cs`
- `src/Data/Queries/GetMediaByTypeCommandHandler.cs`
- `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaSlimDTO.cs`
- `src/Application/_Shared/Mappers/PlexMedia/PlexMediaDTOMapper.*.cs`

### Existing frontend targets

- Poster card: `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/MediaPoster.vue`
- Poster image on TV detail page: `src/AppHost/ClientApp/src/components/MediaOverview/PosterTable/MediaPosterImage.vue`
- Table view: `src/AppHost/ClientApp/src/components/MediaOverview/MediaTable/MediaQTable.vue`
- TV detail seasons/episodes: `src/AppHost/ClientApp/src/components/MediaOverview/MediaList.vue`
- TV detail page: `src/AppHost/ClientApp/src/pages/tvshows/[libraryId]/details/[tvShowId].vue`
- Store: `src/AppHost/ClientApp/src/store/mediaOverviewStore.ts`

Current poster layout has:

- hover overlay for title/actions,
- bottom sort overlay,
- bottom quality bar.

Top-left remains the least conflicting poster location for compact indicators.

## Recommendations

1. **Keep sparse hit rows, but add comparison scopes.**  
   A sparse hit row table stores only positive comparison results: “this remote media item matched this owned library/media item”, plus whether the remote copy is higher quality. It does **not** store one row for every missing item. That keeps tables small, but row absence is ambiguous unless the system also knows that a comparison actually ran.

   A comparison scope row solves that ambiguity. The scope says: “remote library X was compared against owned library Y for media type Z, using algorithm version N, against these library snapshots, and the comparison completed.” Inside that completed scope, a missing item can be derived by anti-joining remote media against hit rows. Outside a completed/current scope, no hit row means only “not compared yet” or “comparison stale”, not “missing”.

   Example:

   | Data | Meaning |
   |---|---|
   | Scope exists for Remote Movies A -> Owned Movies B, and no `PlexMovieComparison` hit for remote movie `123` | Movie `123` is missing from owned library B |
   | No scope exists for Remote Movies A -> Owned Movies B | Missing cannot be inferred yet |
   | Scope exists, hit row exists, remote quality > owned quality | Movie is owned but upgradeable |
   | Scope exists, hit row exists, remote quality <= owned quality | Movie is already owned at same-or-better quality |

2. **Start with movie comparison first.**  
   Movies are top-level only, so they prove matching, quality comparison, filters, and upgrade actions before TV hierarchy adds aggregation complexity.

3. **Treat TV as episode-first internally.**  
   Episode rows are the real leaf truth. Show and season counts can be derived or materialized after performance is measured.

4. **Store `MatchType` from day one.**  
   It is cheap, helps debugging false matches, and keeps the path open for a later “hide lower-confidence matches” feature.

5. **Measure before materializing aggregates.**  
   Add aggregate tables only if indexed anti-join/query projection is too slow on realistic large libraries.

## Next steps

1. Confirm the sparse scope + hit-row model.
2. Design the first `PlexMovieComparison` schema and indexes.
3. Define the comparison recalculation job boundary and invalidation inputs.
4. Implement movie matching first: GUID waterfall, exact title/year fallback, quality comparison.
5. Add backend filters for missing, higher quality, and missing-or-higher-quality.
6. Project comparison buckets into `PlexMediaSlimDTO` / `PlexMediaDTO`.
7. Add compact movie indicators and owned-library upgrade action.
8. Extend the same model to TV episodes, then derive season/show summaries.

## Implementation design notes

This section should be finalized after the brainstormed behavior and storage shape are accepted.

### API contract direction

Do not add a separate comparison DTO wrapper. Add comparison fields directly to `PlexMediaSlimDTO` and `PlexMediaDTO` so browse and detail screens can read the same shape without extra nesting.

Use three media buckets:

```csharp
public record PlexMediaSlimDTO
{
    // existing fields...
    public PlexMediaComparisonState ComparisonState { get; init; }
    public PlexMediaComparisonBucketDTO? Missing { get; init; }
    public PlexMediaComparisonBucketDTO? HigherQuality { get; init; }
}

public record PlexMediaDTO
{
    // existing fields...
    public PlexMediaComparisonState ComparisonState { get; init; }
    public PlexMediaComparisonBucketDTO? Missing { get; init; }
    public PlexMediaComparisonBucketDTO? HigherQuality { get; init; }
}

public record PlexMediaComparisonBucketDTO
{
    public int TopLevel { get; init; } // movie, tv show, album
    public int MidLevel { get; init; } // season, disc
    public int LeafLevel { get; init; } // episode, track
}
```

### Match type enum direction

```csharp
public enum PlexMediaComparisonMatchType
{
    None = 0,
    TmdbGuid = 1,
    ImdbGuid = 2,
    TvdbGuid = 3,
    NormalizedTitleAndYear = 4,
    NormalizedTitleYearAndDuration = 5,
    ParentAndChildNumbers = 6,
}
```

## Risk assessment

| Risk | Impact | Mitigation |
|---|---|---|
| Comparison calculation is expensive | Slow library update or browsing | Calculate after library updates and store sparse indexed hits |
| Missing filters are slow with sparse rows | Slow anti-join queries | Add targeted indexes first; materialize aggregate counts only if measured |
| Stored comparison goes stale | Wrong indicators after sync | Recalculate on owned/remote media updates, ownership/access changes, and algorithm changes |
| Title/year fallback creates false matches | Missing content hidden incorrectly | Use GUIDs first, keep fallback conservative, store match type/confidence |
| GUID-only matching misses valid items | False missing indicators | Use waterfall fallback after GUIDs |
| TV aggregation is confusing | Users cannot tell what is missing | Separate full mid-level counts from loose leaf-level counts |
| Badge clutter | Posters become noisy | Icons first, tooltip for detail, table/detail pages for fuller text |
| Future music support forces redesign | More DTO churn later | Use universal hierarchy buckets now |

## Rejected alternatives

### Frontend-only comparison

Rejected because large libraries would require loading owned and remote media into the browser, duplicating data transfer and breaking virtual-scroll assumptions.

### Single status enum only

Rejected because top-level and mid-level media can simultaneously have missing children and higher-quality children. A headline enum is useful for filtering, but counts and pair rows remain the source of truth.

### Dedicated comparison page

Rejected because existing browse pages already contain selection/download UX. Inline indicators fit the “see what I’m missing while browsing” and “upgrade what I own” use cases.

### Glowing border

Rejected because Reaparr already uses glow/hover effects and bottom overlays; border status risks visual conflict.
