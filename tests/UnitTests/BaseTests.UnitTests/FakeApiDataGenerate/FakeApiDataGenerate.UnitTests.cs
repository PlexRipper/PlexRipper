using System.Diagnostics;

namespace BaseTests.UnitTests;

public class FakeApiDataGenerateUnitTests : BaseUnitTest<MockPlexApiServer>
{
    private Stopwatch _stopwatch = new Stopwatch();

    public FakeApiDataGenerateUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldGeneratePlexApiMockDataFast_WhenGeneratingALargeDataset()
    {
        // Arrange
        Action<PlexApiDataConfig> options = x =>
        {
            x.PlexServerAccessCount = 1;
            x.MovieLibraryCount = 3;
            x.MoviesPerLibraryCount = 5000;
        };
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

        // Act
        _stopwatch.Start();
        _sut.Setup(handler, options);
        _stopwatch.Stop();

        // Assert
        var elapsed = _stopwatch.Elapsed;
        _output.WriteLine($"Elapsed time: {elapsed}");
        elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
    }
}
