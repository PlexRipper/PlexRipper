using System;
using System.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlexRipper.BaseTests
{
    public class PlexMockServer : IDisposable
    {
        private readonly PlexMockServerConfig _config;

        #region Fields

        private readonly byte[] _downloadFile = { };

        #endregion

        #region Constructor

        public PlexMockServer(PlexMockServerConfig config = null)
        {
            _config = config ?? new PlexMockServerConfig();

            if (_config.DownloadFileSizeInMb > 0)
            {
                _downloadFile = FakeData.GetDownloadFile(_config.DownloadFileSizeInMb);
            }

            Server = WireMockServer.Start();
            ServerUri = new Uri(Server.Urls[0]);
            GetDownloadUri = new Uri($"{Server.Urls[0]}{PlexMockServerConfig.FileUrl}");

            Setup(config);
        }

        #endregion

        #region Properties

        public WireMockServer Server { get; }

        public Uri GetDownloadUri { get; }

        public Uri ServerUri { get; }

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

            return Server;
        }

        #endregion

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}