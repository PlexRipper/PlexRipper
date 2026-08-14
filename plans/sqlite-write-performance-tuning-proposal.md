# SQLite Write-Performance Tuning Proposal

## Goal

Improve SQLite write responsiveness and library-sync throughput for a multi-gigabyte Reaparr database, with an explicit focus on preventing `SQLITE_BUSY`/`SQLITE_LOCKED` errors and reducing write contention, while preserving concurrent background jobs and avoiding blind durability or schema trade-offs.

The primary user-visible objective is:

> Ordinary application writes should remain responsive while multiple TV and movie library-sync jobs are active.

Aggregate sync throughput is important, but it is secondary to preventing long pauses and starvation of settings, download-state, authentication, scheduler, and queue writes.

## Agreed constraints and priorities

1. **Do not broadly limit concurrent background jobs in the initial work.**
   - TV/movie library-sync concurrency remains unchanged for the initial database and connection phases.
   - Later, evidence may justify reducing, deferring, or separately throttling selected low-priority maintenance jobs.
   - Any such change must be narrow, independently configurable, and measured; it must not silently become a global job-concurrency cap.
   - Jobs may also be scheduled or phased differently.

2. **Start with database and connection configuration.**
   - Correct the WAL/checkpoint configuration before redesigning media synchronization.
   - Keep changes small and independently verifiable.

3. **The SQLite concurrency library must be removed.**
   - Its current unbounded process-wide write queue harms responsiveness and creates head-of-line blocking.
   - It must not be removed in isolation; transaction retry and timeout behavior must change with it.

4. **Use a 120-second busy timeout as the proposed ceiling.**
   - The agreed lower bound is at least 120 seconds.
   - This proposal recommends **exactly 120 seconds**, not an unbounded value and not 5,000 seconds.
   - A busy timeout is failure handling, not a performance optimization.

5. **Index removal is allowed when evidence shows low or duplicate value.**
   - Remove exact duplicate indexes first.
   - Audit broad media-index removal separately with real query plans and usage evidence.

6. **Incremental metadata synchronization is high priority but follows the database/configuration work.**
   - The existing destructive relation replacement causes unnecessary write amplification.
   - The already-started incremental library-sync direction should be extended to metadata.

7. **Do not add detailed runtime instrumentation in the initial configuration phase.**
   - Use existing phase timers, logs, database metadata, WAL/file behavior, and repeatable sync scenarios.
   - Add targeted telemetry only if the configuration and queue changes cannot be evaluated reliably.

## Current workload and database evidence

The inspected database is:

- Path: `/mnt/DATA/ReaparrCache/Config/ReaparrDB.db`
- Filesystem: local `ext4`
- Main database size: approximately **4.0 GiB**
- Observed WAL size: approximately **4.6 GiB**
- Page size: **4,096 bytes**
- Main database pages: approximately **1.024 million**
- Freelist pages at inspection time: **0**
- Auto-vacuum persisted mode: **NONE** (`0`)
- Access: one Reaparr process; no known external readers
- Available memory: moderate, approximately **4–16 GiB**

Largest write-sensitive tables:

| Table | Approximate rows | Approximate size |
|---|---:|---:|
| `PlexTvShowEpisodes` | 3,799,783 | 1,911 MiB |
| `PlexTvShowEpisodeData` | 3,854,414 | 680 MiB |
| `PlexMovie` | 424,793 | 223 MiB |
| `PlexMovieData` | 440,800 | 86 MiB |
| `PlexTvShowSeason` | 270,132 | 70 MiB |
| `PlexTvShows` | 118,730 | 61 MiB |

The large TV synchronization path currently:

- materializes existing shows, seasons, and episodes;
- constructs large in-memory dictionaries and lists;
- performs delete/update/insert work in a large serialized transaction;
- maintains multiple indexes for every changed row;
- then launches genre, country, and actor synchronization tasks concurrently;
- those metadata tasks ultimately compete for the same single SQLite writer.

