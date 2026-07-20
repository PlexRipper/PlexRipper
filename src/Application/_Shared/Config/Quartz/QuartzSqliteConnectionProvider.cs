using System.Data;
using System.Data.Common;
using EntityFrameworkCore.Sqlite.Concurrency;
using Microsoft.Data.Sqlite;
using Quartz.Impl.AdoJobStore.Common;

namespace Reaparr.Application;

/// <summary>
/// Custom Quartz <see cref="IDbProvider"/> that applies the same SQLite PRAGMAs
/// (<c>busy_timeout</c>, <c>journal_mode=WAL</c>, <c>synchronous=NORMAL</c>, etc.)
/// as the EF Core pipeline via <see cref="SqliteConnectionEnhancer.ApplyRuntimePragmas"/>.
///
/// Quartz's <c>AdoJobStore</c> opens raw ADO.NET connections that bypass the EF Core
/// interceptor pipeline. Without this provider, Quartz connections get <c>busy_timeout=0</c>
/// (the SQLite default), which causes instant <c>SQLITE_BUSY</c> ("database is locked")
/// whenever an EF Core write transaction holds the lock — most commonly observed during
/// misfire recovery.
/// </summary>
public sealed class QuartzSqliteConnectionProvider : IDbProvider
{
    private const string MetadataProductName = "Quartz-SQLite-Custom";
    private const string MetadataAssemblyName = "Microsoft.Data.Sqlite";

    private readonly DbProviderFactory _factory;
    private string _connectionString = "";

    /// <summary>
    /// Parameterless constructor required by Quartz's reflection-based instantiation
    /// via <c>ObjectUtils.InstantiateType</c>.
    /// </summary>
    public QuartzSqliteConnectionProvider()
    {
        _factory = SqliteFactory.Instance;
    }

    /// <inheritdoc />
    public void Initialize() { }

    /// <inheritdoc />
    public void Shutdown() { }

    /// <inheritdoc />
    public string ConnectionString
    {
        get => _connectionString;
        set => _connectionString = value;
    }

    /// <inheritdoc />
    public DbMetadata Metadata => new()
    {
        ProductName = MetadataProductName,
        AssemblyName = MetadataAssemblyName,
    };

    /// <inheritdoc />
    public DbCommand CreateCommand()
    {
        var cmd = _factory.CreateCommand()
            ?? throw new InvalidOperationException("SqliteFactory returned null command");

        return cmd;
    }

    /// <inheritdoc />
    public DbConnection CreateConnection()
    {
        var conn = _factory.CreateConnection()
            ?? throw new InvalidOperationException("SqliteFactory returned null connection");

        conn.ConnectionString = _connectionString;

        // Quartz calls CreateConnection() then Open(). We hook StateChange
        // to apply PRAGMAs on every Open() — this is the same pattern as
        // EF Core's ConnectionOpened interceptor, but for raw ADO.NET.
        conn.StateChange += (_, args) =>
        {
            if (args.CurrentState == ConnectionState.Open)
                SqliteConnectionEnhancer.ApplyRuntimePragmas(conn);
        };

        return conn;
    }
}
