using Autofac;
using Data.Contracts;
using Environment;
using Logging.Interface;
using PlexApi.Contracts;
using PlexRipper.Data;
using PlexRipper.Identity;
using PlexRipper.Identity.Contracts;
using Serilog;
using Serilog.Events;
using Log = Logging.Log;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly LogEventLevel _logEventLevel;

    private string _databaseName = string.Empty;

    protected bool IsDatabaseSetup;

    protected readonly ILog Log;

    protected AutoMock mock { get; set; }

    // Use loose behavior here to avoid Dispose() not mocked exception
    protected Mock<HttpMessageHandler> HttpHandlerMock = new(MockBehavior.Loose);

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        _output = output;
        _logEventLevel = logEventLevel;

        EnvironmentExtensions.SetUnmaskedLogMode(true);

        LogManager.SetupLogging(logEventLevel);
        LogConfig.SetTestOutputHelper(output);
        BogusExtensions.Setup();
        Log = LogManager.CreateLogInstance(output, typeof(BaseUnitTest));

        mock = AutoMock.GetStrict(SetDefaultBuilder);
    }

    /// <summary>
    /// Gets a new instance of <see cref="PlexRipperDbContext"/> for every time it is called.
    /// </summary>

    // ReSharper disable once InconsistentNaming
    protected IPlexRipperDbContext IDbContext
    {
        get
        {
            if (!IsDatabaseSetup)
            {
                var logEvent = Log.ErrorLine(
                    "The test database has not been setup yet, run SetupDatabase() in the test first!"
                );
                throw new Exception(logEvent.ToString());
            }

            return MockDatabase.GetMemoryPlexRipperDbContext(_databaseName);
        }
    }

    protected IAuthDbContext IAuthDbContext
    {
        get
        {
            if (!IsDatabaseSetup)
            {
                var logEvent = Log.ErrorLine(
                    "The test database has not been setup yet, run SetupDatabase() in the test first!"
                );
                throw new Exception(logEvent.ToString());
            }

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
        // Database context can be setup once and then retrieved by its DB name.
        _databaseName = MockDatabase.GetMemoryDatabaseName();
        await MockDatabase.GetMemoryDbContext(_databaseName).Setup(seed, options);
        IsDatabaseSetup = true;
        return seed;
    }

    private void SetDefaultBuilder(ContainerBuilder builder)
    {
        builder
            .Register<ILogger>(
                (_, _) =>
                {
                    LogManager.SetupLogging(_logEventLevel);
                    LogConfig.SetTestOutputHelper(_output);
                    return LogConfig.GetLogger();
                }
            )
            .SingleInstance();

        // Database context can be setup once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(_databaseName))
            .As<PlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryPlexRipperDbContext(_databaseName))
            .As<IPlexRipperDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<AuthDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(_databaseName))
            .As<IAuthDbContext>()
            .InstancePerDependency();

        builder.RegisterType<Log>().As<ILog>().SingleInstance();
        builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>)).InstancePerDependency();
    }

    protected void SetupHttpClient(Action<Mock<HttpMessageHandler>>? action = null)
    {
        mock = AutoMock.GetStrict(builder =>
        {
            SetDefaultBuilder(builder);

            builder
                .Register(_ =>
                {
                    action?.Invoke(HttpHandlerMock);

                    return new HttpClient(HttpHandlerMock.Object);
                })
                .As<HttpClient>()
                .SingleInstance();
        });

        // Mock to avoid HttpClient.Dispose() not mocked exception
        mock.Mock<IPlexApiClient>().Setup(x => x.Dispose());
    }

    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
        {
            MockDatabase.GetMemoryPlexRipperDbContext(_databaseName).EnsureDeleted();
        }
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