Multiple library-sync jobs can overlap, so CPU/RAM preparation, long reads, and write transactions can overlap across jobs even though SQLite can commit only one writer at a time.

## Current EF Core and SQLite configuration

The connection configuration currently provides several good defaults:

- `Pooling = true`
- `Foreign Keys = true`
- WAL journal mode requested by the connection enhancer
- `synchronous = NORMAL`
- `temp_store = MEMORY`
- `mmap_size = 256 MiB`
- `cache_size = -20000`, approximately 20 MiB per connection
- `locking_mode = NORMAL`
- `secure_delete = OFF`
- default query tracking is disabled
- bulk operations are used for large media writes
- explicit write transactions are upgraded toward `BEGIN IMMEDIATE`

These settings already cover most of the safe recommendations in the referenced SQLite tuning article. The largest opportunities are not `mmap_size` or `temp_store`; they are checkpoint correctness, write admission/transaction shape, planner maintenance, and index/write amplification.

## Completed planner-statistics action

`ANALYZE` has been run successfully through Rider MCP.

Current statistics state:

- `sqlite_stat1`: **159 rows**
- `sqlite_stat4`: **2,899 samples**
- analyzed tables: **49**
- application tables: **59**
- all 10 application tables without statistics were empty at verification time

This gives the planner cardinality information for all populated tables and indexes. Examples include:

- `PlexTvShowEpisodes(PlexApiRatingKey, PlexServerId)` is effectively unique on the full key;
- `PlexTvShowEpisodes(PlexLibraryId)` averages roughly 58,000 rows per library;
- `PlexTvShowEpisodeData(Quality)` is low-selectivity, averaging hundreds of thousands of rows per value.

`ANALYZE` can improve read, delete, and update plan selection. It does not reduce the physical cost of maintaining indexes during inserts and deletes.

Future maintenance should use `PRAGMA optimize` rather than repeatedly running a full unconditional `ANALYZE` on startup.

Add a dedicated low-priority background job for planner maintenance. Its lifecycle is both startup-triggered and periodic, but not a once-per-startup write on every process launch:

- request `PRAGMA optimize=0x10002` once when the background job's long-lived maintenance connection starts, so a newly opened connection may consider every table;
- run ordinary `PRAGMA optimize` from the same non-overlapping background job approximately once per day while the application remains up;
- request an additional run after index/schema migrations and after unusually large data changes;
- persist or otherwise coordinate the last successful daily run so repeated restarts do not create a maintenance loop;
- execute it at low priority and treat a busy database as a reason to defer, not to block foreground writes.

This is a lightweight, bounded planner-maintenance job. A full unconditional `ANALYZE` is reserved for deliberate operator maintenance or evidence that `PRAGMA optimize` is insufficient.

## Confirmed configuration defects

### 1. WAL checkpoint settings have the wrong scope

The current connection setup classifies these as database-scoped and executes them only once per process/database:

```sql
PRAGMA journal_size_limit = 134217728;
PRAGMA wal_autocheckpoint = 1000;
```

That classification is incorrect for pooled multi-connection use.

- `wal_autocheckpoint` configures the connection's automatic checkpoint callback.
- `journal_size_limit` is maintained by the connection and affects journal/WAL truncation when that connection performs the relevant reset/checkpoint work.
- Setting either only on whichever connection opens first does not consistently configure later pooled connections.

The observed 4.6 GiB WAL is consistent with ineffective reset/truncation and/or internal long-running read snapshots preventing checkpoint completion. `journal_size_limit = 128 MiB` is not a hard cap on an active WAL; it can only shrink the retained WAL after SQLite can reset it.

There is no EF Core `UseSqlite(...)` fluent option for `wal_autocheckpoint` or `journal_size_limit`. The actionable correction is to execute both PRAGMAs whenever a physical SQLite connection opens rather than relying on one-time initialization.

### 2. Existing-database initialization comments overstate effects

The initialization block also runs:

