using System.Data.Common;
using EntityFrameworkCore.Sqlite.Concurrency.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Sqlite.Concurrency;

/// <summary>
/// Interceptor for SQLite that handles WAL mode, busy timeouts, and transaction upgrades.
/// </summary>
public class SqliteConcurrencyInterceptor : DbCommandInterceptor, IDbConnectionInterceptor, IDbTransactionInterceptor
{
    private readonly SqliteConcurrencyOptions _options;
    private readonly string _connectionString;
    private readonly ILogger<SqliteConcurrencyInterceptor>? _logger;

    /// <summary>
    /// Gets the concurrency options configured for this interceptor.
    /// </summary>
    public SqliteConcurrencyOptions Options => _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteConcurrencyInterceptor"/> class.
    /// </summary>
    /// <param name="options">The concurrency options.</param>
    /// <param name="connectionString">The connection string.</param>
    public SqliteConcurrencyInterceptor(SqliteConcurrencyOptions options, string connectionString)
    {
        _options = options;
        _connectionString = connectionString;
        // Ensure the write queue is created for this database with the configured capacity.
        SqliteConnectionEnhancer.GetWriteQueue(connectionString, options.WriteQueueCapacity);
        _logger = options.LoggerFactory?.CreateLogger<SqliteConcurrencyInterceptor>();
    }

    // --- Connection Management ---

