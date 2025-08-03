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

        var downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync(CancellationToken);
        var testDownloadTask = downloadTasks.First().Children.First();
        await IDbContext.SetDownloadStatus(testDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        // Act
        await IDbContext.DetermineDownloadStatus(testDownloadTask.ToKey(), CancellationToken);

        // Assert
        downloadTasks = await IDbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync(CancellationToken);

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

        var downloadTasks = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync(CancellationToken);

        var downloadTaskTvShowEpisodeFile = downloadTasks
            .ElementAt(3)
            .Children.ElementAt(2)
            .Children.ElementAt(3)
            .Children.ElementAt(0);

        await IDbContext.SetDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey(), DownloadStatus.Error);

        // Act
        await IDbContext.DetermineDownloadStatus(downloadTaskTvShowEpisodeFile.ToKey(), CancellationToken);

        // Assert
        var downloadTasksDb = await IDbContext
            .DownloadTaskTvShow.AsTracking()
            .IncludeAll()
            .ToListAsync(CancellationToken);
        downloadTasksDb[3].DownloadStatus.ShouldBe(DownloadStatus.Error);
    }
}