```sql
PRAGMA page_size = 4096;
PRAGMA auto_vacuum = INCREMENTAL;
```

For the existing database:

- page size is already 4,096 bytes;
- persisted auto-vacuum remains `NONE`;
- changing auto-vacuum on an established database requires a deliberate rebuild/`VACUUM` workflow.

These commands should not be treated as proof that the existing database has incremental auto-vacuum enabled. Remove them from runtime initialization rather than repeatedly issuing ineffective SQL.

A future database-creation or rebuild path may deliberately select a page size before schema creation, but that belongs to schema/bootstrap policy rather than connection tuning. Auto-vacuum conversion is not proposed for this workload: the database is expected to refresh reusable pages during synchronization, conversion requires a blocking full-file `VACUUM`, auto-vacuum adds pointer-map and page-movement overhead, and it does not defragment the database. Reconsider only if measurements show sustained delete-driven file growth that cannot be reused and there is an approved backup, downtime, free-space, and rollback procedure.

### 3. The checkpoint diagnostic exists but is unused

The current implementation can run a passive checkpoint and report WAL/checkpoint progress, but no production caller uses that diagnostic.

A passive checkpoint does not solve an indefinitely held read snapshot, but it is the appropriate non-blocking primitive for routine maintenance and detecting checkpoint starvation.

The remedy for an indefinitely held internal read snapshot is to identify and shorten its lifetime, not to make the routine checkpoint more aggressive:

1. record checkpoint results (`busy`, total WAL frames, and checkpointed frames), WAL bytes, elapsed time, and active-operation context;
2. trigger a warning only after blocked frames or WAL growth persist across multiple samples, avoiding noise from one normal busy result;
3. correlate the warning with long-lived `DbContext`, enumerator/stream, and transaction lifetimes in library sync and background jobs;
4. materialize required read data before lengthy CPU/network work, dispose readers/contexts promptly, and use bounded keyset pages rather than streaming a query while writes continue;
5. retry `PASSIVE` after the reader ends;
6. use `RESTART` or `TRUNCATE` only at a proven quiescent startup/shutdown/idle point and with a bounded timeout.

External processes cannot be repaired by application refactoring, so persistent blockage must also report enough path/PID/operation evidence for operator diagnosis. Routine maintenance must never forcibly terminate readers.

## Position on background-job concurrency and scheduling

The initial connection, checkpoint, concurrency-library removal, and sync phases will not reduce TV/movie library-sync concurrency.

Scheduling changes remain valid because job concurrency and simultaneous write admission are separate controls. The scheduler may continue running multiple jobs while their phases are arranged to reduce pathological overlap.

Potential scheduling changes before any throttling:

- separate network-fetch/CPU reconciliation phases from database write phases;
- avoid starting every library's largest write phase at the same instant;
- let low-volume operational writes retain a chance to acquire the SQLite writer between bounded media-write transactions;
- run routine passive checkpoints outside the hottest commit burst;
- preserve current TickerQ declarations for primary library-sync jobs.

If the earlier phases still leave foreground writes starved, a later experiment may reduce, defer, or separately throttle named low-priority maintenance jobs. That experiment must have its own option, baseline, acceptance metric, and rollback; it must not become a process-wide limit or silently include primary library-sync jobs.

No scheduling or throttling change should be used to hide incorrect connection/checkpoint configuration.

## Index findings and proposal

### Exact duplicate indexes to remove first

Six explicit metadata relation indexes duplicate the exact key order already supplied by each table's composite primary-key auto-index:

