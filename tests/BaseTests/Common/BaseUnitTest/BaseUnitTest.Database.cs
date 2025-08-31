using Reaparr.Data;
using Reaparr.Data.Contracts;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest : IDisposable
{
    private string _databaseName = string.Empty;

    protected bool IsDatabaseSetup;

    /// <summary>
    /// Gets a new instance of <see cref="ReaparrDbContext"/> for every time it is called.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    protected IReaparrDbContext IDbContext
    {
        get
        {
            DataBaseSetupGuard();

            return MockDatabase.GetMemoryReaparrDbContext(_databaseName);
        }
    }

    private void DataBaseSetupGuard()
    {
        if (!IsDatabaseSetup)
        {
            var logEvent = Log.ErrorMsg(
                "The test database has not been setup yet, run SetupDatabase() in the test first!"
            );
            throw new Exception(logEvent);
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

    protected Mock<IReaparrDbContext> MockIDbContext => new();

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="ReaparrDbContext"/> for every test.
    /// </summary>
    /// <param name="seed"> The fake data seed to use for the database setup.</param>
    /// <param name="options"> The options to use for the fake data setup.</param>
    protected Task<Seed> SetupDatabase(int seed, Action<FakeDataConfig>? options = null) =>
        SetupDatabase(new Seed(seed), options);

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="ReaparrDbContext"/> for every test.
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
