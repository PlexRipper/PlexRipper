using Environment;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.BaseTests;

public class BaseIntegrationTests
{
    private readonly ILog _log;

    protected BaseIntegrationTests(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Verbose)
    {
        EnvironmentExtensions.SetLogLevel(logLevel);
        EnvironmentExtensions.EnableUnmaskedLog(true);

        // Ensure that the test output helper is set first
        LogConfig.SetTestOutputHelper(output);

        LogManager.SetupLogging(logLevel);
        _log = LogManager.CreateLogInstance(typeof(BaseIntegrationTests));

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
