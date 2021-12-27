using System;
using System.Linq;
using System.Reflection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlexRipper.BaseTests
{
    public class PlexMockServer
    {
        private readonly PlexMockServerConfig _config;

        #region Fields

        private static readonly string _fileUrl = "/media/movies/default";

        #endregion

        #region Constructor

        public PlexMockServer(PlexMockServerConfig config = null)
        {
            _config = config ?? new PlexMockServerConfig();

            Server = WireMockServer.Start();
            ServerUrl = Server.Urls[0];
            GetDownloadUri = new Uri($"{ServerUrl}{_fileUrl}");

            Setup(config);
        }

        #endregion

        #region Properties

        public Uri GetDownloadUri { get; }

        public WireMockServer Server { get; }

        public string ServerUrl { get; }

        #endregion

        #region Public Methods

        private WireMockServer Setup(PlexMockServerConfig config)
        {
            if (config.File?.Any() ?? false)
            {
                Server
                    .Given(Request.Create().WithPath(_fileUrl).UsingGet())
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(206)
                            .WithBody(config.File)
                    );
            }

            return Server;
        }

        #endregion
    }
}