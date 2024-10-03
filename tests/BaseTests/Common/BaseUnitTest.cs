using Autofac;
using Data.Contracts;
using Logging.Interface;
using PlexRipper.Application;
using PlexRipper.Data;
using Serilog;
using Serilog.Events;
using Log = Logging.Log;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    private string _databaseName;
    private bool _isDatabaseSetup;
    protected readonly ILog Log;

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
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
    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected readonly AutoMock mock;

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel)
    {
        mock = AutoMock.GetStrict(builder =>
        {
            builder
                .Register<ILogger>(
                    (_, _) =>
                    {
                        LogManager.SetupLogging(logEventLevel);
                        LogConfig.SetTestOutputHelper(output);
                        return LogConfig.GetLogger();
                    }
                )
                .SingleInstance();

            // Register the Mediatr module if the class is a handler.
            if (typeof(TUnitTestClass).Name.Contains("Handler"))
                builder.RegisterModule<MediatrModule>();

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

    /// <summary>
    /// Sends a Mediatr request to the handler through the various pipelines.
    /// This ensures its as close to production as possible.
    /// </summary>
    /// <param name="request"> The request to send to the handler. </param>
    /// <typeparam name="TResponse"> The response type of the request. </typeparam>
    /// <returns> The response from the handler. </returns>
    protected Task<TResponse> SendMediatr<TResponse>(IRequest<TResponse> request) =>
        mock.Create<IMediator>().Send(request);

    public new virtual void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
