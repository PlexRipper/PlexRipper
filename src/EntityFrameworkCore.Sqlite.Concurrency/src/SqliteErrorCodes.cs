using Microsoft.Data.Sqlite;

namespace EntityFrameworkCore.Sqlite.Concurrency;

/// <summary>
/// SQLite primary and extended error code constants, with helpers for classifying exceptions.
/// </summary>
/// <remarks>
/// SQLite error codes follow the pattern: extended = primary | (reason &lt;&lt; 8).
/// <see cref="SqliteException.SqliteErrorCode"/> holds the primary code (low 8 bits).
/// <see cref="SqliteException.SqliteExtendedErrorCode"/> holds the full extended code.
/// </remarks>
internal static class SqliteErrorCodes
{
    // Primary error codes
    public const int Busy   = 5;  // SQLITE_BUSY
    public const int Locked = 6;  // SQLITE_LOCKED

    // Extended SQLITE_BUSY variants (5 | reason << 8)
    public const int BusyRecovery = 261;  // SQLITE_BUSY_RECOVERY   — WAL recovery in progress; safe to wait and retry
    public const int BusySnapshot = 517;  // SQLITE_BUSY_SNAPSHOT   — read snapshot too old; must rollback and restart
    public const int BusyTimeout  = 773;  // SQLITE_BUSY_TIMEOUT    — busy_timeout expired

    // Extended SQLITE_LOCKED variant (6 | reason << 8)
    public const int LockedSharedCache = 262; // SQLITE_LOCKED_SHAREDCACHE

    /// <summary>
    /// Returns <see langword="true"/> for SQLITE_BUSY variants that can be resolved by
    /// waiting and retrying the same operation — SQLITE_BUSY, SQLITE_BUSY_RECOVERY,
    /// and SQLITE_BUSY_TIMEOUT.
    /// </summary>
    /// <remarks>
    /// SQLITE_BUSY_SNAPSHOT is excluded. It indicates that the connection's read snapshot
    /// is older than the current database state, so the full transaction must be rolled
    /// back and restarted rather than simply waited out.
    /// </remarks>
    public static bool IsRetryableBusy(SqliteException ex)
        => ex.SqliteErrorCode == Busy
        && ex.SqliteExtendedErrorCode != BusySnapshot;

    /// <summary>
    /// Returns <see langword="true"/> when the exception is SQLITE_BUSY_SNAPSHOT,
    /// meaning the connection's read snapshot is stale after another writer committed.
    /// The transaction must be fully rolled back and restarted from scratch.
    /// </summary>
    public static bool IsBusySnapshot(SqliteException ex)
        => ex.SqliteExtendedErrorCode == BusySnapshot;

    /// <summary>
    /// Returns <see langword="true"/> for any SQLITE_BUSY variant (retryable or not).
    /// </summary>
    public static bool IsAnyBusy(SqliteException ex)
        => ex.SqliteErrorCode == Busy;

    /// <summary>
    /// Returns <see langword="true"/> for SQLITE_LOCKED, which indicates a conflict
    /// <em>within the same connection</em> (e.g., a write attempted while another
    /// statement on the same connection is still reading). This is an application-level
    /// bug and should <b>not</b> be retried.
    /// </summary>
    public static bool IsLocked(SqliteException ex)
        => ex.SqliteErrorCode == Locked;
}
