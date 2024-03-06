using PlexRipper.Application;

namespace DownloadManager.UnitTests;

public class PlexDownloadClient_Setup_UnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        await SetupDatabase();

        // Act
        var result = await _sut.Setup(null);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDownloadTaskPlexServerIsNull()
    {
        //Arrange
        await SetupDatabase();

        var downloadTask = FakeData.GetMovieDownloadTask().Generate();
        downloadTask.PlexServer = null;

        // Act
        var result = await _sut.Setup(downloadTask.ToKey());

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
}