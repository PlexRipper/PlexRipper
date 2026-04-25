namespace Reaparr.BaseTests;

[NotInParallel("IntegrationTests")]
public abstract class BaseIntegrationTests
{
    private readonly ILogger _log;

    protected CancellationToken CancellationToken =>
        TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None;

    /// <summary>
    /// Sets the given environment variable overrides for the duration of the test and then restores them.
    /// Integration tests boot the full AppHost in separate async flows, so this helper updates both
    /// process environment variables and the AsyncLocal test overrides used by <see cref="EnvironmentExtensions"/>.
    /// </summary>
    protected static IDisposable OverrideEnvironmentVariables(
        IReadOnlyDictionary<string, string?> environmentVariables
    ) => new IntegrationEnvironmentOverrideScope(environmentVariables);

    protected BaseIntegrationTests(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        EnvironmentExtensions.SetLogLevel(logLevel);
        EnvironmentExtensions.EnableUnmaskedLog(true);

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

    /// <summary>
    /// Integration tests boot the full AppHost and exercise it through hosted startup, HTTP request handling,
    /// and background work that may run outside the original async flow of the test method.
    /// <see cref="EnvironmentExtensions.WithOverrides(IReadOnlyDictionary{string, string?})"/> only uses
    /// <see cref="AsyncLocal{T}"/>, which is flow-scoped and therefore not reliable for all hosted integration
    /// paths. This scope applies the same overrides to both the process environment and the AsyncLocal-based
    /// test override mechanism, then restores the original process values on dispose.
    /// </summary>
    private sealed class IntegrationEnvironmentOverrideScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues;
        private readonly IDisposable _asyncLocalOverride;

        public IntegrationEnvironmentOverrideScope(IReadOnlyDictionary<string, string?> environmentVariables)
        {
            _originalValues = environmentVariables.ToDictionary(
                pair => pair.Key,
                pair => System.Environment.GetEnvironmentVariable(pair.Key)
            );

            foreach (var pair in environmentVariables)
                System.Environment.SetEnvironmentVariable(pair.Key, pair.Value);

            _asyncLocalOverride = EnvironmentExtensions.WithOverrides(environmentVariables);
        }

        public void Dispose()
        {
            _asyncLocalOverride.Dispose();

            foreach (var pair in _originalValues)
                System.Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}
