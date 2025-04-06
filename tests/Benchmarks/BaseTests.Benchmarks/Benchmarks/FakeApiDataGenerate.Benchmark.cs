using BenchmarkDotNet.Attributes;
using Moq;
using PlexRipper.BaseTests;

namespace BaseTests.Benchmarks;

public class FakeApiDataGenerateBenchmark
{
    private Mock<HttpMessageHandler> _handler;
    private MockPlexApiServer _sut;

    public void Setup()
    {
        _handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        _sut = new MockPlexApiServer();
    }

    [Benchmark]
    public void ShouldRunTheBenchmarkOnGeneratingMockData_WhenOptionsIsConfigured()
    {
        Action<PlexApiDataConfig> options = x =>
        {
            x.PlexServerAccessCount = 1;
            x.MovieLibraryCount = 1;
            x.MoviesPerLibraryCount = 500;
        };

        _sut.Setup(_handler, options);
    }
}
