using Data.Contracts;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.BaseTests;

[Collection("Sequential")]
public class BaseIntegrationTests : IAsyncLifetime
{
    private readonly List<PlexMockServer> _plexMockServers = new();
    private HttpClient _client;
    protected BaseContainer Container;
    protected readonly ILog _log;

    protected BaseIntegrationTests(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Verbose)
    {
        System.Environment.SetEnvironmentVariable("LOG_LEVEL", logLevel.ToString().ToUpper());

        // Ensure that the test output helper is set first
        LogConfig.SetTestOutputHelper(output);

        LogManager.SetupLogging(logLevel);
        _log = LogManager.CreateLogInstance(typeof(BaseIntegrationTests));
        _log.Information("Initialized integration test with database name: {DatabaseName}", DatabaseName);
        BogusExtensions.Setup();
    }

    protected int MockPlexServerCount => _plexMockServers.Count;

    protected bool AllMockPlexServersStarted => _plexMockServers.All(x => x.IsStarted);

    protected string DatabaseName { get; } = MockDatabase.GetMemoryDatabaseName();

    protected async Task CreateContainer(Action<UnitTestDataConfig>? options = null)
    {
        Container = await BaseContainer.Create(_log, options);
    }

    protected IPlexRipperDbContext DbContext => Container.Resolve<IPlexRipperDbContext>();

    public Task InitializeAsync()
    {
        _log.InformationLine("Initialize Integration Test");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _log.WarningLine("Integration Test has ended, Disposing!");

        // Dispose HttpClient
        _client?.Dispose();

        // Dispose PlexMockServers
        foreach (var plexMockServer in _plexMockServers)
            plexMockServer.Dispose();

        if (Container is not null)
        {
            await Container.Boot.StopAsync(CancellationToken.None);
            Container.Dispose();
        }

        _log.FatalLine("Container disposed");
    }
}