| Explicit duplicate index | Duplicate primary-key index | Size |
|---|---|---:|
| `IX_PlexMovieActors_PlexActorId_PlexMovieId` | `sqlite_autoindex_PlexMovieActors_1` | 20.88 MiB |
| `IX_PlexMovieCountries_CountryId_PlexMovieId` | `sqlite_autoindex_PlexMovieCountries_1` | 7.88 MiB |
| `IX_PlexMovieGenres_GenresId_PlexMovieId` | `sqlite_autoindex_PlexMovieGenres_1` | 12.52 MiB |
| `IX_PlexTvShowActors_PlexActorId_PlexTvShowId` | `sqlite_autoindex_PlexTvShowActors_1` | 5.29 MiB |
| `IX_PlexTvShowCountries_CountryId_PlexTvShowId` | `sqlite_autoindex_PlexTvShowCountries_1` | 1.96 MiB |
| `IX_PlexTvShowGenres_GenresId_PlexTvShowId` | `sqlite_autoindex_PlexTvShowGenres_1` | 3.58 MiB |

Total redundant explicit B-tree storage is approximately **52.11 MiB**.

Removing these six explicit duplicates should:

- eliminate one redundant B-tree update per affected relation insert/delete;
- reduce WAL volume during destructive metadata replacement;
- preserve the same leading-column and full-key lookup capability through the primary-key index;
- preserve uniqueness through the primary key.

This is the safest first index migration, subject to generated migration review and query-plan verification.

### Media-index audit is separate

Do not broadly remove media indexes based only on low selectivity or file size.

Likely audit candidates include:

- standalone `Quality` indexes;
- standalone `SortIndex` indexes;
- standalone `PlexServerId` indexes where a composite index covers the actual query shape;
- overlapping `PlexLibraryId`, ordering, and quality combinations;
- indexes maintained on multi-million-row episode and episode-data tables.

Some apparently low-selectivity indexes support ordering, filtering, cascade/reconciliation deletes, or navigation queries. Each candidate needs:

1. mapped LINQ/SQL query consumers;
2. `EXPLAIN QUERY PLAN` before removal;
3. cardinality from `sqlite_stat1`/`sqlite_stat4`;
4. write-volume and storage cost;
5. a migration that can be rolled back;
6. representative read-latency verification.

The metadata duplicate-index migration can proceed before this broader audit.

## Final implementation plan

### Phase 0 — Remove the SQLite concurrency library while maintaining transaction safety

Remove `EntityFrameworkCore.Sqlite.Concurrency` as a standalone library, together with its project references and application registrations. Move only the required connection configuration and transaction-safety behavior into the appropriate Reaparr data-layer components.

The current library owns a process-global, unbounded FIFO queue per connection string. It serializes operations for their full duration, creates head-of-line blocking, and can retain an unlimited backlog of closures containing contexts, entities, and object graphs. Remove that design rather than replacing it with another managed write queue.

Implementation requirements:

- remove `SqliteWriteQueue`, its database-keyed queue registry, and every process-wide FIFO write path;
- remove or replace `ExecuteSerializedWriteAsync` and the library-specific extension APIs at every call site;
- do not retain a hidden compatibility queue or any replacement path that captures `DbContext`, entities, or operation closures in an unbounded backlog;
- preserve the connection-open mechanism needed to apply SQLite PRAGMAs on every physical connection;
- allow standalone `SaveChanges`, `ExecuteUpdate`, `ExecuteDelete`, and bulk operations to use SQLite WAL locking with bounded busy handling;
- retain `BEGIN IMMEDIATE` for known, short multi-command write transactions;
- retry an entire transaction delegate after retryable `SQLITE_BUSY`, `SQLITE_LOCKED`, or `SQLITE_BUSY_SNAPSHOT` failures, never an individual statement after partial work;
- set the busy timeout to exactly 120 seconds per transaction attempt;
- permit at most two whole-transaction attempts unless a shorter explicit total elapsed-time budget is implemented;
- apply jitter only between whole-transaction attempts and never add nested statement-level retry inside an open transaction;
- preserve cancellation as far as `Microsoft.Data.Sqlite` permits;
- log final busy/locked failures with operation category, wait duration, and attempt count;
- prevent interceptor recursion or replay that could duplicate side effects.

