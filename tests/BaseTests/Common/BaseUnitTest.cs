using Autofac;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using PlexRipper.Data;
using PlexRipper.WebAPI;
using Serilog;
using Serilog.Events;
using Log = Logging.Log;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    #region Fields

    private string _databaseName;
    protected PlexRipperDbContext DbContext;
    private bool disableForeignKeyCheck;
    private bool isDatabaseSetup;
    protected ILog _log;

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
        _log = LogManager.CreateLogInstance(output, typeof(BaseUnitTest), logEventLevel);
    }

    #endregion

    #region Properties

    protected int Seed { get; set; } = 0;

    /// <summary>
    /// Gets a new instance of <see cref="PlexRipperDbContext"/> for every time it is called.
    /// </summary>

    // ReSharper disable once InconsistentNaming
    protected IPlexRipperDbContext IDbContext => GetDbContext();

    #endregion

    #region Methods

    #region Public

    public virtual void Dispose()
    {
        DbContext?.Dispose();
    }

    #endregion

    #endregion

    /// <summary>
    /// Create a new <see cref="PlexRipperDbContext"/> instance for the same in memory database and assign it to DbContext.
    /// This is useful to recreate a DbContext that should not be reused between operations.
    /// </summary>
    protected void ResetDbContext()
    {
        DbContext = GetDbContext();
    }

    protected PlexRipperDbContext GetDbContext()
    {
        if (!isDatabaseSetup)
        {
            var logEvent = _log.ErrorLine(
                "The test database has not been setup yet, run SetupDatabase() in the test first!"
            );
            throw new Exception(logEvent.ToLogString());
        }

        return MockDatabase.GetMemoryDbContext(_databaseName, disableForeignKeyCheck);
    }

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.
    /// </summary>
    /// <param name="options"></param>
    protected async Task SetupDatabase(Action<FakeDataConfig> options = null)
    {
        isDatabaseSetup = true;

        var config = FakeDataConfig.FromOptions(options);
        disableForeignKeyCheck = config.DisableForeignKeyCheck;

        // Database context can be setup once and then retrieved by its DB name.
        DbContext = await GetDbContext().Setup(Seed, options);
        _databaseName = DbContext.DatabaseName;
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
            builder.RegisterInstance(MapperSetup.CreateMapper()).As<IMapper>().SingleInstance();
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

    public override void Dispose()
    {
        base.Dispose();
        mock?.Dispose();
    }

    #endregion

    #endregion
}
