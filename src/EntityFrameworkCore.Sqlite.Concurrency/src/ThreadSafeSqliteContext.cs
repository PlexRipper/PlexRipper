using EntityFrameworkCore.Sqlite.Concurrency.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

namespace EntityFrameworkCore.Sqlite.Concurrency;

/// <summary>
/// A thread-safe SQLite context that provides application-level serialization for writes.
/// </summary>
/// <typeparam name="TContext">The type of the actual DbContext.</typeparam>
public class ThreadSafeSqliteContext<TContext> : DbContext where TContext : DbContext
{
    private readonly string? _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadSafeSqliteContext{TContext}"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    public ThreadSafeSqliteContext(string connectionString)
    {
        _connectionString = SqliteConnectionEnhancer.GetOptimizedConnectionString(connectionString);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadSafeSqliteContext{TContext}"/> class using options.
    /// </summary>
    /// <param name="options">The options.</param>
    public ThreadSafeSqliteContext(DbContextOptions options) : base(options)
    {
#pragma warning disable EF1001
        var extension = options.FindExtension<SqliteOptionsExtension>();
#pragma warning restore EF1001
        if (extension?.ConnectionString != null)
            _connectionString = SqliteConnectionEnhancer.GetOptimizedConnectionString(extension.ConnectionString);
        else if (extension?.Connection != null)
            _connectionString = SqliteConnectionEnhancer.GetOptimizedConnectionString(extension.Connection.ConnectionString);
    }

    private SqliteWriteQueue WriteQueue
    {
        get
        {
            var cs = _connectionString ?? Database.GetDbConnection().ConnectionString;
            return SqliteConnectionEnhancer.GetWriteQueue(cs, Options.WriteQueueCapacity);
        }
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _connectionString != null)
        {
            optionsBuilder.UseSqliteWithConcurrency(_connectionString);
        }
    }

    /// <summary>
    /// Executes a write operation with app-level serialization and automatic transaction management.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// <para>
    /// Two classes of <c>SQLITE_BUSY</c> are handled:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <b>SQLITE_BUSY / SQLITE_BUSY_RECOVERY / SQLITE_BUSY_TIMEOUT</b> — the
    ///     transaction is rolled back and retried after exponential backoff with jitter.
    ///   </item>
    ///   <item>
    ///     <b>SQLITE_BUSY_SNAPSHOT</b> — the read snapshot became stale. The transaction
    ///     is rolled back and the entire operation lambda is restarted so it can re-query
    ///     any data that may now be stale.
    ///   </item>
    /// </list>
    /// <para>
    ///   <b>SQLITE_LOCKED</b> (same-connection conflict) propagates immediately and is
    ///   not retried, as it indicates an application-level bug.
    /// </para>
    /// </remarks>
    public async Task<T> ExecuteWriteAsync<T>(
        Func<TContext, Task<T>> operation,
        CancellationToken ct = default)
    {
        var maxRetryAttempts = Options.MaxRetryAttempts;

        return await WriteQueue.EnqueueAsync(async () =>
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    // The interceptor will upgrade this BEGIN to BEGIN IMMEDIATE, ensuring
                    // no later statement in the transaction fails with SQLITE_BUSY before
                    // commit (as long as UpgradeTransactionsToImmediate is true).
                    await using var transaction = await Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable, ct);

                    var result = await operation((TContext)(object)this);
                    await SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return result;
                }
                catch (SqliteException ex) when (SqliteErrorCodes.IsAnyBusy(ex))
                {
                    attempt++;
                    if (attempt >= maxRetryAttempts)
                    {
                        var kind = SqliteErrorCodes.IsBusySnapshot(ex)
                            ? "SQLITE_BUSY_SNAPSHOT (stale read snapshot — another writer committed after this transaction began)"
                            : $"SQLITE_BUSY (extended code {ex.SqliteExtendedErrorCode})";

                        throw new TimeoutException(
                            $"SQLite database busy after {attempt} retry attempt(s). " +
                            $"Error: {kind}. " +
                            $"Consider increasing MaxRetryAttempts or BusyTimeout.",
                            ex);
                    }

                    // Exponential backoff with full jitter: sleep in [baseDelay, 2×baseDelay].
                    var baseDelay = 100 * Math.Pow(2, attempt);
                    var jitter    = Random.Shared.NextDouble() * baseDelay;
                    await Task.Delay(TimeSpan.FromMilliseconds(baseDelay + jitter), ct);
                }
            }
        }, ct);
    }

    /// <summary>
    /// Executes a write operation with app-level serialization and automatic transaction management.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task ExecuteWriteAsync(
        Func<TContext, Task> operation,
        CancellationToken ct = default)
    {
        await ExecuteWriteAsync(async ctx =>
        {
            await operation(ctx);
            return true;
        }, ct);
    }

    /// <summary>
    /// Executes a read operation without locking.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// WAL mode allows reads to proceed concurrently with writes. Keep read transactions
    /// short to avoid blocking WAL checkpoint completion, which can cause the WAL file to
    /// grow and degrade read performance over time.
    /// </remarks>
    public async Task<T> ExecuteReadAsync<T>(
        Func<TContext, Task<T>> operation,
        CancellationToken ct = default)
    {
        return await operation((TContext)(object)this);
    }

    /// <summary>
    /// Performs a bulk insert with optimized settings and app-level locking.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="ct">The cancellation token.</param>
    public async Task BulkInsertSafeAsync<T>(
        IList<T> entities,
        CancellationToken ct = default) where T : class
    {
        await ExecuteWriteAsync(async ctx =>
        {
            foreach (var batch in entities.Chunk(1000))
            {
                await ctx.AddRangeAsync(batch, ct);
                await ctx.SaveChangesAsync(ct);
                ctx.ChangeTracker.Clear();
            }
        }, ct);
    }


    private SqliteConcurrencyOptions? _options;

    private SqliteConcurrencyOptions Options
    {
        get
        {
            if (_options != null) return _options;

            // Read the options configured via UseSqliteWithConcurrency so that
            // MaxRetryAttempts, WriteQueueCapacity, etc. reflect the user's settings.
            if (_connectionString != null)
            {
                var interceptor = SqliteConnectionEnhancer.TryGetInterceptor(_connectionString);
                if (interceptor != null)
                {
                    _options = interceptor.Options;
                    return _options;
                }
            }

            _options = new SqliteConcurrencyOptions();
            return _options;
        }
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
