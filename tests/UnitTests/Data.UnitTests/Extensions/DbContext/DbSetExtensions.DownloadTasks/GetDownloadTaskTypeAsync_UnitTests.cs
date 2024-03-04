using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests.Extensions.DbContext.DbSetExtensions.DownloadTasks;

public class GetDownloadTaskTypeAsync_UnitTests : BaseUnitTest
{
    public GetDownloadTaskTypeAsync_UnitTests(ITestOutputHelper output) : base(output) { }

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
            config.TvShowDownloadTasksCount = 5;
        });
        var downloadTasks = await IDbContext.DownloadTaskTvShow.ToListAsync();
        var testDownloadTask = downloadTasks[3];

        // Act
        var downloadTaskType = await IDbContext.GetDownloadTaskTypeAsync(testDownloadTask.Id);

        // Assert
        downloadTaskType.ShouldBe(DownloadTaskType.TvShow);
    }
}