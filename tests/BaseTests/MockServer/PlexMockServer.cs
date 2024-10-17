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
    private readonly Action<PlexApiDataConfig>? _fakeDataConfig;

    public PlexMockServer(Seed seed, PlexMockServerConfig? options = null)
    {
        _config = options ?? new PlexMockServerConfig();
        _fakeDataConfig = _config.FakeDataConfig;

        Server = WireMockServer.Start(
            new WireMockServerSettings()
            {
                Logger = new WiremockLogger(_log),
                HostingScheme = HostingScheme.HttpAndHttps,
            }
        );

        ServerUri = new Uri(Server.Urls[0]);
        Setup(seed);
        _log.Debug("Created {NameOfPlexMockServer} with url: {ServerUri}", nameof(PlexMockServer), ServerUri);
    }

    public WireMockServer Server { get; }

    public Uri ServerUri { get; }

    public bool IsStarted => Server.IsStarted;

    private void Setup(Seed seed)
    {
        SetupServerIdentity(seed);

        // Set up the Plex libraries
        var librarySections = FakePlexApiData.GetAllLibrariesResponseBody(seed, _fakeDataConfig);

        Server
            .Given(Request.Create().WithPath(PlexApiPaths.LibrarySectionsPath).UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", ContentType.ApplicationJson)
                    .WithPlexSdkJsonContent(librarySections)
            );

        // Set up the media metadata for each library
        foreach (var librarySection in librarySections.MediaContainer.Directory)
        {
            var libraryData = FakePlexApiData.GetPlexLibrarySectionAllResponse(seed, librarySection, _fakeDataConfig);
            var url = PlexApiPaths.GetLibrariesSectionsPath(librarySection.Key);
            Server
                .Given(Request.Create().WithPath(url).UsingGet())
                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", ContentType.ApplicationJson)
                        .WithPlexSdkJsonContent(libraryData)
                );
        }

        SetupDownloadableFile();
    }

    private void SetupServerIdentity(Seed seed)
    {
        Server
            .Given(Request.Create().WithPath(PlexApiPaths.ServerIdentityPath).UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", ContentType.ApplicationJson)
                    .WithPlexSdkJsonContent(FakePlexApiData.GetPlexServerIdentityResponse(seed, _fakeDataConfig))
            );
    }

    private void SetupDownloadableFile()
    {
        if (_config.DownloadFileSizeInMb > 0)
        {
            var downloadFile = FakeData.GetDownloadFile(_config.DownloadFileSizeInMb);

            _log.Debug("Created file to be downloaded with length: {DownloadFileSize}", downloadFile.LongLength);
            Server
                .Given(Request.Create().WithPath(PlexMockServerConfig.FileUrl).UsingGet())
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
