using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PlexRipper.Domain;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PlexRipper.BaseTests
{
    public class MockServer : IMockServer
    {
        private static readonly List<MockMediaData> _mockMediaData = new();

        public string MockMovieMediaPath
        {
            get
            {
                var x = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                return x.Single(str => str.EndsWith("test-video.mp4"));
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
            return new MockMediaData(PlexMediaType.Movie, MockMovieMediaPath);
        }

        public WireMockServer GetPlexMockServer()
        {
            var _server = WireMockServer.Start();

            // foreach (var mockMediaData in GetMockMediaData())
            // {
            //     _server
            //         .Given(Request.Create().WithPath(mockMediaData.RelativeUrl).UsingGet())
            //         .RespondWith(
            //             Response.Create()
            //                 .WithStatusCode(206)
            //                 .WithBodyFromFile(Path.Combine(MockMovieMediaPath, mockMediaData.ParentFolderName, mockMediaData.FileName))
            //         );
            // }

            // The default video used for testing
            _server
                .Given(Request.Create().WithPath("/media/movies/default").UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(206)
                        .WithBodyFromFile(MockMovieMediaPath)
                );

            return _server;
        }
    }
}