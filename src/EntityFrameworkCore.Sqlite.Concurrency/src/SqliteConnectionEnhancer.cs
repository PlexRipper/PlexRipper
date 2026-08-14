using System.Collections.Concurrent;
using System.Data.Common;
using EntityFrameworkCore.Sqlite.Concurrency.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Sqlite.Concurrency;

/// <summary>
/// Provides utility methods for enhancing SQLite connections with optimized settings.
/// </summary>
public static class SqliteConnectionEnhancer
{
    // Cache optimized connection strings to avoid repeated parsing
    private static readonly ConcurrentDictionary<string, string> _connectionStringCache = new();

    // Channel-based write queues per connection string — one background writer loop per database file
    private static readonly ConcurrentDictionary<string, SqliteWriteQueue> _writeQueues = new();

    // Kept for backward compatibility; no longer used internally.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _writeLocks = new();

#if !NETSTANDARD2_0

    // Shared interceptors per connection string to avoid leaking background tasks
    private static readonly ConcurrentDictionary<string, SqliteConcurrencyInterceptor> _interceptors = new();
#endif

    /// <summary>
    /// Tracks if the current execution flow already holds a write lock to prevent deadlocks.
    /// </summary>
    public static readonly AsyncLocal<bool> IsWriteLockHeld = new();

    private static readonly ConcurrentDictionary<string, object> _pragmaLocks = new();

    private static readonly ConcurrentDictionary<string, bool> _initializedDatabases = new();

    // Tracks databases that have had pre-open file preparation (ReadOnly clearing, stale shm
    // deletion) applied. Keyed by the resolved absolute path so that relative and absolute
    // forms of the same path map to the same entry.
    private static readonly ConcurrentDictionary<string, bool> _preparedDatabases = new();

    /// <summary>
    /// Gets an optimized version of the provided connection string.
    /// </summary>
    /// <param name="originalConnectionString">The original connection string.</param>
    /// <returns>An optimized connection string.</returns>
    public static string GetOptimizedConnectionString(string originalConnectionString)
    {
        return _connectionStringCache.GetOrAdd(originalConnectionString, ComputeOptimizedConnectionString);
    }

    /// <summary>
    /// Gets the shared write queue for the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="capacity">
    /// Optional bounded capacity. When set, callers that enqueue while the channel is full
    /// will wait asynchronously (back-pressure). Default is unbounded.
    /// </param>
    /// <returns>A <see cref="SqliteWriteQueue"/> that serializes writes for this database.</returns>
    internal static SqliteWriteQueue GetWriteQueue(string connectionString, int? capacity = null)
    {
        return _writeQueues.GetOrAdd(connectionString, _ => new SqliteWriteQueue(capacity));
    }

