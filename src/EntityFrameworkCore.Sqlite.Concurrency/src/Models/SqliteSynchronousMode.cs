namespace EntityFrameworkCore.Sqlite.Concurrency.Models;

/// <summary>
/// Controls how aggressively SQLite flushes write operations to stable storage
/// (<c>PRAGMA synchronous</c>).
/// </summary>
/// <remarks>
/// <para>
/// The right choice depends on your durability requirements and workload:
/// </para>
/// <list type="bullet">
///   <item>
///     For WAL mode (the default), <see cref="Normal"/> is almost always correct.
///     Commits are durable against application crashes; the small risk is losing the
///     last transaction or two after an unexpected power loss or OS crash.
///   </item>
///   <item>
///     For rollback journal mode, <see cref="Full"/> or <see cref="Extra"/> provides
///     stronger guarantees at the cost of more <c>fsync</c> calls per commit.
///   </item>
///   <item>
///     <see cref="Off"/> trades all durability for maximum speed and is only
///     appropriate for ephemeral or fully-reproducible databases.
///   </item>
/// </list>
/// <para>
/// See the <see href="https://www.sqlite.org/pragma.html#pragma_synchronous">SQLite
/// documentation</see> for the complete specification.
/// </para>
/// </remarks>
public enum SqliteSynchronousMode
{
    /// <summary>
    /// <c>PRAGMA synchronous = OFF</c> — SQLite hands off writes to the OS and
    /// continues without waiting for any flush. Maximum performance.
    /// <para>
    /// <b>Risk:</b> A power failure or OS crash at any point can corrupt the database.
    /// Not recommended for production data.
    /// </para>
    /// </summary>
    Off = 0,

    /// <summary>
    /// <c>PRAGMA synchronous = NORMAL</c> — SQLite syncs at WAL checkpoint boundaries
    /// rather than on every commit.
    /// <para>
    /// The database is always consistent after an application crash. A power failure or
    /// OS crash may roll back the last one or two transactions that had been committed
    /// by the application but not yet checkpointed.
    /// </para>
    /// <para>
    /// This is the recommended setting for WAL mode and the library default. It
    /// delivers significantly higher write throughput than <see cref="Full"/> while
    /// providing acceptable durability for most workloads.
    /// </para>
    /// </summary>
    Normal = 1,

    /// <summary>
    /// <c>PRAGMA synchronous = FULL</c> — SQLite issues an <c>fsync</c> (or platform
    /// equivalent) on every commit, ensuring committed data survives a power failure
    /// or OS crash.
    /// <para>
    /// Significantly slower than <see cref="Normal"/> for write-heavy workloads because
    /// every commit blocks until the OS confirms the data is on stable storage.
    /// </para>
    /// </summary>
    Full = 2,

    /// <summary>
    /// <c>PRAGMA synchronous = EXTRA</c> — Like <see cref="Full"/>, but also syncs
    /// the directory containing the database file after deleting or truncating the
    /// rollback journal (DELETE/TRUNCATE mode). Provides the strongest durability
    /// guarantee against filesystems that do not persist directory entries on crash.
    /// <para>
    /// Has no additional benefit beyond <see cref="Full"/> in WAL mode.
    /// </para>
    /// </summary>
    Extra = 3
}
