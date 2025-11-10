using System.Diagnostics;

namespace Reaparr.BaseTests.UnitTests;

public class FakeApiDataGenerateUnitTests : BaseUnitTest<MockPlexApiServer>
{
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
        var stopWatch = Stopwatch.StartNew();
        Sut.Setup(handler, options);
        stopWatch.Stop();

        // Assert
        var elapsed = stopWatch.Elapsed;
        Output.WriteLine($"Elapsed time: {elapsed}");
        elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
    }
}
