using System.Net.Http.Json;
using PlexRipper.Domain.Config;

namespace PlexRipper.BaseTests;

public class BaseIntegrationTests : IAsyncLifetime, IAsyncDisposable
{
    #region Fields

    private readonly List<PlexMockServer> _plexMockServers = new();
    private System.Net.Http.HttpClient _client = null;
    private MockPlexApi _mockPlexApi;
    protected BaseContainer Container;

    #endregion

    #region Constructors

    protected BaseIntegrationTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    #endregion

    #region Properties

    protected int MockPlexServerCount => _plexMockServers.Count;

    protected bool AllMockPlexServersStarted => _plexMockServers.All(x => x.IsStarted);

    protected List<Uri> GetPlexServerUris => _plexMockServers.Select(x => x.ServerUri).ToList();

    #endregion

    #region Methods

    #region Private

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        _client.Dispose();
        await Container.Boot.StopAsync(CancellationToken.None);
        Container?.Dispose();
        Log.Fatal("Container disposed");
    }

    #endregion

    #region Public

    public async Task InitializeAsync()
    {
        Log.Information("Initialize Integration Test");
    }

    public async Task DisposeAsync() { }

    #endregion

    #endregion

    protected async Task CreateContainer(int seed)
    {
        await CreateContainer(config => config.Seed = seed);
    }

    protected async Task CreateContainer([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        Container = await BaseContainer.Create(options);
    }

    protected void SpinUpPlexServer([CanBeNull] Action<PlexMockServerConfig> options = null)
    {
        _plexMockServers.Add(new PlexMockServer(options));
    }

    protected void SpinUpPlexServers([CanBeNull] Action<List<PlexMockServerConfig>> options = null)
    {
        var config = PlexMockServerConfig.FromOptions(options);
        foreach (var serverConfig in config)
            _plexMockServers.Add(new PlexMockServer(serverConfig));
    }

    protected void SetupMockPlexApi([CanBeNull] Action<MockPlexApiConfig> options = null)
    {
        _mockPlexApi = new MockPlexApi(options, GetPlexServerUris);
    }


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
}