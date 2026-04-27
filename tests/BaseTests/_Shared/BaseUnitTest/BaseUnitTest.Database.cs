namespace Reaparr.BaseTests;

public partial class BaseUnitTest : IDisposable
{
    private string _databaseName = string.Empty;

    // Held alive to keep the SQLite shared-cache in-memory database alive for the test duration.
    // SQLite destroys an in-memory database when all connections to it are closed.
    private ReaparrDbContext? _setupReaparrDbContext;
    private AuthDbContext? _setupAuthDbContext;

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

            return MockDatabase.GetMemoryReaparrDbContext(Mock.Container.Resolve<IPathProvider>(), _databaseName);
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

            return MockDatabase.GetMemoryAuthDbContext(Mock.Container.Resolve<IPathProvider>(), _databaseName);
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
        var mockPathProvider = Mock.Container.Resolve<IPathProvider>();
        var (reaparrContext, authContext) = MockDatabase.GetMemoryDbContext(mockPathProvider, _databaseName);

        // Hold references to keep the SQLite shared-cache in-memory connections open.
        // SQLite destroys the in-memory database when all connections close.
        _setupReaparrDbContext = reaparrContext;
        _setupAuthDbContext = authContext;
        await (reaparrContext, authContext).Setup(seed, mockPathProvider, options);
        IsDatabaseSetup = true;
        return seed;
    }
}