`BEGIN IMMEDIATE` asks for SQLite's write reservation when a known write transaction starts instead of beginning as a deferred reader and failing later during a read-to-write upgrade. WAL readers can continue while another writer waits under the bounded busy policy. Do not use it for read-only work or around slow network/CPU work before the first write; keep the transaction body short and free of unrelated awaits.

Unit-test verification must use real temporary SQLite database files and multiple independent connections/contexts. Add focused tests for:

- overlapping long and short write operations;
- cancellation while waiting for a write lock;
- complete rollback and whole-transaction retry after `SQLITE_BUSY_SNAPSHOT`;
- no duplicate inserts or partially replayed operations;
- no managed unbounded backlog;
- bounded timeout and retry behavior;
- preserved per-connection configuration after the library has been removed.

Keep this phase isolated from media-synchronization redesign so it can be reverted independently if lock behavior regresses.

### Phase 1 — Correct connection-scoped PRAGMAs

There is no native EF Core SQLite fluent API for `wal_autocheckpoint` or `journal_size_limit`. Preserve a Reaparr data-layer connection-open hook after Phase 0 and move all genuinely connection-scoped settings into its every-open block:

```sql
PRAGMA busy_timeout = 120000;
PRAGMA wal_autocheckpoint = 1000;
PRAGMA journal_size_limit = 134217728;
PRAGMA mmap_size = 268435456;
PRAGMA temp_store = MEMORY;
PRAGMA cache_size = -20000;
PRAGMA synchronous = NORMAL;
PRAGMA locking_mode = NORMAL;
PRAGMA secure_delete = OFF;
```

Notes:

- Keep WAL mode enabled as one-time/idempotent database setup; do not move `journal_mode` into the every-open block because changing it can lock and affect the database globally.
- Keep `synchronous = NORMAL`; this already captures the safe WAL write-latency gain from the tuning article.
- Keep the initial 256 MiB mmap and 20 MiB per-connection page cache. With moderate RAM and many contexts, increasing both blindly can worsen memory pressure.
- Configure `ReaparrDbContext` and `AuthDbContext` with the same connection string because both contexts target the same SQLite database file.
- Reclassify or remove misleading one-time initialization settings.
- Remove runtime `page_size` and `auto_vacuum` commands; do not add an auto-vacuum conversion migration in this phase.
- Keep provider-specific PRAGMA SQL in the Reaparr data layer rather than recreating the removed concurrency library.

Unit-test verification must use real temporary SQLite database files:

- open multiple independent `ReaparrDbContext` and `AuthDbContext` instances/connections against the same connection string;
- query each connection's PRAGMAs in unit tests;
- prove every physical connection receives the same busy timeout, auto-checkpoint threshold, journal-size limit, synchronous mode, cache, temp-store, and mmap values;
- prove WAL mode remains enabled for the shared database.

### Phase 2 — Add the SQLite maintenance background job

Add one non-overlapping, low-priority background job that owns planner-statistics and routine WAL maintenance. Use separate triggers and guards for the two operations so neither can overlap with itself or compete unnecessarily with foreground writes.

Planner-maintenance requirements:

- request `PRAGMA optimize=0x10002` when the job's long-lived maintenance connection starts, subject to a persisted last-success guard;
- run ordinary `PRAGMA optimize` approximately once per day;
- request an additional run after index/schema changes and unusually large data changes;
- defer when the database is busy rather than blocking foreground writes;
- record the last successful run, duration, and outcome;
- do not run a full unconditional `ANALYZE` on every application startup.

WAL-maintenance requirements:

- retain `wal_autocheckpoint = 1000` on every connection;
- periodically attempt `PRAGMA wal_checkpoint(PASSIVE)`;
- record `busy`, total WAL frames, checkpointed frames, WAL bytes, elapsed time, and active-operation context;
- warn only when blocked frames or WAL growth persist across multiple samples;
- do not run `FULL`, `RESTART`, or `TRUNCATE` checkpoints in the hot path;
- permit `RESTART` or `TRUNCATE` only at a proven quiescent startup, shutdown, or idle point with a bounded timeout.

