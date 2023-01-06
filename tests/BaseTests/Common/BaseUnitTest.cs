using Autofac;
using AutoMapper;
using PlexRipper.Data;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    #region Fields

    private string _databaseName;
    protected PlexRipperDbContext DbContext;
    private bool isDatabaseSetup = false;

    #endregion

    #region Constructors

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    protected BaseUnitTest(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
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
            Log.Error(msg);
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

    protected readonly TUnitTestClass _sut;

    protected readonly AutoMock mock;

    #endregion

    #region Constructors

    protected BaseUnitTest(ITestOutputHelper output, bool disableMockCreate = false) : base(output)
    {
        mock = AutoMock.GetStrict(builder => builder.RegisterInstance(MapperSetup.CreateMapper())
            .As<IMapper>()
            .SingleInstance());
        ;

        if (!disableMockCreate)
            _sut = mock.Create<TUnitTestClass>();
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