    /// <summary>
    /// Gets a shared write lock for the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>A semaphore used for write synchronization.</returns>
    /// <remarks>
    /// Preserved for backward compatibility. Internal code now uses
    /// <see cref="GetWriteQueue"/> for write serialization.
    /// </remarks>
    [Obsolete("Use GetWriteQueue for write serialization. This method is preserved for backward compatibility.")]
    public static SemaphoreSlim GetWriteLock(string connectionString)
    {
        return _writeLocks.GetOrAdd(connectionString, _ => new SemaphoreSlim(1, 1));
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Returns the cached interceptor for a connection string, or <see langword="null"/> if none has been registered.
    /// </summary>
    internal static SqliteConcurrencyInterceptor? TryGetInterceptor(string connectionString)
    {
        _interceptors.TryGetValue(connectionString, out var interceptor);
        return interceptor;
    }

    /// <summary>
    /// Gets or creates a concurrency interceptor for the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="options">The concurrency options.</param>
    /// <returns>A <see cref="SqliteConcurrencyInterceptor"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided <paramref name="options"/> do not match the options of an
    /// existing interceptor for the same <paramref name="connectionString"/>.
    /// </exception>
    /// <remarks>
    /// Callers must use consistent options for the same connection string, as interceptors
    /// are cached and shared. <see cref="SqliteConcurrencyOptions.LoggerFactory"/> is
    /// excluded from this check.
    /// </remarks>
    public static SqliteConcurrencyInterceptor GetInterceptor(string connectionString, SqliteConcurrencyOptions options)
    {
        if (_interceptors.TryGetValue(connectionString, out var existingInterceptor))
        {
            if (!existingInterceptor.Options.Equals(options))
            {
                throw new ArgumentException(
                    $"Mismatched SqliteConcurrencyOptions for connection string. "
                        + $"Existing options: {FormatOptions(existingInterceptor.Options)}, "
                        + $"Incoming options: {FormatOptions(options)}. "
                        + $"Interceptors are shared per connection string and must be configured consistently.",
                    nameof(options)
                );
            }

            return existingInterceptor;
        }

        return _interceptors.GetOrAdd(connectionString, cs => new SqliteConcurrencyInterceptor(options, cs));
    }

    private static string FormatOptions(SqliteConcurrencyOptions options)
    {
        return $"[MaxRetryAttempts={options.MaxRetryAttempts}, "
            + $"BusyTimeout={options.BusyTimeout}, "
            + $"CommandTimeout={options.CommandTimeout}, "
            + $"WalAutoCheckpoint={options.WalAutoCheckpoint}, "
            + $"SynchronousMode={options.SynchronousMode}, "
            + $"UpgradeTransactionsToImmediate={options.UpgradeTransactionsToImmediate}]";
    }
#endif

    private static string ComputeOptimizedConnectionString(string originalConnectionString)
    {
        var builder = new SqliteConnectionStringBuilder(originalConnectionString);

        // Cache=Shared uses a single shared page cache across connections, which conflicts
        // with WAL mode's snapshot isolation model. In WAL mode each connection must track
        // its own read snapshot independently; shared-cache connections share internal
        // pager state in a way that can produce inconsistent snapshot visibility and
        // violates WAL's reader/writer non-blocking guarantee.
        // Connection pooling (Pooling=true, set below) achieves efficient reuse without
        // this incompatibility. See https://www.sqlite.org/wal.html for details.
        if (builder.Cache == SqliteCacheMode.Shared)
            throw new ArgumentException(
                "Cache=Shared is incompatible with WAL mode and cannot be used with "
                    + "ThreadSafeEFCore.SQLite. Remove 'Cache=Shared' from your connection string. "
                    + "Connection pooling (Pooling=true) is enabled automatically and provides "
                    + "efficient connection reuse without the WAL incompatibility.",
                nameof(originalConnectionString)
            );

        builder.Pooling = true;
        builder.ForeignKeys = true;
        builder.RecursiveTriggers = true;
        builder.Mode = SqliteOpenMode.ReadWriteCreate;

        return builder.ToString();
    }

    /// <summary>
    /// Prepares the database file for opening: clears the read-only file attribute and removes
    /// any stale <c>.db-shm</c> file. Must be called <em>before</em> the connection is opened
    /// so that SQLite sees a writable file and does not fall back to a read-only pager.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On Windows, MSBuild sometimes stamps the database file with <see cref="FileAttributes.ReadOnly"/>
    /// when it is included in the project as <c>Content</c> with <c>CopyToOutputDirectory</c>.
    /// SQLite's <c>sqlite3_open_v2</c> attempts <c>O_RDWR</c>; if the OS returns
    /// <c>EACCES</c>, it silently falls back to <c>O_RDONLY</c>, which permanently marks the
    /// pager as read-only. Any subsequent write attempt — including
    /// <c>PRAGMA journal_mode = WAL</c> — then returns <c>SQLITE_READONLY</c>. Clearing the
    /// attribute before the call to <c>Open()</c> prevents this fallback.
    /// </para>
    /// <para>
    /// The <c>.db-shm</c> file is SQLite's shared-memory hash table that indexes WAL frames.
    /// It contains no committed data and is always recreated on the next WAL open. A stale
    /// copy left by a previous crashed process can cause <c>SQLITE_READONLY_CANTINIT</c>
    /// because SQLite detects a header mismatch.
    /// </para>
    /// <para>
    /// This method runs at most once per database file per process (subsequent calls are
    /// no-ops). It is safe to call from multiple threads; an internal lock serializes the
    /// first call.
    /// </para>
    /// </remarks>
    /// <param name="connection">The connection that is about to be opened.</param>
    public static void PrepareForConnectionOpen(DbConnection connection)
    {
        if (connection is not SqliteConnection sqliteConnection)
            return;

        var builder = new SqliteConnectionStringBuilder(sqliteConnection.ConnectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrEmpty(dataSource))
            return;

        // Resolve to an absolute path using the same working directory that SQLite will use,
        // so that relative forms (e.g. "Data Source=app.db") and absolute forms map to the
        // same key. Path.GetFullPath is a no-op when the path is already absolute.
        var dbFullPath = Path.GetFullPath(dataSource);

        if (_preparedDatabases.ContainsKey(dbFullPath))
            return;

        var lockObj = _pragmaLocks.GetOrAdd(dbFullPath, _ => new object());
        lock (lockObj)
        {
            if (_preparedDatabases.ContainsKey(dbFullPath))
                return;

            // Clear the read-only attribute from the database file so SQLite can open
            // it in read-write mode. This must happen before Open() is called.
            TryClearReadOnly(dbFullPath);

            // Clear the read-only attribute from the WAL file so that WAL recovery can
            // complete. If the WAL file is read-only, SQLite falls back to a read-only
            // pager during recovery, which then blocks PRAGMA journal_mode = WAL.
            TryClearReadOnly(dbFullPath + "-wal");

            // Delete the stale shm file. If another process currently has the database
            // open the file will be locked on Windows and File.Delete will throw — we
            // leave it untouched in that case and let SQLite sort it out.
            var shmPath = dbFullPath + "-shm";
            if (File.Exists(shmPath))
            {
                try
                {
                    File.Delete(shmPath);
                }
                catch
                {
                    /* locked by live process — leave it alone */
                }
            }

            _preparedDatabases.TryAdd(dbFullPath, true);
        }
    }

    private static void TryClearReadOnly(string path)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var attrs = File.GetAttributes(path);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(path, attrs & ~FileAttributes.ReadOnly);
        }
        catch
        {
            /* best-effort */
        }
    }

    /// <summary>
    /// Applies runtime PRAGMAs to the specified connection using default options.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    public static void ApplyRuntimePragmas(DbConnection connection)
    {
        ApplyRuntimePragmas(connection, new SqliteConcurrencyOptions());
    }

    /// <summary>
    /// Applies runtime PRAGMAs to the specified connection using the provided options.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="options">The concurrency options.</param>
    public static void ApplyRuntimePragmas(DbConnection connection, SqliteConcurrencyOptions options)
    {
        if (connection is not SqliteConnection sqliteConnection)
            return;

        var builder = new SqliteConnectionStringBuilder(sqliteConnection.ConnectionString);
        var dataSource = builder.DataSource;

        // Resolve to an absolute path so that relative forms ("Data Source=app.db") and
        // absolute forms of the same file map to the same _initializedDatabases key.
        var dbKey = string.IsNullOrEmpty(dataSource) ? dataSource : Path.GetFullPath(dataSource);

        // 1. Database-scoped PRAGMAs — executed once per process per database file.
        //    These settings are persistent (stored in the database header) and affect all
        //    connections to the same file.
        if (!_initializedDatabases.ContainsKey(dbKey))
        {
            var lockObj = _pragmaLocks.GetOrAdd(dbKey, _ => new object());
            lock (lockObj)
            {
                if (!_initializedDatabases.ContainsKey(dbKey))
                {
                    var logger = options.LoggerFactory?.CreateLogger(nameof(SqliteConnectionEnhancer));

                    // WAL mode is initialized first and in isolation so that a SQLITE_READONLY
                    // failure is logged as a warning rather than crashing the entire interceptor.
                    // PrepareForConnectionOpen (called from ConnectionOpening before sqlite3_open_v2)
                    // handles the common root causes proactively. If WAL still fails here, the
                    // database falls back to the default journal mode — concurrent read/write
                    // performance is reduced but the database remains functional.
                    try
                    {
                        using var walCommand = sqliteConnection.CreateCommand();
                        walCommand.CommandText = "PRAGMA journal_mode = WAL;";
                        walCommand.ExecuteNonQuery();
                    }
                    catch (SqliteException ex) when (ex.SqliteErrorCode == 8) // SQLITE_READONLY
                    {
                        logger?.LogWarning(
                            ex,
                            "Could not enable WAL mode for '{DataSource}' "
                                + "(SQLITE_READONLY, extended code: {Extended}). "
                                + "The database will use the default journal mode — concurrent read/write "
                                + "performance will be reduced. To resolve: ensure the database directory "
                                + "is writable and delete any stale .db-shm / .db-wal files alongside the "
                                + "database, then restart the application.",
                            dataSource,
                            ex.SqliteExtendedErrorCode
                        );
                    }

                    try
                    {
                        using var initCommand = sqliteConnection.CreateCommand();
                        initCommand.CommandText =
                            $@"
                            -- 4 096 bytes aligns with modern OS page sizes (ext4, NTFS, APFS) and is the
                            -- SQLite recommended default. Changing page_size after data exists has no effect
                            -- without a VACUUM, so this is a no-op on pre-existing databases.
                            PRAGMA page_size = 4096;

                            -- INCREMENTAL auto-vacuum reclaims free pages on demand (PRAGMA incremental_vacuum)
                            -- without the heavy full-database rewrite that FULL auto-vacuum performs on every
                            -- commit. NONE means free pages are never returned to the OS.
                            PRAGMA auto_vacuum = INCREMENTAL;

                            -- Caps the on-disk size of the rollback journal / WAL after a checkpoint or commit.
                            -- 128 MB is a reasonable upper bound; without this the WAL can grow unbounded when
                            -- long-running readers prevent checkpoint completion.
                            PRAGMA journal_size_limit = 134217728;

                            -- Trigger an automatic passive checkpoint after this many WAL frames are written.
                            -- 1 000 frames × 4 096 bytes ≈ 4 MB. Smaller values keep the WAL compact (faster
                            -- reads) at the cost of more checkpoint I/O. Set to 0 to disable auto-checkpoint.
                            PRAGMA wal_autocheckpoint = {options.WalAutoCheckpoint};
                        ";
                        initCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(
                            ex,
                            "One or more database-initialization PRAGMAs failed for '{DataSource}'. "
                                + "The connection-scoped PRAGMAs (busy_timeout, cache_size, etc.) will still be applied.",
                            dataSource
                        );
                    }

                    // Mark as initialized regardless of partial failures above so that the
                    // failed PRAGMAs are not retried on every subsequent connection open.
                    _initializedDatabases.TryAdd(dbKey, true);
                }
            }
        }

        // 2. Connection-scoped PRAGMAs — applied on every connection open.
        //    These are per-connection settings that are not stored in the database file.
        using var command = sqliteConnection.CreateCommand();
        command.CommandText =
            $@"
            -- How long (ms) this connection will spin waiting for a lock before returning
            -- SQLITE_BUSY. This is the first layer of busy handling; the library adds a
            -- second layer (application-level retry with jitter) because SQLite bypasses
            -- this handler when it detects a potential deadlock.
            PRAGMA busy_timeout = {(int)options.BusyTimeout.TotalMilliseconds};

            -- Memory-mapped I/O size (256 MB). Allows the OS virtual-memory subsystem to
            -- serve reads directly from the mapped region, bypassing read() syscalls for
            -- hot pages. Adjust down on memory-constrained hosts.
            PRAGMA mmap_size = 268435456;

            -- Store internal temporary tables and indices in RAM instead of a temp file.
            -- Eliminates temp-file I/O for sort and aggregation operations.
            PRAGMA temp_store = MEMORY;

            -- Negative value = kibibytes. -20 000 ≈ 20 MB page cache per connection.
            -- Each connection maintains its own cache; size accordingly for your process.
            PRAGMA cache_size = -20000;

            -- Durability vs. performance trade-off. See SqliteSynchronousMode for full
            -- documentation. NORMAL is the recommended setting for WAL mode: the database
            -- is always consistent after an application crash; a power loss or OS crash
            -- may roll back the last one or two commits that had not yet been checkpointed.
            PRAGMA synchronous = {options.SynchronousMode.ToString().ToUpperInvariant()};

            -- NORMAL (default): connections release file locks between transactions,
            -- allowing other processes to access the database. EXCLUSIVE holds locks
            -- permanently and can improve single-process throughput but prevents any
            -- other process from opening the file.
            PRAGMA locking_mode = NORMAL;

            -- OFF: deleted content is overwritten with zeros on VACUUM only, not on every
            -- DELETE. Improves write performance. Enable if the database stores sensitive
            -- data that must not be recoverable from free pages after deletion.
            PRAGMA secure_delete = OFF;
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs a passive WAL checkpoint and returns its status, which indicates whether
    /// the WAL is growing and whether long-running readers are blocking reclamation.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the target database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A <see cref="WalCheckpointStatus"/> describing the current WAL state.
    /// Returns a zeroed status when the database is not in WAL mode.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A <em>passive</em> checkpoint transfers already-committed WAL frames to the main
    /// database file without blocking readers or writers. It is the safest checkpoint
    /// mode for health monitoring.
    /// </para>
    /// <para>
    /// Call this periodically (e.g., every few minutes) to detect WAL growth pressure.
    /// A persistently <see cref="WalCheckpointStatus.IsBusy"/> result combined with a
    /// large <see cref="WalCheckpointStatus.TotalWalFrames"/> means long-running read
    /// transactions are preventing WAL reclamation and will eventually degrade read
    /// performance as readers must scan an ever-larger WAL on every page lookup.
    /// </para>
    /// </remarks>
    public static async Task<WalCheckpointStatus> GetWalCheckpointStatusAsync(
        DbConnection connection,
        CancellationToken cancellationToken = default
    )
    {
        if (connection is not SqliteConnection)
            return new WalCheckpointStatus(false, 0, 0);

        using var command = connection.CreateCommand();

        // PRAGMA wal_checkpoint(PASSIVE) returns a single row: (busy, log, checkpointed)
        // busy        — 1 if blocked by an active reader, 0 otherwise
        // log         — total WAL frames
        // checkpointed — frames successfully written back to the main DB
        command.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new WalCheckpointStatus(false, 0, 0);

        var busy = reader.GetInt32(0) != 0;
        var totalFrames = reader.GetInt32(1);
        var checkpointed = reader.GetInt32(2);

        return new WalCheckpointStatus(busy, totalFrames, checkpointed);
    }

    /// <summary>
    /// Checks for a stale EF Core migration lock and optionally removes it.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the target database.</param>
    /// <param name="release">
    /// When <see langword="true"/> (the default), deletes the stale lock row so that
    /// EF Core can proceed with migrations. When <see langword="false"/>, only checks
    /// for the lock without modifying the database.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if a stale migration lock was found (and released when
    /// <paramref name="release"/> is <see langword="true"/>); <see langword="false"/>
    /// if the <c>__EFMigrationsLock</c> table does not exist or contains no rows.
    /// </returns>
    /// <remarks>
    /// <para>
    /// EF Core serializes concurrent migrations using a <c>__EFMigrationsLock</c> table.
    /// If a migration process crashes or is killed after acquiring the lock but before
    /// releasing it, subsequent migration attempts will block indefinitely waiting for
    /// a lock that will never be freed.
    /// </para>
    /// <para>
    /// This is especially relevant in multi-instance deployments where every app instance
    /// calls <c>Database.Migrate()</c> at startup. The safest strategy is to run
    /// migrations as a single, controlled step (e.g. an init container or a deployment
    /// script) rather than from every instance concurrently. When a stale lock does
    /// occur, call this method once with <paramref name="release"/> set to
    /// <see langword="true"/> before retrying the migration.
    /// </para>
    /// <example>
    /// <code>
    /// // At startup, before calling Database.Migrate():
    /// var wasStale = await SqliteConnectionEnhancer.TryReleaseMigrationLockAsync(connection);
    /// if (wasStale)
    ///     logger.LogWarning("Stale EF migration lock detected and released.");
    /// await db.Database.MigrateAsync();
    /// </code>
    /// </example>
    /// </remarks>
    public static async Task<bool> TryReleaseMigrationLockAsync(
        DbConnection connection,
        bool release = true,
        CancellationToken cancellationToken = default
    )
    {
        if (connection is not SqliteConnection)
            return false;

        // Check whether the migrations lock table exists at all.
        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsLock';";
        var tableCount = (long)(await tableCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
        if (tableCount == 0)
            return false;

        // Check whether a lock row is present.
        using var lockCmd = connection.CreateCommand();
        lockCmd.CommandText = "SELECT COUNT(*) FROM __EFMigrationsLock;";
        var lockCount = (long)(await lockCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
        if (lockCount == 0)
            return false;

        // A stale lock exists — remove it if requested.
        if (release)
        {
            using var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM __EFMigrationsLock;";
            await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return true;
    }
}