    /// <inheritdoc />
    public void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SqliteConnectionEnhancer.ApplyRuntimePragmas(connection, _options);
    }

    /// <inheritdoc />
    public Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        SqliteConnectionEnhancer.ApplyRuntimePragmas(connection, _options);
        return Task.CompletedTask;
    }

    // --- Command Interception ---

    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result
    )
    {
        UpgradeToBeginImmediate(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default
    )
    {
        UpgradeToBeginImmediate(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpgradeToBeginImmediate(command);

        if (!ShouldSerializeNonQuery(command, eventData, result))
            return base.NonQueryExecuting(command, eventData, result);

        var rows = eventData
            .Context!.ExecuteSerializedWriteAsync(
                _ => Task.FromResult(command.ExecuteNonQuery()),
                _options.MaxRetryAttempts
            )
            .GetAwaiter()
            .GetResult();

        return InterceptionResult<int>.SuppressWithResult(rows);
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        UpgradeToBeginImmediate(command);

        if (!ShouldSerializeNonQuery(command, eventData, result))
            return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

        var rows = await eventData.Context!.ExecuteSerializedWriteAsync(
            command.ExecuteNonQueryAsync,
            _options.MaxRetryAttempts,
            cancellationToken
        );

        return InterceptionResult<int>.SuppressWithResult(rows);
    }

    /// <inheritdoc />
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result
    )
    {
        UpgradeToBeginImmediate(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default
    )
    {
        UpgradeToBeginImmediate(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    // --- Command Failure Logging ---

    /// <inheritdoc />
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        LogCommandFailure(eventData.Exception, command.CommandText);
        base.CommandFailed(command, eventData);
    }

    /// <inheritdoc />
    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        LogCommandFailure(eventData.Exception, command.CommandText);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private bool ShouldSerializeNonQuery(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        if (result.HasResult || SqliteConnectionEnhancer.IsWriteLockHeld.Value)
            return false;

        if (eventData.Context is null || command.Connection is not SqliteConnection)
            return false;

        // A caller-owned transaction can span multiple commands. Queueing only one of
        // them can deadlock if another queued writer blocks behind that open transaction.
        // Use ExecuteSerializedTransactionAsync to hold the queue for the whole unit.
        if (command.Transaction is not null)
            return false;

        return eventData.CommandSource
            is CommandSource.ExecuteDelete
                or CommandSource.ExecuteUpdate
                or CommandSource.ExecuteSqlRaw;
    }

    private void LogCommandFailure(Exception? exception, string commandText)
    {
        if (_logger is null || exception is not SqliteException sqlEx)
            return;

        if (SqliteErrorCodes.IsBusySnapshot(sqlEx))
        {
            _logger.LogWarning(
                sqlEx,
                "SQLITE_BUSY_SNAPSHOT on command [{Command}]: the connection's read snapshot is stale — "
                    + "another writer committed after this transaction began. The transaction will be rolled back "
                    + "and retried from scratch. Extended error code: {ExtendedCode}.",
                TruncateCommand(commandText),
                sqlEx.SqliteExtendedErrorCode
            );
        }
        else if (SqliteErrorCodes.IsRetryableBusy(sqlEx))
        {
            _logger.LogWarning(
                sqlEx,
                "SQLITE_BUSY on command [{Command}]: the database is locked by another connection. "
                    + "Will retry with backoff. Extended error code: {ExtendedCode}.",
                TruncateCommand(commandText),
                sqlEx.SqliteExtendedErrorCode
            );
        }
        else if (SqliteErrorCodes.IsLocked(sqlEx))
        {
            _logger.LogError(
                sqlEx,
                "SQLITE_LOCKED on command [{Command}]: conflict within the same connection. "
                    + "This typically indicates a statement is still open on the same connection while "
                    + "a write is attempted. Extended error code: {ExtendedCode}.",
                TruncateCommand(commandText),
                sqlEx.SqliteExtendedErrorCode
            );
        }
    }

    private void UpgradeToBeginImmediate(DbCommand command)
    {
        if (!_options.UpgradeTransactionsToImmediate)
            return;

        var text = command.CommandText.Trim();
        if (!text.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase))
            return;
        if (text.Contains("IMMEDIATE", StringComparison.OrdinalIgnoreCase))
            return;
        if (text.Contains("EXCLUSIVE", StringComparison.OrdinalIgnoreCase))
            return;

        if (
            text.Equals("BEGIN", StringComparison.OrdinalIgnoreCase)
            || text.Equals("BEGIN TRANSACTION", StringComparison.OrdinalIgnoreCase)
            || text.Equals("BEGIN DEFERRED", StringComparison.OrdinalIgnoreCase)
            || text.Equals("BEGIN DEFERRED TRANSACTION", StringComparison.OrdinalIgnoreCase)
        )
        {
            command.CommandText = "BEGIN IMMEDIATE";
            _logger?.LogDebug(
                "Upgraded [{Original}] to BEGIN IMMEDIATE to prevent SQLITE_BUSY_SNAPSHOT "
                    + "mid-transaction. Set UpgradeTransactionsToImmediate = false to disable.",
                text
            );
        }
    }

    private static string TruncateCommand(string commandText) =>
        commandText.Length <= 120 ? commandText : commandText[..120] + "…";

    // --- Transaction Interception ---

    /// <inheritdoc />
    public InterceptionResult<DbTransaction> TransactionStarting(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result
    )
    {
        return result;
    }

    /// <inheritdoc />
    public ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result,
        CancellationToken cancellationToken = default
    )
    {
        return new(result);
    }

    /// <inheritdoc />
    public DbTransaction TransactionStarted(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result
    ) => result;

    /// <inheritdoc />
    public ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public InterceptionResult TransactionCommitted(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public InterceptionResult TransactionRolledBack(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public InterceptionResult CreatingSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> CreatingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData) { }

    /// <inheritdoc />
    public Task CreatedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    /// <inheritdoc />
    public InterceptionResult RollingBackToSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> RollingBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData) { }

    /// <inheritdoc />
    public Task RolledBackToSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    /// <inheritdoc />
    public InterceptionResult ReleasingSavepoint(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> ReleasingSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    /// <inheritdoc />
    public void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData) { }

    /// <inheritdoc />
    public Task ReleasedSavepointAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        CancellationToken cancellationToken = default
    ) => Task.CompletedTask;

    /// <inheritdoc />
    public InterceptionResult TransactionExplictlyStarted(
        DbConnection connection,
        TransactionEndEventData eventData,
        InterceptionResult result
    ) => result;

    /// <inheritdoc />
    public ValueTask<InterceptionResult> TransactionExplictlyStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default
    ) => new(result);

    // --- Connection Management (IConnectionInterceptor) ---
    /// <inheritdoc />
    public void ConnectionOpening(DbConnection connection, ConnectionEventData eventData)
    {
        SqliteConnectionEnhancer.PrepareForConnectionOpen(connection);
    }

    /// <inheritdoc />
    public Task ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        SqliteConnectionEnhancer.PrepareForConnectionOpen(connection);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData) { }

    /// <inheritdoc />
    public Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData) => Task.CompletedTask;

    /// <inheritdoc />
    public void ConnectionClosing(DbConnection connection, ConnectionEventData eventData) { }

    /// <inheritdoc />
    public Task ConnectionClosingAsync(DbConnection connection, ConnectionEventData eventData) => Task.CompletedTask;

    /// <inheritdoc />
    public void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData) { }

    /// <inheritdoc />
    public Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData) =>
        Task.CompletedTask;
}
