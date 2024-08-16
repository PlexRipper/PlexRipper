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
    #region Fields

    private string _databaseName;
    private bool _isDatabaseSetup;
    protected readonly ILog Log;

    #endregion

    #region Constructors

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Debug)
    {
        LogConfig.SetTestOutputHelper(output);
        LogManager.SetupLogging(logEventLevel);
        BogusExtensions.Setup();
        Log = LogManager.CreateLogInstance(output, typeof(BaseUnitTest), logEventLevel);
    }

    #endregion

    #region Properties

    protected int Seed { get; set; } = 0;

    /// <summary>
    /// Gets a new instance of <see cref="PlexRipperDbContext"/> for every time it is called.
    /// </summary>

    // ReSharper disable once InconsistentNaming
    protected IPlexRipperDbContext IDbContext => GetDbContext();

    protected Mock<IPlexRipperDbContext> MockIDbContext => new();

    private List<PlexRipperDbContext> _dbContexts = new();

    #endregion

    protected PlexRipperDbContext GetDbContext()
    {
        if (!_isDatabaseSetup)
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
        _isDatabaseSetup = true;
    }

    public virtual void Dispose()
    {
        if (_isDatabaseSetup)
            _dbContexts.ForEach(x => x.Dispose());
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    #region Fields

    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected readonly AutoMock mock;

    #endregion

    #region Constructors

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Debug)
        : base(output, logEventLevel)
    {
        mock = AutoMock.GetStrict(builder =>
        {
            builder
                .Register<ILogger>(
                    (_, _) =>
                    {
                        LogConfig.SetTestOutputHelper(output);
                        return LogConfig.GetLogger(logEventLevel);
                    }
                )
                .SingleInstance();

            // Database context can be setup once and then retrieved by its DB name.
            builder
                .Register((_, _) => GetDbContext())
                .As<PlexRipperDbContext>() // Register as concrete type
                .As<IPlexRipperDbContext>() // Also register as interface
                .InstancePerDependency();

            builder.RegisterType<Log>().As<ILog>().SingleInstance();
            builder.RegisterGeneric(typeof(Log<>)).As(typeof(ILog<>)).InstancePerDependency();
        });

        mock.Mock<IHttpClientFactory>().Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
    }

    #endregion

    #region Methods

    #region Public

    public new virtual void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }

    #endregion

    #endregion
}
