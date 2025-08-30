using Reaparr.Environment;
using Reaparr.Logging;
using Serilog.Events;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.BaseTests;

[Collection("Integration Tests")]
public abstract class BaseIntegrationTests
{
    private readonly ILog _log;

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected BaseIntegrationTests(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        EnvironmentExtensions.SetLogLevel(logLevel);
        EnvironmentExtensions.EnableUnmaskedLog(true);

        // Ensure that the test output helper is set first
        var testLogConfig = new TestLogConfig(output);

        LogManager.SetupLogging(logLevel);
        _log = testLogConfig.CreateLogInstance<BaseIntegrationTests>();

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
    }

    protected Task<BaseContainer> CreateContainer(int seed, Action<UnitTestDataConfig>? options = null) =>
        CreateContainer(new Seed(seed), options);

    protected Task<BaseContainer> CreateContainer(Seed seed, Action<UnitTestDataConfig>? options = null) =>
        BaseContainer.Create(_log, seed, options);
}
