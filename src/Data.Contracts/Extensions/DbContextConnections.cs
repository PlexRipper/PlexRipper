using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Reaparr.Domain;
using Reaparr.Environment;
using Reaparr.Logging;

namespace Reaparr.Data.Contracts;

public static class DbContextConnections
{
    public static readonly string ConnectionString = new SqliteConnectionStringBuilder(
        $"Data Source={PathProvider.DatabasePath};"
    )
    {
        // Mixing shared-cache mode and write-ahead logging is discouraged. For optimal performance, remove Cache=Shared when the database is configured to use write-ahead logging.
        // https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings#basic
        Cache = SqliteCacheMode.Default,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Pooling = true,

        // Do not set the default timeout as it conflicts with the command timeout.
        // Source: https://stackoverflow.com/q/6232633/8205497
        // DefaultTimeout = 60,
    }.ToString();

    public static void DefaultConfiguration(this DbContextOptionsBuilder optionsBuilder, Type contextType)
    {
        // Source: https://github.com/tompazourek/NaturalSort.Extension
        SqliteConnection databaseConnection = new(ConnectionString);
        databaseConnection.CreateCollation(
            OrderByNaturalExtensions.CollationName,
            (x, y) => _naturalComparer.Compare(x, y)
        );

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.LogTo(text => LogManager.DbContextLogger(text), LogLevel.Error);
        optionsBuilder.EnableDetailedErrors();

        optionsBuilder.UseSqlite(
            databaseConnection,
            b =>
            {
                // Wait as long as needed for the database to be unlocked
                b.CommandTimeout(300);
                b.MigrationsAssembly(contextType.Assembly.FullName);
            }
        );
    }

    private static readonly NaturalSortComparer _naturalComparer = new(StringComparison.InvariantCultureIgnoreCase);
}
