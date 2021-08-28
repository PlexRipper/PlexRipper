using System.Collections.Generic;
using System.IO;
using System.Linq;
using Environment;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlexRipper.BaseTests
{
    public class MockServer : IMockServer
    {
        private readonly IPathSystem _pathSystem;

        private static readonly List<MockMediaData> _mockMediaData = new();

        public MockServer(IPathSystem pathSystem)
        {
            _pathSystem = pathSystem;
        }

        public string MockMovieMediaPath
        {
            get
            {
                var basePath = Directory.GetParent(_pathSystem.RootDirectory).Parent.Parent.Parent.Parent;
                return Path.Join(basePath.FullName, "BaseTests", "PlexMockServer", "media", "movies");
            }
        }

        public List<MockMediaData> GetMockMediaData()
        {
            if (!_mockMediaData.Any())
            {
                foreach (string dir in Directory.GetDirectories(MockMovieMediaPath))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        _mockMediaData.Add(new MockMediaData(PlexMediaType.Movie, file));
                    }
                }
            }

            return _mockMediaData;
        }

        public MockMediaData GetDefaultMovieMockMediaData()
        {
            string filepath = Path.Combine(MockMovieMediaPath, "default", "test-video.mp4");
            return new MockMediaData(PlexMediaType.Movie, filepath);
        }

        public WireMockServer GetPlexMockServer()
        {
            var _server = WireMockServer.Start();

            foreach (var mockMediaData in GetMockMediaData())
            {
                _server
                    .Given(Request.Create().WithPath(mockMediaData.RelativeUrl).UsingGet())
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(206)
                            .WithBodyFromFile(Path.Combine(MockMovieMediaPath, mockMediaData.ParentFolderName, mockMediaData.FileName))
                    );
            }

            // The default video used for testing
            _server
                .Given(Request.Create().WithPath("/media/movies/default").UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(206)
                        .WithBodyFromFile(Path.Combine(MockMovieMediaPath, "test-video.mp4"))
                );

            return _server;
        }
    }
}