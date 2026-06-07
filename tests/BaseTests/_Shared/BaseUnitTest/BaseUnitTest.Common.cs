namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    protected readonly ILogger Log;

    // Use loose behavior here to avoid Dispose() not mocked exception
    protected Mock<HttpMessageHandler> HttpHandlerMock = new(MockBehavior.Loose);

    protected CancellationToken CancellationToken =>
        TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None;

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        // Pass the TestLogConfig to LogFactory so all application logs go to test output
        var appRunTimeInfo = new MockAppRuntimeInfo { IsUnmasked = true };
        var testLogConfig = new TestLogConfig(
            appRunTimeInfo,
            new MockPathProvider(MockDatabase.GetMemoryDatabaseName())
        );
        LogFactory.SetupLogging(testLogConfig, appRunTimeInfo, logEventLevel);

        BogusExtensions.Setup();

        Log = LogFactory.Create<BaseUnitTest>();

        Mock = AutoMock.GetStrict(SetDefaultBuilder);
    }



    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
        {
            _setupReaparrDbContext?.EnsureDeleted();
            _setupReaparrDbContext?.Dispose();
            _setupAuthDbContext?.Dispose();
        }
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass Sut => Mock.Create<TUnitTestClass>();

    protected BaseUnitTest(LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(logEventLevel) { }

    public override void Dispose()
    {
        base.Dispose();
        Mock.Dispose();
    }
}
