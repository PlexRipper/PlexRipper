using PlexRipper.PlexApi.Common;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using WireMock.Types;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock a individual Plex server, this is not the same as the PlexApi which is a central server
/// Source: https://github.com/WireMock-Net/WireMock.Net/
/// </summary>
public class PlexMockServer : IDisposable
{
    private readonly PlexMockServerConfig _config;
    private readonly Action<PlexApiDataConfig> _fakeDataConfig;

    #region Fields

    private readonly byte[] _downloadFile = { };

    #endregion

    #region Constructor

    public PlexMockServer(Action<PlexMockServerConfig> options = null) : this(PlexMockServerConfig.FromOptions(options)) { }

    public PlexMockServer(PlexMockServerConfig options = null)
    {
        _config = options;
        _fakeDataConfig = _config.FakeDataConfig;
        if (_config.DownloadFileSizeInMb > 0)
            _downloadFile = FakeData.GetDownloadFile(_config.DownloadFileSizeInMb);

        Server = WireMockServer.Start(new WireMockServerSettings()
        {
            ThrowExceptionWhenMatcherFails = true,
        });

        ServerUri = new Uri(Server.Urls[0]);
        DownloadUri = new Uri($"{Server.Urls[0]}{PlexMockServerConfig.FileUrl}");
        Server = Setup(_config);
        Log.Debug($"Created {nameof(PlexMockServer)} with url: {ServerUri}");
    }

    #endregion

    #region Properties

    public WireMockServer Server { get; }

    public Uri DownloadUri { get; }

    public Uri ServerUri { get; }

    public long DownloadFileSizeInBytes => _config.DownloadFileSizeInMb * 1024;

    public bool IsStarted => Server.IsStarted;

    #endregion

    #region Public Methods

    private WireMockServer Setup(PlexMockServerConfig config)
    {
        if (_downloadFile?.Any() ?? false)
        {
            Server
                .Given(Request.Create().WithPath(PlexMockServerConfig.FileUrl).UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(206)
                        .WithBody(_downloadFile)
                );
        }

        Server
            .Given(Request.Create().WithPath(PlexApiPaths.ServerIdentityPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(FakePlexApiData.GetPlexServerIdentityResponse(_fakeDataConfig)));

        Server
            .Given(Request.Create().WithPath(PlexApiPaths.LibrarySectionsPath).WithParam("X-Plex-Token").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(FakePlexApiData.GetLibraryMediaContainer(_fakeDataConfig)));

        return Server;
    }

    #endregion

    public void Dispose()
    {
        Server.Stop();
        Server?.Dispose();
    }
}