using System.Net;
using Logging.Interface;
using PlexRipper.PlexApi;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using WireMock.Types;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock an individual Plex server, this is not the same as the PlexApi which is a central server
/// Source: https://github.com/WireMock-Net/WireMock.Net/
/// </summary>
public class PlexMockServer : IDisposable
{
    private readonly ILog _log = LogManager.CreateLogInstance<PlexMockServer>();
    private readonly PlexMockServerConfig _config;
    private readonly Action<PlexApiDataConfig> _fakeDataConfig;

    public PlexMockServer(PlexMockServerConfig? options = null)
    {
        _config = options ?? new PlexMockServerConfig();
        _fakeDataConfig = _config.FakeDataConfig;

        Server = WireMockServer.Start(
            new WireMockServerSettings()
            {
                // TODO Migrate this option to the new version: https://github.com/WireMock-Net/WireMock.Net/issues/1086
                //ThrowExceptionWhenMatcherFails = true,
                HostingScheme = HostingScheme.HttpAndHttps,
            }
        );

        ServerUri = new Uri(Server.Urls[0]);
        DownloadUri = new Uri($"{Server.Urls[0]}{PlexMockServerConfig.FileUrl}");
        Setup();
        _log.Debug("Created {NameOfPlexMockServer} with url: {ServerUri}", nameof(PlexMockServer), ServerUri);
    }

    public WireMockServer Server { get; }

    public Uri DownloadUri { get; }

    public Uri ServerUri { get; }

    public long DownloadFileSizeInBytes => _config.DownloadFileSizeInMb * 1024;

    public bool IsStarted => Server.IsStarted;

    private void Setup()
    {
        SetupServerIdentity();

        // Set up the Plex libraries
        var librarySections = FakePlexApiData.GetLibraryMediaContainer(_fakeDataConfig);

        Server
            .Given(Request.Create().WithPath(PlexApiPaths.LibrarySectionsPath).WithParam("X-Plex-Token").UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(librarySections)
            );

        // Set up the media metadata for each library
        foreach (var librarySection in librarySections.MediaContainer.Directory)
        {
            var libraryData = FakePlexApiData.GetPlexLibrarySectionAllResponse(librarySection, _fakeDataConfig);
            var url = PlexApiPaths.GetLibrariesSectionsPath(librarySection.Key);
            Server
                .Given(Request.Create().WithPath(url).WithParam("X-Plex-Token").UsingGet())
                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(libraryData)
                );
        }

        SetupDownloadableFile();
    }

    private void SetupServerIdentity()
    {
        Server
            .Given(Request.Create().WithPath(PlexApiPaths.ServerIdentityPath).UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(FakePlexApiData.GetPlexServerIdentityResponse(_fakeDataConfig))
            );
    }

    private void SetupDownloadableFile()
    {
        if (_config.DownloadFileSizeInMb > 0)
        {
            var downloadFile = FakeData.GetDownloadFile(_config.DownloadFileSizeInMb);

            _log.Debug("Created file to be downloaded with length: {DownloadFileSize}", downloadFile.LongLength);
            Server
                .Given(Request.Create().WithPath(PlexMockServerConfig.FileUrl).WithParam("X-Plex-Token").UsingGet())
                .RespondWith(
                    Response
                        .Create()
                        .WithHeader("Content-Type", "application/octet-stream")
                        .WithHeader("Content-Length", downloadFile.LongLength.ToString())
                        .WithStatusCode(HttpStatusCode.OK)
                        .WithBody(downloadFile)
                );
        }
    }

    public void Dispose()
    {
        Server.Stop();
        Server.Dispose();
    }
}
