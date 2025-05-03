using BenchmarkDotNet.Attributes;
using Moq;
using PlexRipper.BaseTests;

namespace BaseTests.Benchmarks;

public class FakeApiDataGenerateBenchmark
{
    [Benchmark]
    public void ShouldRunTheBenchmarkOnGeneratingMockData_WhenOptionsIsConfigured()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        var sut = new MockPlexApiServer();

        Action<PlexApiDataConfig> options = x =>
        {
            x.PlexServerAccessCount = 1;
            x.MovieLibraryCount = 1;
            x.MoviesPerLibraryCount = 500;
        };

        sut.Setup(handler, options);
    }
}
