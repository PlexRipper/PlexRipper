using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NaturalSort.Extension;
using Reaparr.Environment;

namespace Reaparr.Data.Contracts;

public static class DbContextConnections
{
    private const int BUSY_TIMEOUT_SECONDS = 120;
    private static readonly OnConnectionOpenInterceptor _connectionOpenInterceptor = new();

    public static string GetConnectionString(string dataSource, SqliteOpenMode mode) =>
        new SqliteConnectionStringBuilder
        {
            Mode = mode,
            ForeignKeys = true,
            DataSource = dataSource,
            Pooling = true,
            DefaultTimeout = BUSY_TIMEOUT_SECONDS,
        }.ToString();

    public static void ConfigureSqlite(
        this DbContextOptionsBuilder optionsBuilder,
        string dataSource,
        SqliteOpenMode mode
    )
    {
        optionsBuilder.AddInterceptors(_connectionOpenInterceptor);
        optionsBuilder.UseSqlite(
            GetConnectionString(dataSource, mode),
            options => options.CommandTimeout(BUSY_TIMEOUT_SECONDS + 5) // command handling does not expire before SQLite’s BUSY_TIMEOUT_SECONDS.
        );
    }

    public static void DefaultConfiguration(
        this DbContextOptionsBuilder optionsBuilder,
        IPathProvider pathProvider,
        IAppRuntimeInfo appRuntimeInfo,
        Type contextType
    )
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        if (appRuntimeInfo.IsDevelopmentEnvironment || appRuntimeInfo.IsIntegrationTestMode)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
        }

        optionsBuilder.ConfigureSqlite(pathProvider.DatabasePath, SqliteOpenMode.ReadWriteCreate);

        var sqliteOptionsBuilder = new SqliteDbContextOptionsBuilder(optionsBuilder);
        sqliteOptionsBuilder.MigrationsAssembly(contextType.Assembly.FullName);

        // Use split queries for multiple collection includes to avoid cartesian explosion.
        // This resolves: "Compiling a query which loads related collections for more than one
        // collection navigation... no 'QuerySplittingBehavior' has been configured"
        // See: https://go.microsoft.com/fwlink/?linkid=2134277
        sqliteOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }

    private sealed class OnConnectionOpenInterceptor : DbConnectionInterceptor
    {
        private static readonly NaturalSortComparer _comparer = new(StringComparison.OrdinalIgnoreCase);

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            ConfigureConnection(connection);
            base.ConnectionOpened(connection, eventData);
        }

        public override async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default
        )
        {
            ConfigureConnection(connection);
            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        private static void ConfigureConnection(DbConnection connection)
        {
            if (connection is not SqliteConnection sqliteConnection)
                return;

            sqliteConnection.CreateCollation(OrderByNaturalExtensions.CollationName, (x, y) => _comparer.Compare(x, y));

            using var command = sqliteConnection.CreateCommand();
            command.CommandText = $"PRAGMA busy_timeout = {BUSY_TIMEOUT_SECONDS * 1000};";
            command.ExecuteNonQuery();
        }
    }
}
