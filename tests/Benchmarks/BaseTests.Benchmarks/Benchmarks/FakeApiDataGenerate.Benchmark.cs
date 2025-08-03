using BenchmarkDotNet.Attributes;
using Moq;
using PlexRipper.BaseTests;
using Serilog.Events;
using Xunit;

namespace BaseTests.Benchmarks;

public class FakeApiDataGenerateBenchmark : BaseUnitTest
{
    protected FakeApiDataGenerateBenchmark(ITestOutputHelper output, LogEventLevel logEventLevel)
        : base(output, logEventLevel) { }

    [Benchmark]
    public void ShouldRunTheBenchmarkOnGeneratingMockData_WhenOptionsIsConfigured()
    {
        SetupDatabase(999);

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        var sut = new MockPlexApiServer(IDbContext);

        Action<PlexApiDataConfig> options = x =>
        {
            x.PlexServerAccessCount = 1;
            x.MovieLibraryCount = 1;
            x.MoviesPerLibraryCount = 500;
        };

        sut.Setup(handler, options);
    }
}
