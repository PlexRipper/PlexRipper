using Data.Contracts;
using PlexRipper.Data;
using PlexRipper.Identity.Contracts;

namespace PlexRipper.BaseTests;

public partial class BaseUnitTest : IDisposable
{
    private string _databaseName = string.Empty;

    protected bool IsDatabaseSetup;

    /// <summary>
    /// Gets a new instance of <see cref="PlexRipperDbContext"/> for every time it is called.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    protected IPlexRipperDbContext IDbContext
    {
        get
        {
            DataBaseSetupGuard();

            return MockDatabase.GetMemoryPlexRipperDbContext(_databaseName);
        }
    }

    private void DataBaseSetupGuard()
    {
        if (!IsDatabaseSetup)
        {
            var logEvent = Log.ErrorLine(
                "The test database has not been setup yet, run SetupDatabase() in the test first!"
            );
            throw new Exception(logEvent.ToString());
        }
    }

    protected IAuthDbContext IAuthDbContext
    {
        get
        {
            DataBaseSetupGuard();

            return MockDatabase.GetMemoryAuthDbContext(_databaseName);
        }
    }

    protected Mock<IPlexRipperDbContext> MockIDbContext => new();

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.
    /// </summary>
    /// <param name="seed"> The fake data seed to use for the database setup.</param>
    /// <param name="options"> The options to use for the fake data setup.</param>
    protected Task<Seed> SetupDatabase(int seed, Action<FakeDataConfig>? options = null) =>
        SetupDatabase(new Seed(seed), options);

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.
    /// </summary>
    /// <param name="seed"> The fake data seed to use for the database setup.</param>
    /// <param name="options"> The options to use for the fake data setup.</param>
    protected async Task<Seed> SetupDatabase(Seed seed, Action<FakeDataConfig>? options = null)
    {
        // Database context can be set up once and then retrieved by its DB name.
        _databaseName = MockDatabase.GetMemoryDatabaseName();
        await MockDatabase.GetMemoryDbContext(_databaseName).Setup(seed, options);
        IsDatabaseSetup = true;
        return seed;
    }
}