If passive checkpoints remain blocked, correlate the samples with long-lived `DbContext`, enumerator/stream, and transaction lifetimes. Materialize required data before lengthy CPU/network work, dispose readers and contexts promptly, and use bounded keyset pages instead of streaming a query while writes continue. Retry `PASSIVE` after the reader ends; do not forcibly terminate readers.

Unit-test verification must use real temporary SQLite database files and verify:

- the background job cannot overlap with itself;
- the persisted planner-maintenance guard prevents repeated work after restarts;
- planner maintenance defers on contention;
- passive checkpoint results and persistent-blockage warnings are recorded correctly;
- routine maintenance never selects a blocking checkpoint mode;
- the WAL can reset after readers finish and the workload becomes quiescent.

### Phase 3 — Remove exact duplicate metadata indexes

Create and review an EF Core migration dropping the six explicit duplicate relation indexes listed above.

Verification checkpoint:

- inspect the generated migration;
- confirm primary-key auto-indexes remain;
- verify representative relation lookups still use the composite primary-key index;
- run metadata synchronization tests;
- request the maintenance service's next `PRAGMA optimize` run after applying the migration; do not execute competing optimize jobs directly from migration code;
- compare metadata relation write time and WAL growth.

### Phase 4 — Extend incremental synchronization to metadata

Replace destructive delete-and-reinsert metadata synchronization with set reconciliation for:

- movie actors, genres, and countries;
- TV-show actors, genres, and countries;
- library-level actors, genres, and countries where applicable.

For each relation set:

1. load only existing keys required for comparison;
2. compute additions and removals;
3. leave unchanged rows untouched;
4. delete only removed keys;
5. insert only added keys;
6. use bounded transaction units where all-or-nothing replacement is not required;
7. preserve cancellation and idempotency.

The sync data is reproducible and partial progress is acceptable, so large relation updates can be divided into bounded units to improve writer fairness. Each unit must still be internally consistent and safely repeatable.

Verification checkpoint:

- no-change metadata refresh produces approximately zero relation writes;
- small metadata change writes only the actual delta;
- interrupted sync can be restarted safely;
- overlapping syncs do not corrupt relation state;
- ordinary operational writes can complete between bounded media-write units.

### Phase 5 — Audit high-cost media indexes

Build an index-to-query matrix for all indexes on:

- `PlexTvShowEpisodes`;
- `PlexTvShowEpisodeData`;
- `PlexTvShowSeason`;
- `PlexTvShows`;
- `PlexMovie`;
- `PlexMovieData`.

Prioritize indexes that are:

- low-selectivity;
- prefixes of another useful composite index;
- absent from real query plans;
- large and maintained for every incremental sync;
- duplicative of a primary/unique index.

Remove indexes one migration at a time or in tightly related groups. Request a coordinated `PRAGMA optimize` run and perform representative query-plan checks after each migration.

## Acceptance criteria

The proposal is successful when all of the following are true:

1. **Configuration correctness**
   - every pooled SQLite connection receives the intended connection-scoped PRAGMAs;
   - WAL mode and `synchronous = NORMAL` remain enabled;
   - busy timeout is explicit and bounded;
   - planner statistics are maintained by one coordinated startup/daily `PRAGMA optimize` lifecycle.

2. **Responsiveness**
   - settings, download-state, scheduler, authentication, and other small writes no longer sit behind the removed library's unbounded managed write queue;
   - overlapping syncs do not produce repeated unexplained 30/120-second stalls;
   - cancellation and final lock failures remain bounded and visible.

3. **WAL behavior**
   - passive checkpoints make progress when internal readers finish;
   - the WAL can reset after the workload becomes quiescent;
   - retained WAL size honors the configured limit after a successful reset;
   - no blocking checkpoint mode runs routinely in the hot path.

