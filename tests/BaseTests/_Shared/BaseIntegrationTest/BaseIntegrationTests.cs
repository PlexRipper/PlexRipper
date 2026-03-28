using Reaparr.Environment;
using Serilog.Events;

namespace Reaparr.BaseTests;

[NotInParallel("IntegrationTests")]
public abstract class BaseIntegrationTests
{
    private readonly ILogger _log;

    protected CancellationToken CancellationToken =>
        TUnit.Core.TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None;

    protected BaseIntegrationTests(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        EnvironmentExtensions.SetLogLevel(logLevel);
        EnvironmentExtensions.EnableUnmaskedLog(true);

        var testLogConfig = new TestLogConfig();

        // Pass the TestLogConfig to LogFactory so all application logs go to test output
        LogFactory.SetupLogging(logLevel, testLogConfig);
        _log = LogFactory.Create<BaseIntegrationTests>();

        BogusExtensions.Setup();
    }

    protected static async Task WaitForDatabaseConditionAsync(
        Func<bool> condition,
        int maxRetries = 10,
        int delayMs = 500
    )
    {
        for (var i = 0; i < maxRetries; i++)
        {
            if (condition())
                return;

            await Task.Delay(delayMs);
        }

        throw new TimeoutException(
            $"Database condition was not met after {maxRetries} retries (total wait: {maxRetries * delayMs}ms)"
        );
    }

    protected static async Task WaitForDatabaseConditionAsync(
        Func<Task<bool>> condition,
        int maxRetries = 10,
        int delayMs = 500
    )
    {
        for (var i = 0; i < maxRetries; i++)
        {
            if (await condition())
                return;

            await Task.Delay(delayMs);
        }

        throw new TimeoutException(
            $"Database condition was not met after {maxRetries} retries (total wait: {maxRetries * delayMs}ms)"
        );
    }

    protected Task<BaseContainer> CreateContainer(int seed, Action<UnitTestDataConfig>? options = null) =>
        CreateContainer(new Seed(seed), options);

    protected Task<BaseContainer> CreateContainer(Seed seed, Action<UnitTestDataConfig>? options = null) =>
        BaseContainer.Create(_log, seed, options);
}
