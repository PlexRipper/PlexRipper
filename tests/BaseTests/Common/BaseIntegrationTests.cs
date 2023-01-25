using System.Net.Http.Json;
using Logging.Interface;
using PlexRipper.Data;
using PlexRipper.Domain.Config;
using Serilog.Core;
using Serilog.Events;

namespace PlexRipper.BaseTests;

[Collection("Sequential")]
public class BaseIntegrationTests : IAsyncLifetime
{
    #region Fields

    private readonly List<PlexMockServer> _plexMockServers = new();
    private System.Net.Http.HttpClient _client;
    private MockPlexApi _mockPlexApi;
    protected BaseContainer Container;
    protected ILog _log;

    #endregion

    #region Constructors

    protected BaseIntegrationTests(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        // Log.SetupTestLogging(output, logLevel);
        LogConfig.SetTestOutputHelper(output);
        _log = LogConfig.GetLog(logLevel);
        DatabaseName = MockDatabase.GetMemoryDatabaseName();
        Log.Information($"Initialized integration test with database name: {DatabaseName}");
        BogusExtensions.Setup();
    }

    #endregion

    #region Properties

    protected int MockPlexServerCount => _plexMockServers.Count;

    protected bool AllMockPlexServersStarted => _plexMockServers.All(x => x.IsStarted);

    protected List<Uri> GetPlexServerUris => _plexMockServers.Select(x => x.ServerUri).ToList();

    protected string DatabaseName { get; }

    protected int Seed { get; set; } = 0;

    #endregion

    #region Methods

    #region Protected

    #region SetupPlexRipper

    protected async Task CreateContainer(int seed)
    {
        await CreateContainer(config => config.Seed = seed);
    }

    protected async Task CreateContainer(Action<UnitTestDataConfig> options = null)
    {
        Container = await BaseContainer.Create(DatabaseName, Seed, options, _mockPlexApi);
    }

    #endregion

    #region MockPlexApi

    protected void SetupMockPlexApi(Action<MockPlexApiConfig> options = null)
    {
        if (Container is not null)
        {
            const string msg = $"{nameof(CreateContainer)}() has already been called, cannot {nameof(SetupMockPlexApi)}()";
            Log.Error(msg);
            throw new Exception(msg);
        }

        _mockPlexApi = new MockPlexApi(_log, options, GetPlexServerUris);
    }

    #endregion

    #region SetupDatabase

    protected async Task SetupDatabase(Action<FakeDataConfig> options = null)
    {
        // Database context can be setup once and then retrieved by its DB name.
        await MockDatabase.GetMemoryDbContext(DatabaseName).Setup(Seed, options);
    }

    #endregion

    #region MockPlexServer

    protected Uri SpinUpPlexServer(Action<PlexMockServerConfig> options = null)
    {
        var mockServer = new PlexMockServer(_log, options);
        _plexMockServers.Add(mockServer);
        return mockServer.ServerUri;
    }

    protected void SpinUpPlexServers(Action<List<PlexMockServerConfig>> options = null)
    {
        var config = PlexMockServerConfig.FromOptions(options);
        foreach (var serverConfig in config)
            _plexMockServers.Add(new PlexMockServer(serverConfig));
    }

    #endregion

    #region HttpClient

    protected async Task<T> GetAsync<T>(string requestUri)
    {
        _client ??= _mockPlexApi.CreateClient();
        try
        {
            var response = await _client.GetAsync(requestUri);
            var x = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<T>(DefaultJsonSerializerOptions.ConfigBase);
        }
        catch (Exception e)
        {
            Log.Error($"RequestURI: {requestUri}");
            Log.Error(e);
            throw;
        }
    }

    protected async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage httpRequestMessage,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        _client = _mockPlexApi is not null ? _mockPlexApi.CreateClient() : new System.Net.Http.HttpClient();
        try
        {
            return await _client.SendAsync(httpRequestMessage, option);
        }
        catch (Exception e)
        {
            Log.Error($"RequestURI: {httpRequestMessage.RequestUri}");
            Log.Error(e);
            throw;
        }
    }

    protected PlexRipperDbContext DbContext => Container.PlexRipperDbContext;

    #endregion

    #endregion

    #region Public

    public Task InitializeAsync()
    {
        Log.Information("Initialize Integration Test");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Log.Warning("Integration Test has ended, Disposing!");

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

        Log.Fatal("Container disposed");
    }

    #endregion

    #endregion
}