4. **Write amplification**
   - six exact duplicate metadata indexes are gone while their primary-key indexes remain;
   - no-change metadata syncs stop deleting and recreating unchanged relations;
   - broader media indexes are removed only with query-plan evidence.

5. **Concurrency policy**
   - initial connection, checkpoint, concurrency-library removal, and sync changes do not reduce TV/movie library-sync concurrency;
   - any later throttling applies only to named low-priority maintenance job classes, is independently configurable, and is justified by measured foreground-write impact;
   - scheduling changes may alter phase timing without silently introducing a global job-concurrency limit.

6. **Safety**
   - no use of `synchronous = OFF`, `journal_mode = OFF/MEMORY`, `locking_mode = EXCLUSIVE`, or disabled foreign keys for production data;
   - no broad database rebuild or auto-vacuum conversion without a separate proposal and backup/rollback procedure.

## Explicit non-goals

- Migrating from SQLite to PostgreSQL or another server database.
- Broadly reducing TV/movie library-sync concurrency during the initial phases. Selective throttling of named low-priority maintenance jobs remains an evidence-driven later option.
- Turning durability off for benchmark numbers.
- Treating a huge busy timeout as a substitute for transaction design.
- Increasing mmap or page cache merely because more RAM exists.
- Running `VACUUM` or converting auto-vacuum mode during the initial phases.
- Removing media indexes without query-plan and usage evidence.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Removing the concurrency library's managed queue produces SQLite lock storms | For known write transactions, acquire the write reservation at transaction start with `BEGIN IMMEDIATE`; keep those transactions short, use bounded whole-transaction retry and an exact 120-second per-attempt ceiling, and retain an isolated rollback path. Read-only transactions remain deferred. |
| 120-second timeout delays cancellation | Limit transaction attempts and total elapsed budget; do not multiply the timeout by a large retry count. |
| Passive checkpoint cannot progress | Sample blocked frames repeatedly, correlate them with long-lived readers/contexts/transactions, shorten or materialize those reads, and retry `PASSIVE`; do not escalate immediately to blocking checkpoints. |
| WAL remains physically large after frames are checkpointed | Distinguish logical frames from retained file allocation; reset/truncate only at a controlled quiescent point. |
| Index removal regresses reads | Remove only exact duplicates first; use `EXPLAIN QUERY PLAN` and migration rollback for all other candidates. |
| Per-connection caches exhaust moderate RAM | Keep conservative 20 MiB cache and 256 MiB mmap initially; avoid shared cache and blind increases. |
| Multiple sync jobs still contend heavily | Use bounded write units and phase scheduling first; if evidence still shows foreground starvation, independently throttle only named low-priority maintenance jobs rather than applying a global concurrency cap. |
| Planner statistics become stale after large changes | Run lightweight `PRAGMA optimize` from one non-overlapping service at startup subject to its last-run guard, approximately daily, and after schema/index or unusually large data changes. |

## Source references

- Reaparr project instructions: `AGENTS.md`
- Reaparr backend conventions: `.agents/skills/reaparr-backend/SKILL.md`
- SQLite tuning article: <https://phiresky.github.io/blog/2020/sqlite-performance-tuning/>
- SQLite WAL documentation: <https://www.sqlite.org/wal.html>
- SQLite PRAGMA documentation: <https://www.sqlite.org/pragma.html>
- SQLite planner statistics and `PRAGMA optimize`: <https://www.sqlite.org/lang_analyze.html>
- Microsoft.Data.Sqlite locking, retry, and timeout guidance: <https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors>
- Referenced Reddit discussion: <https://old.reddit.com/r/sqlite/comments/1en6t4c/sqlite_is_the_goat/>

The Reddit advice reinforces WAL, transactions/batching, planner maintenance, one physical writer, and prepared/bulk operations. Unsafe suggestions such as `journal_mode = OFF`, `synchronous = OFF`, disabled foreign keys, and `locking_mode = EXCLUSIVE` are intentionally excluded from this production proposal.
