using System.Net.Http.Json;
using Data.Contracts;
using Logging.Interface;
using PlexRipper.Data;
using PlexRipper.Domain.Config;
using Serilog.Events;

namespace PlexRipper.BaseTests;

[Collection("Sequential")]
public class BaseIntegrationTests : IAsyncLifetime
{
    private readonly List<PlexMockServer> _plexMockServers = new();
    private HttpClient _client;
    private MockPlexApi _mockPlexApi;
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

    protected List<Uri> GetPlexServerUris => _plexMockServers.Select(x => x.ServerUri).ToList();

    protected string DatabaseName { get; } = MockDatabase.GetMemoryDatabaseName();

    protected int Seed { get; set; } = Random.Shared.Next(int.MaxValue);

    protected async Task CreateContainer(int seed)
    {
        await CreateContainer(config => config.Seed = seed);
    }

    protected async Task CreateContainer(Action<UnitTestDataConfig>? options = null)
    {
        Container = await BaseContainer.Create(_log, DatabaseName, options, _mockPlexApi);
    }

    protected void SetupMockPlexApi(Action<MockPlexApiConfig> options = null)
    {
        if (Container is not null)
        {
            _log.Here()
                .Error(
                    "{NameOfCreateContainer}() has already been called, cannot {NameOfSetupMockPlexApi}()",
                    nameof(CreateContainer),
                    nameof(SetupMockPlexApi)
                );

            // throw new Exception(msg);
        }

        _mockPlexApi = new MockPlexApi(_log, options, GetPlexServerUris);
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

    protected void SpinUpPlexServers(Action<List<PlexMockServerConfig>> options = null)
    {
        var config = PlexMockServerConfig.FromOptions(options);
        foreach (var serverConfig in config)
            _plexMockServers.Add(new PlexMockServer(serverConfig));
    }

    protected async Task<T> GetAsync<T>(string requestUri)
    {
        _client ??= _mockPlexApi.CreateClient();
        try
        {
            var response = await _client.GetAsync(requestUri);
            var x = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<T>(DefaultJsonSerializerOptions.ConfigStandard);
        }
        catch (Exception e)
        {
            _log.Error("RequestURI: {RequestUri}", requestUri);
            _log.Error(e);
            throw;
        }
    }

    protected async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage httpRequestMessage,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead
    )
    {
        _client = _mockPlexApi is not null ? _mockPlexApi.CreateClient() : new HttpClient();
        try
        {
            return await _client.SendAsync(httpRequestMessage, option);
        }
        catch (Exception e)
        {
            _log.Error("RequestURI: {RequestUri}", httpRequestMessage.RequestUri);
            _log.Error(e);
            throw;
        }
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
