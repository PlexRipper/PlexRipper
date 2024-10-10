using Environment;
using Microsoft.Data.Sqlite;

namespace Data.Contracts;

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
}
