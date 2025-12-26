using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Reaparr.Environment;

namespace Reaparr.Data.Contracts;

public static class DbContextConnections
{
    public static readonly string ConnectionString = new SqliteConnectionStringBuilder
    {
        // Mixing shared-cache mode and write-ahead logging is discouraged. For optimal performance, remove Cache=Shared when the database is configured to use write-ahead logging.
        // https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings#basic
        Cache = SqliteCacheMode.Default,
        Mode = SqliteOpenMode.ReadWriteCreate,
        DataSource = PathProvider.DatabasePath,
        Pooling = true,
        DefaultTimeout = 60,
    }.ToString();

    /// <summary>
    /// Singleton interceptor that registers the NATURALSORT collation on each connection.
    /// </summary>
    private static readonly NaturalSortCollationInterceptor _collationInterceptor = new();

    public static void DefaultConfiguration(this DbContextOptionsBuilder optionsBuilder, Type contextType)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.AddInterceptors(_collationInterceptor);

        optionsBuilder.UseSqlite(
            ConnectionString,
            b =>
            {
                b.MigrationsAssembly(contextType.Assembly.FullName);
            }
        );
    }
}
