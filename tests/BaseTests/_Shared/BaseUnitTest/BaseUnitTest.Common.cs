using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.BaseTests;

public partial class BaseUnitTest
{
    protected readonly ILogger Log;

    /// <summary>
    /// Serializes tests that mutate <see cref="System.Environment"/> variables.
    /// </summary>
    protected static readonly SemaphoreSlim EnvironmentLock = new(1, 1);

    /// <summary>
    /// Sets the given environment variables for the duration of <paramref name="action"/>,
    /// then restores the originals. Serialized via <see cref="EnvironmentLock"/> to keep
    /// parallel tests from interfering with each other.
    /// </summary>
    protected static async Task WithEnvironmentVariablesAsync(
        IReadOnlyDictionary<string, string?> environmentVariables,
        Func<Task> action
    )
    {
        await EnvironmentLock.WaitAsync();

        var originalValues = environmentVariables.ToDictionary(
            x => x.Key,
            x => System.Environment.GetEnvironmentVariable(x.Key)
        );

        try
        {
            foreach (var environmentVariable in environmentVariables)
                System.Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);

            await action();
        }
        finally
        {
            foreach (var originalValue in originalValues)
                System.Environment.SetEnvironmentVariable(originalValue.Key, originalValue.Value);

            EnvironmentLock.Release();
        }
    }

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
        EnvironmentExtensions.EnableUnmaskedLog(true);

        // Pass the TestLogConfig to LogFactory so all application logs go to test output
        var testLogConfig = new TestLogConfig();
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
                s.AddTransient(_ => Mock.Mock<IReaparrDbContextFactory>().Object);
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContextFactory>());
                s.AddTransient(_ => Mock.Mock<ICommandExecutor>().Object);
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<IProgressHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<INotificationHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadTaskScheduler>().Object);
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
