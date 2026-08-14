using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Reaparr.Environment;

namespace Reaparr.Data.Contracts;

public static class DbContextConnections
{
    /// <summary>
    /// Singleton interceptor that registers the NATURALSORT collation on each connection.
    /// </summary>
    private static readonly NaturalSortCollationInterceptor _collationInterceptor = new();

    public static string GetConnectionString(IPathProvider pathProvider) =>
        new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            DataSource = pathProvider.DatabasePath,
            Pooling = true,
        }.ToString();

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

        optionsBuilder.AddInterceptors(_collationInterceptor);

        optionsBuilder.UseSqlite(
            GetConnectionString(pathProvider),
            options =>
            {
                options.CommandTimeout(120);
            }
        );

        var sqliteOptionsBuilder = new SqliteDbContextOptionsBuilder(optionsBuilder);
        sqliteOptionsBuilder.MigrationsAssembly(contextType.Assembly.FullName);

        // Use split queries for multiple collection includes to avoid cartesian explosion.
        // This resolves: "Compiling a query which loads related collections for more than one
        // collection navigation... no 'QuerySplittingBehavior' has been configured"
        // See: https://go.microsoft.com/fwlink/?linkid=2134277
        sqliteOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }
}
