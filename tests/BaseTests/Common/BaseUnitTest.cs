using Autofac;
using Data.Contracts;
using Logging.Interface;
using PlexRipper.Data;
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

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        _output = output;
        _logEventLevel = logEventLevel;

        LogManager.SetupLogging(logEventLevel);
        LogConfig.SetTestOutputHelper(output);
        BogusExtensions.Setup();
        Log = LogManager.CreateLogInstance(output, typeof(BaseUnitTest));
    }

    protected int Seed { get; set; } = 0;

    /// <summary>
    /// Gets a new instance of <see cref="PlexRipperDbContext"/> for every time it is called.
    /// </summary>

    // ReSharper disable once InconsistentNaming
    protected IPlexRipperDbContext IDbContext => GetDbContext();

    protected Mock<IPlexRipperDbContext> MockIDbContext => new();

    private List<PlexRipperDbContext> _dbContexts = new();

    protected PlexRipperDbContext GetDbContext()
    {
        if (!IsDatabaseSetup)
        {
            var logEvent = Log.ErrorLine(
                "The test database has not been setup yet, run SetupDatabase() in the test first!"
            );
            throw new Exception(logEvent.ToLogString());
        }

        _dbContexts.Add(MockDatabase.GetMemoryDbContext(_databaseName));
        return _dbContexts.Last();
    }

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.
    /// </summary>
    /// <param name="options"></param>
    protected async Task SetupDatabase(Action<FakeDataConfig>? options = null)
    {
        // Database context can be setup once and then retrieved by its DB name.
        var dbContext = await MockDatabase.GetMemoryDbContext().Setup(Seed, options);
        _databaseName = dbContext.DatabaseName;
        _dbContexts.Add(dbContext);
        IsDatabaseSetup = true;
    }

    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
            _dbContexts.ForEach(x => x.Dispose());
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected AutoMock mock { get; private set; }

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel)
    {
        mock = AutoMock.GetStrict(SetDefaultBuilder);
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
            .Register((_, _) => GetDbContext())
            .As<PlexRipperDbContext>()
            .As<IPlexRipperDbContext>()
            .InstancePerDependency();

        builder.RegisterType<Log>().As<ILog>().SingleInstance();
        builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>)).InstancePerDependency();
    }

    protected void SetupHttpClient(Action<Mock<HttpMessageHandler>?> action)
    {
        mock = AutoMock.GetStrict(builder =>
        {
            SetDefaultBuilder(builder);

            builder
                .Register(
                    (_, _) =>
                    {
                        var handlerMock = mock.Mock<HttpMessageHandler>();

                        action(handlerMock);

                        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost:1234") };
                    }
                )
                .As<HttpClient>()
                .SingleInstance();
        });
    }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
