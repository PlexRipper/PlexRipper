using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    protected T SetupEndpointUnitTest<T>(Action<IServiceCollection>? extraServices = null)
        where T : class, IEndpoint
    {
        return Factory.Create<T>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                // All different dependencies that are needed for the endpoint need to be added here. And then they can be mocked in the test.
                s.AddTransient(_ => Mock.Create<ILogger>());
                s.AddTransient(_ => Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Mock<IReaparrDbContextFactory>().Object);
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContextFactory>());
                s.AddTransient(_ => Mock.Mock<ICommandExecutor>().Object);
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<IProgressHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<INotificationHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadTaskScheduler>().Object);
                s.AddSingleton(_ => Mock.Container.Resolve<IPathProvider>());
                s.AddSingleton(_ => Mock.Container.Resolve<IAppBuildInfo>());
                s.AddSingleton(_ => Mock.Mock<IHostApplicationLifetime>().Object);

                extraServices?.Invoke(s);
            });
        });
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
