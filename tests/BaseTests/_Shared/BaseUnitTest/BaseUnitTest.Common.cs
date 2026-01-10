using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.Identity.Contracts;
using Serilog.Events;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    protected readonly ITestOutputHelper Output;

    protected readonly ILogger Log;

    // Use loose behavior here to avoid Dispose() not mocked exception
    protected Mock<HttpMessageHandler> HttpHandlerMock = new(MockBehavior.Loose);

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// This constructor is run before every test
    /// </summary>
    /// <param name="output">Sets up the logging system for logging during testing.</param>
    /// <param name="logEventLevel"></param>
    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        Output = output;

        EnvironmentExtensions.EnableUnmaskedLog(true);

        // Pass the TestLogConfig to LogFactory so all application logs go to test output
        var testLogConfig = new TestLogConfig(output);
        LogFactory.SetupLogging(logEventLevel, testLogConfig);

        BogusExtensions.Setup();

        Log = LogFactory.Create<BaseUnitTest>();

        Mock = AutoMock.GetStrict(SetDefaultBuilder);
    }

    protected T SetupEndpointUnitTest<T>()
        where T : class, IEndpoint
    {
        return Factory.Create<T>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                // All different dependencies that are needed for the endpoint need to be added here. And then they can be mocked in the test.
                s.AddTransient(_ => Mock.Create<ILogger>());
                s.AddTransient(_ => Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<ICommandExecutor>());
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<ISignalRService>().Object);
            });
        });
    }

    public virtual void Dispose()
    {
        if (IsDatabaseSetup)
        {
            MockDatabase.GetMemoryReaparrDbContext(_databaseName).EnsureDeleted();
        }
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest
    where TUnitTestClass : class
{
    protected TUnitTestClass Sut => Mock.Create<TUnitTestClass>();

    protected BaseUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    public override void Dispose()
    {
        base.Dispose();
        Mock.Dispose();
    }
}
