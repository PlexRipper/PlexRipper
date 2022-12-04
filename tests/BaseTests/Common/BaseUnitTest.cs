using PlexRipper.Data;

namespace PlexRipper.BaseTests;

public class BaseUnitTest : IDisposable
{
    protected PlexRipperDbContext DbContext;
    protected readonly string DatabaseName;

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="enableMemoryDatabase">Creates and maintains a unique in memory database <see cref="PlexRipperDbContext"/> for every test.</param>
    protected BaseUnitTest(ITestOutputHelper output, bool enableMemoryDatabase = false)
    {
        Log.SetupTestLogging(output);
        if (enableMemoryDatabase)
        {
            DbContext = MockDatabase.GetMemoryDbContext();
            DatabaseName = DbContext.DatabaseName;
        }
    }

    /// <summary>
    /// Create a new <see cref="PlexRipperDbContext"/> instance for the same in memory database and assign it to DbContext.
    /// This is useful to recreate a DbContext that should not be reused between operations.
    /// </summary>
    protected void ResetDbContext()
    {
        DbContext = MockDatabase.GetMemoryDbContext(DatabaseName);
    }


    public virtual void Dispose()
    {
        DbContext?.Dispose();
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest where TUnitTestClass : class
{
    protected readonly TUnitTestClass _sut;

    protected readonly AutoMock mock = AutoMock.GetStrict();

    protected BaseUnitTest(ITestOutputHelper output, bool disableMockCreate = false, bool enableMemoryDatabase = false) : base(output, enableMemoryDatabase)
    {
        if (!disableMockCreate)
            _sut = mock.Create<TUnitTestClass>();
    }

    public override void Dispose()
    {
        base.Dispose();
        mock?.Dispose();
    }
}