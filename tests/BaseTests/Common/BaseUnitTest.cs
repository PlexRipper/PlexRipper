using Autofac;
using AutoMapper;
using Logging.Interface;
using PlexRipper.Data;
using PlexRipper.WebAPI;
using Serilog.Events;
using Log = Logging.Log2.Log;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    #region Fields

    private string _databaseName;
    protected PlexRipperDbContext DbContext;
    private bool isDatabaseSetup = false;
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
        LogConfig.SetupLogging(logEventLevel);
        BogusExtensions.Setup();
        _log = new Log(LogConfig.GetLogger());
    }

    #endregion

    #region Properties

    protected int Seed { get; set; } = 0;

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
            var msg = $"The test database has not been setup yet, run \"{nameof(SetupDatabase)}()\" in the test first!";
            Logging.Log.Error(msg);
            throw new Exception(msg);
        }

        return MockDatabase.GetMemoryDbContext(_databaseName);
    }

    /// <summary>
    /// Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.
    /// </summary>
    /// <param name="options"></param>
    protected async Task SetupDatabase(Action<FakeDataConfig> options = null)
    {
        isDatabaseSetup = true;

        // Database context can be setup once and then retrieved by its DB name.
        DbContext = await GetDbContext().Setup(Seed, options);
        _databaseName = DbContext.DatabaseName;
    }

    protected void SaveChanges()
    {
        DbContext.SaveChanges();
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest where TUnitTestClass : class
{
    #region Fields

    protected TUnitTestClass _sut => mock.Create<TUnitTestClass>();

    protected readonly AutoMock mock;

    #endregion

    #region Constructors

    protected BaseUnitTest(ITestOutputHelper output) : base(output)
    {
        mock = AutoMock.GetStrict(builder =>
        {
            builder.RegisterInstance(MapperSetup.CreateMapper())
                .As<IMapper>()
                .SingleInstance();
        });
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