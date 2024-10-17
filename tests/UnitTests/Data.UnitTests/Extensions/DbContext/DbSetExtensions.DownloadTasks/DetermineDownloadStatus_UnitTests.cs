using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests;

public class DetermineDownloadStatus_UnitTests : BaseUnitTest
{
    public DetermineDownloadStatus_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeMovieDataToDownloadFinished_WhenTheMovieDataIsDownloadStatusIsDownloadFinished()
    {
        // Arrange
        await SetupDatabase(
            77674,
            config =>
            {
                config.MovieDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync();
        var testDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        // Act
        await IDbContext.DetermineDownloadStatus(testDownloadTask.ToKey());

        // Assert
        downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync();

        downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.DownloadFinished);
    }

    [Fact]
    public async Task ShouldSetTheDownloadTaskParentOfTypeEpisodeDataToError_WhenTheEpisodeDataIsDownloadStatusIsError()
    {
        // Arrange
        await SetupDatabase(
            864828,
            config =>
            {
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();

        var downloadTaskTvShowEpisodeFile = downloadTasks[3].Children[2].Children[3].Children[0];
        await IDbContext.SetDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey(), DownloadStatus.Error);

        // Act
        await IDbContext.DetermineDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey());

        // Assert
        var downloadTasksDb = await IDbContext.DownloadTaskTvShow.AsTracking().IncludeAll().ToListAsync();
        downloadTasksDb[3].DownloadStatus.ShouldBe(DownloadStatus.Error);
    }
}
