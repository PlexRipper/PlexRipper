using Microsoft.Data.Sqlite;
using Reaparr.Identity;

namespace Reaparr.Data.UnitTests;

public class DbContextConnectionsUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldApplyPhaseOneConfigurationToEveryPhysicalConnection()
    {
        // Arrange
        var databasePath = Path.Combine(Path.GetTempPath(), $"reaparr-sqlite-config-{Guid.NewGuid():N}.db");
        var pathProvider = new MockPathProvider($"sqlite-config-{Guid.NewGuid():N}");
        var appRuntimeInfo = new MockAppRuntimeInfo();
        var reaparrOptions = new DbContextOptionsBuilder<ReaparrDbContext>();
        reaparrOptions.ConfigureSqlite(databasePath, SqliteOpenMode.ReadWriteCreate);
        var authOptions = new DbContextOptionsBuilder<AuthDbContext>();
        authOptions.ConfigureSqlite(databasePath, SqliteOpenMode.ReadWriteCreate);
        await using var setupContext = new ReaparrDbContext(reaparrOptions.Options, Log, pathProvider, appRuntimeInfo);
        DbContextConnections.EnableWriteAheadLogging(setupContext.Database.GetDbConnection());

        // Act
        await using var reaparrContext1 = new ReaparrDbContext(
            reaparrOptions.Options,
            Log,
            pathProvider,
            appRuntimeInfo
        );
        await using var authContext = new AuthDbContext(authOptions.Options, Log, pathProvider, appRuntimeInfo);
        await using var reaparrContext2 = new ReaparrDbContext(
            reaparrOptions.Options,
            Log,
            pathProvider,
            appRuntimeInfo
        );

        var configurations = await Task.WhenAll(
            ReadConfiguration(reaparrContext1),
            ReadConfiguration(authContext),
            ReadConfiguration(reaparrContext2)
        );

        // Assert
        configurations.ShouldAllBe(x => x == ExpectedConfiguration);
    }

    private static readonly SqliteConfiguration ExpectedConfiguration = new(
        120,
        1000,
        134217728,
        268435456,
        2,
        -20000,
        1,
        "normal",
        0,
        "wal"
    );

    private static async Task<SqliteConfiguration> ReadConfiguration(DbContext context)
    {
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();

        return new SqliteConfiguration(
            ((SqliteConnection)connection).DefaultTimeout,
            await ReadInt64(connection, "wal_autocheckpoint"),
            await ReadInt64(connection, "journal_size_limit"),
            await ReadInt64(connection, "mmap_size"),
            await ReadInt64(connection, "temp_store"),
            await ReadInt64(connection, "cache_size"),
            await ReadInt64(connection, "synchronous"),
            await ReadText(connection, "locking_mode"),
            await ReadInt64(connection, "secure_delete"),
            await ReadText(connection, "journal_mode")
        );
    }

    private static async Task<long> ReadInt64(System.Data.Common.DbConnection connection, string pragma)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA {pragma};";
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static async Task<string> ReadText(System.Data.Common.DbConnection connection, string pragma)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA {pragma};";
        return (string)(await command.ExecuteScalarAsync())!;
    }

    private sealed record SqliteConfiguration(
        long DefaultCommandTimeout,
        long WalAutoCheckpoint,
        long JournalSizeLimit,
        long MmapSize,
        long TempStore,
        long CacheSize,
        long Synchronous,
        string LockingMode,
        long SecureDelete,
        string JournalMode
    );
}
