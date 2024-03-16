using Data.Contracts;

namespace Data.UnitTests.Extensions.DbContext.DbSetExtensions.DownloadTasks;

public class CalculateDownloadStatus_UnitTests : BaseUnitTest
{
    public CalculateDownloadStatus_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeMovieDataToDownloadFinished_WhenTheMovieDataIsDownloadStatusIsDownloadFinished()
    {
        // Arrange
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 5; });

        var downloadTasks = await IDbContext.GetAllDownloadTasksAsync(asTracking: true);
        var testDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        // Act
        await IDbContext.DetermineDownloadStatus(testDownloadTask.ToKey());

        // Assert
        downloadTasks = await IDbContext.GetAllDownloadTasksAsync(asTracking: true);

        downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.DownloadFinished);
    }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeEpisodeDataToError_WhenTheEpisodeDataIsDownloadStatusIsError()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonCount = 5;
            config.TvShowEpisodeCount = 5;
        });

        var downloadTasks = await IDbContext.GetAllDownloadTasksAsync(asTracking: true);
        var testDownloadTask = downloadTasks[3].Children[2].Children[3].Children[0];
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.Error);

        // Act
        await IDbContext.DetermineDownloadStatus(testDownloadTask.ToKey());

        // Assert
        downloadTasks = await IDbContext.GetAllDownloadTasksAsync(asTracking: true);

        downloadTasks[3].DownloadStatus.ShouldBe(DownloadStatus.Error);
    }
}