using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlexRipper.Domain;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MockPlexApiServer
{
    public static class MockServer
    {
        private static readonly List<MockMediaData> _mockMediaData = new();

        public static string MockMovieMediaPath => Path.Combine(FileSystemPaths.RootDirectory, "media", "movies");

        public static List<MockMediaData> GetMockMediaData()
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

        public static WireMockServer GetPlexMockServer()
        {
            var _server = WireMockServer.Start();

            var moviePath = $@"{FileSystemPaths.RootDirectory}/media/movies";

            foreach (var mockMediaData in GetMockMediaData())
            {
                _server
                    .Given(Request.Create().WithPath(mockMediaData.RelativeUrl).UsingGet())
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(206)
                            .WithBodyFromFile(Path.Combine(moviePath, mockMediaData.ParentFolderName, mockMediaData.FileName))
                    );
            }

            return _server;
        }
    }
}