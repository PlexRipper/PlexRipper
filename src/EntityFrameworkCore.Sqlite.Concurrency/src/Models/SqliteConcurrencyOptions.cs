using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Sqlite.Concurrency.Models;

/// <summary>
/// Options for configuring SQLite concurrency and performance.
/// </summary>
public class SqliteConcurrencyOptions : IEquatable<SqliteConcurrencyOptions>
{
    /// <summary>
    /// The maximum number of retry attempts when an operation fails with
    /// <c>SQLITE_BUSY</c> or <c>SQLITE_BUSY_SNAPSHOT</c>.
    /// </summary>
    /// <remarks>
    /// Each retry uses exponential backoff with jitter starting at 100 ms.
    /// Must be greater than zero. Default is <c>3</c>.
    /// </remarks>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Per-connection busy timeout passed to <c>PRAGMA busy_timeout</c>.
    /// SQLite will automatically retry lock acquisition for up to this duration
    /// before returning <c>SQLITE_BUSY</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the <em>first</em> layer of busy handling. The library also adds
    /// application-level retry with jitter (the second layer) because SQLite may
    /// bypass the busy handler to avoid deadlocks — for example, when a read
    /// transaction attempts to upgrade to a write transaction.
    /// </para>
    /// <para>
    /// Must be non-negative. A value of <see cref="TimeSpan.Zero"/> disables the
    /// built-in handler and relies entirely on the application-level retry.
    /// Default is 30 seconds.
    /// </para>
    /// </remarks>
    public TimeSpan BusyTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// EF Core command timeout (seconds) applied via
    /// <c>SqliteDbContextOptionsBuilder.CommandTimeout</c>.
    /// </summary>
    /// <remarks>
    /// Must be non-negative. Default is <c>300</c> (5 minutes).
    /// </remarks>
    public int CommandTimeout { get; set; } = 300;

    /// <summary>
    /// Number of WAL frames after which SQLite automatically runs a passive
    /// checkpoint (<c>PRAGMA wal_autocheckpoint</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A lower value keeps the WAL file small (reducing read overhead) but adds more
    /// checkpoint I/O. A higher value reduces checkpoint frequency at the cost of a
    /// larger WAL file and potential read slowdown as readers must scan more frames.
    /// </para>
    /// <para>
    /// Must be non-negative. Set to <c>0</c> to disable automatic checkpointing
    /// (manual checkpoints only via <c>PRAGMA wal_checkpoint</c>). Default is
    /// <c>1000</c> pages (~4 MB at the default 4 096-byte page size).
    /// </para>
    /// </remarks>
    public int WalAutoCheckpoint { get; set; } = 1000;

    /// <summary>
    /// Controls how aggressively SQLite flushes commits to stable storage
    /// (<c>PRAGMA synchronous</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Default: <see cref="SqliteSynchronousMode.Normal"/></b> — recommended for WAL
    /// mode. The database is always consistent after an application crash. A power
    /// failure or OS crash may roll back the last committed transaction(s) that had not
    /// yet been checkpointed.
    /// </para>
    /// <para>
    /// Use <see cref="SqliteSynchronousMode.Full"/> or
    /// <see cref="SqliteSynchronousMode.Extra"/> when power-loss durability is critical.
    /// Be aware that these settings significantly increase write latency due to additional
    /// <c>fsync</c> calls on every commit.
    /// </para>
    /// </remarks>
    public SqliteSynchronousMode SynchronousMode { get; set; } = SqliteSynchronousMode.Normal;

    /// <summary>
    /// When <see langword="true"/> (the default), the interceptor rewrites plain
    /// <c>BEGIN</c> and <c>BEGIN TRANSACTION</c> commands to <c>BEGIN IMMEDIATE</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BEGIN IMMEDIATE</c> acquires a reserved write lock at transaction start,
    /// guaranteeing that no later statement in the transaction will fail with
    /// <c>SQLITE_BUSY</c> before commit. This eliminates the common failure mode where
    /// a deferred transaction (plain <c>BEGIN</c>) successfully reads data but then
    /// fails with <c>SQLITE_BUSY_SNAPSHOT</c> on the first write because another
    /// writer committed in the meantime.
    /// </para>
    /// <para>
    /// <b>Trade-off:</b> Upgrading read-only transactions to <c>IMMEDIATE</c> acquires
    /// an unnecessary write lock, which can reduce concurrency slightly when many
    /// read-only operations run simultaneously. If your application exclusively uses
    /// explicit read-only transactions and manages write transactions manually, set this
    /// to <see langword="false"/> and start write transactions with
    /// <c>BEGIN IMMEDIATE</c> yourself.
    /// </para>
    /// <para>
    /// Commands that already contain <c>IMMEDIATE</c> or <c>EXCLUSIVE</c> are never
    /// modified.
    /// </para>
    /// </remarks>
    public bool UpgradeTransactionsToImmediate { get; set; } = true;

