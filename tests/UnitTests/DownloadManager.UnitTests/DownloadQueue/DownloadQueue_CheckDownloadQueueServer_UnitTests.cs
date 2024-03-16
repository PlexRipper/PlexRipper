using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadQueue_CheckDownloadQueue_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_CheckDownloadQueue_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveNoUpdates_WhenGivenAnEmptyList()
    {
        // Arrange
        await SetupDatabase();

        // Act
        _sut.Setup();
        var result = await _sut.CheckDownloadQueue(new List<int>());

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTaskMovie.AsTracking().Where(x => x.PlexServerId == 1).IncludeAll().ToListAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<Guid>(), It.IsAny<int>())).ReturnOk();

        var startedDownloadTask = downloadTasks[0];
        startedDownloadTask.SetDownloadStatus(DownloadStatus.Downloading);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
    {
        // Arrange
        Seed = 5000;
        await SetupDatabase(config =>
        {
            config.MovieCount = 2;
            config.MovieDownloadTasksCount = 2;
        });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<Guid>(), It.IsAny<int>())).ReturnOk();

        // Act
        var result = await _sut.CheckDownloadQueueServer(downloadTasks[0].PlexServerId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var startedDownloadTask = downloadTasks[0].Children[0];
        result.Value.Id.ShouldBe(startedDownloadTask.Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenAMovieDownloadTasksWithCompleted()
    {
        // Arrange
        Seed = 5000;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await DbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<Guid>(), It.IsAny<int>())).ReturnOk();

        // Act
        var result = await _sut.CheckDownloadQueueServer(downloadTasks[0].PlexServerId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        downloadTasks[0].Children[0].DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
        result.Value.Id.ShouldBe(downloadTasks[0].Children[0].Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenATvShowsDownloadTasksWithCompleted()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 2;
            config.TvShowSeasonDownloadTasksCount = 2;
            config.TvShowEpisodeDownloadTasksCount = 2;
        });
        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await DbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<Guid>(), It.IsAny<int>())).ReturnOk();

        // Act
        var result = await _sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(downloadTasks[0].Children[0].Children[0].Children[0].Id);
    }
}