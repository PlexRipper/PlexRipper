using Data.Contracts;
using Logging.Interface;
using PlexRipper.Data;
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

    protected int Seed { get; set; } = Random.Shared.Next(int.MaxValue);

    protected async Task CreateContainer(int seed)
    {
        await CreateContainer(config => config.Seed = seed);
    }

    protected async Task CreateContainer(Action<UnitTestDataConfig>? options = null)
    {
        Container = await BaseContainer.Create(_log, options);
    }

    protected async Task SetupDatabase(Action<FakeDataConfig>? options = null)
    {
        // Database context can be setup once and then retrieved by its DB name.
        await MockDatabase.GetMemoryDbContext(DatabaseName).Setup(Seed, options);
    }

    protected Uri SpinUpPlexServer(Action<PlexMockServerConfig> options = null)
    {
        var mockServer = new PlexMockServer(options);
        _plexMockServers.Add(mockServer);
        return mockServer.ServerUri;
    }

    protected PlexRipperDbContext DbContext => Container.PlexRipperDbContext;

    protected IPlexRipperDbContext IDbContext => Container.IPlexRipperDbContext;

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