    /// <summary>
    /// Bounded capacity of the per-database write queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, the write queue uses a bounded <see cref="System.Threading.Channels.Channel{T}"/>
    /// with <c>BoundedChannelFullMode.Wait</c>. Callers that enqueue writes while the channel is
    /// at capacity will wait asynchronously until space is available, providing explicit
    /// back-pressure under heavy write load.
    /// </para>
    /// <para>
    /// When <see langword="null"/> (the default), the channel is unbounded and callers are
    /// never delayed by back-pressure. The underlying queue still serializes writes in FIFO
    /// order; only the maximum depth is unrestricted.
    /// </para>
    /// <para>
    /// This value is used only when the write queue is first created for a given database.
    /// Changing it after first use has no effect.
    /// </para>
    /// </remarks>
    public int? WriteQueueCapacity { get; set; }

    /// <summary>
    /// Optional logger factory used to create the interceptor's
    /// <see cref="ILogger"/>. Not included in equality comparison.
    /// </summary>
    /// <remarks>
    /// When provided, the library logs <c>SQLITE_BUSY*</c> events (Warning),
    /// checkpoint-blocked detections (Warning), and <c>BEGIN IMMEDIATE</c> upgrades
    /// (Debug). In a DI scenario, prefer
    /// <see cref="ExtensionMethods.SqliteConcurrencyServiceCollectionExtensions.AddConcurrentSqliteDbContext{TContext}(Microsoft.Extensions.DependencyInjection.IServiceCollection, string, System.Action{SqliteConcurrencyOptions}?, Microsoft.Extensions.DependencyInjection.ServiceLifetime)"/>
    /// which resolves the factory automatically.
    /// </remarks>
    public ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Validates that all option values are within acceptable ranges.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any option value is outside its valid range.
    /// </exception>
    public void Validate()
    {
        if (MaxRetryAttempts <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(MaxRetryAttempts),
                MaxRetryAttempts,
                "MaxRetryAttempts must be greater than zero."
            );

        if (BusyTimeout < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(BusyTimeout),
                BusyTimeout,
                "BusyTimeout must be non-negative."
            );

        if (CommandTimeout < 0)
            throw new ArgumentOutOfRangeException(
                nameof(CommandTimeout),
                CommandTimeout,
                "CommandTimeout must be non-negative."
            );

        if (WalAutoCheckpoint < 0)
            throw new ArgumentOutOfRangeException(
                nameof(WalAutoCheckpoint),
                WalAutoCheckpoint,
                "WalAutoCheckpoint must be non-negative (0 disables auto-checkpoint)."
            );
    }

    /// <inheritdoc />
    public bool Equals(SqliteConcurrencyOptions? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return MaxRetryAttempts == other.MaxRetryAttempts
            && BusyTimeout.Equals(other.BusyTimeout)
            && CommandTimeout == other.CommandTimeout
            && WalAutoCheckpoint == other.WalAutoCheckpoint
            && SynchronousMode == other.SynchronousMode
            && UpgradeTransactionsToImmediate == other.UpgradeTransactionsToImmediate
            && WriteQueueCapacity == other.WriteQueueCapacity;
        // LoggerFactory is intentionally excluded from equality: it is infrastructure
        // metadata and does not affect SQLite behaviour.
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((SqliteConcurrencyOptions)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            MaxRetryAttempts,
            BusyTimeout,
            CommandTimeout,
            WalAutoCheckpoint,
            SynchronousMode,
            UpgradeTransactionsToImmediate,
            WriteQueueCapacity
        );
}
