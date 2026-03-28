using ByteSizeLib;

namespace Reaparr.BaseTests.UnitTests;

public class FakeDataPlexMediaDataUnitTests : BaseUnitTest
{
    [Test]
    public void PlexMovieMediaData_ShouldUseConfiguredDownloadFileSize_WhenProvided()
    {
        // Arrange
        var seed = new Seed(24680);
        const int configuredSizeMb = 128;

        // Act
        var mediaData = FakeData
            .GetPlexMovieMediaData(seed, options => options.DownloadFileSizeInMb = configuredSizeMb)
            .Generate();

        // Assert
        mediaData.Size.ShouldBe((long)ByteSize.FromMebiBytes(configuredSizeMb).Bytes);
    }

    [Test]
    public void PlexTvShowEpisodeMediaData_ShouldUseConfiguredDownloadFileSize_WhenProvided()
    {
        // Arrange
        var seed = new Seed(13579);
        const int configuredSizeMb = 64;

        // Act
        var mediaData = FakeData
            .GetPlexTvShowEpisodeMediaData(seed, options => options.DownloadFileSizeInMb = configuredSizeMb)
            .Generate();

        // Assert
        mediaData.Size.ShouldBe((long)ByteSize.FromMebiBytes(configuredSizeMb).Bytes);
    }
}
