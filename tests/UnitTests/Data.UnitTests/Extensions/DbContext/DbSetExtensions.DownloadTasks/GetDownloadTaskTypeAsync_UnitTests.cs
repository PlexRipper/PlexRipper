using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests.Extensions.DbContext.DbSetExtensions.DownloadTasks;

public class GetDownloadTaskTypeAsync_UnitTests : BaseUnitTest
{
    public GetDownloadTaskTypeAsync_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeMovie_WhenTheGuidIsOfTypeDownloadTaskMovie()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.DisableForeignKeyCheck = true;
            config.MovieDownloadTasksCount = 5;
        });
        var downloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync();
        var testDownloadTask = downloadTasks[2];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Movie);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeTvShow_WhenTheGuidIsOfTypeDownloadTaskTvShow()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.DisableForeignKeyCheck = true;
            config.TvShowDownloadTasksCount = 2;
        });
        var downloadTasks = await IDbContext.DownloadTaskTvShow.ToListAsync();
        var testDownloadTask = downloadTasks[1];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.TvShow);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeSeason_WhenTheGuidIsOfTypeDownloadTaskTvShowSeason()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.DisableForeignKeyCheck = true;
            config.TvShowDownloadTasksCount = 3;
        });
        var downloadTasks = await IDbContext.DownloadTaskTvShowSeason.ToListAsync();
        var testDownloadTask = downloadTasks[2];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Season);
    }

    [Fact]
    public async Task ShouldReturnDownloadTaskTypeEpisode_WhenTheGuidIsOfTypeDownloadTaskTvShowEpisode()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.DisableForeignKeyCheck = true;
            config.TvShowDownloadTasksCount = 2;
        });
        var downloadTasks = await IDbContext.DownloadTaskTvShowEpisode.ToListAsync();
        var testDownloadTask = downloadTasks[0];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.Episode);
    }
}
