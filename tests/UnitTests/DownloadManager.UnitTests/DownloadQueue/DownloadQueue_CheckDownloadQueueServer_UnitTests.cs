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

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<int>(), It.IsAny<int>())).ReturnOk();

        var plexServers = await DbContext.PlexServers
            .AsTracking()
            .Include(x => x.PlexLibraries)
            .ThenInclude(x => x.DownloadTasks)
            .ThenInclude(downloadTask => downloadTask.Children)
            .ToListAsync();

        var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
        startedDownloadTask.DownloadStatus = DownloadStatus.Downloading;
        startedDownloadTask.Children.SetToDownloading();
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
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks()
            .Where(x => x.ParentId == null)
            .Include(downloadTask => downloadTask.Children)
            .ToListAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<int>(), It.IsAny<int>())).ReturnOk();

        // Act
        var result = await _sut.CheckDownloadQueueServer(downloadTasks.First().PlexServerId);

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

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToListAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<int>(), It.IsAny<int>())).ReturnOk();

        // ** Set first task to Completed
        var movieDownloadTask = DbContext.DownloadTasks.Include(x => x.Children).AsTracking().First();
        movieDownloadTask.DownloadStatus = DownloadStatus.Completed;
        movieDownloadTask.Children.ForEach(x => x.DownloadStatus = DownloadStatus.Completed);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.CheckDownloadQueueServer(downloadTasks.First().PlexServerId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(7);
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

        var downloadTasks = await DbContext.DownloadTasks.IncludeDownloadTasks().IncludeByRoot().ToListAsync();
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<int>(), It.IsAny<int>())).ReturnOk();

        // ** Set first task to Completed
        var tvShowDownloadTask = downloadTasks[0];
        tvShowDownloadTask.DownloadStatus = DownloadStatus.Completed;
        tvShowDownloadTask.Children = tvShowDownloadTask.Children.SetToCompleted();
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(downloadTasks[1].Children[0].Children[0].Children[0].Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenNestedInsideATvShowsDownloadTasksWithDownloadFinished()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 2;
            config.TvShowSeasonDownloadTasksCount = 2;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.DownloadTasks.AsTracking().IncludeDownloadTasks().IncludeByRoot().ToListAsync();
        downloadTasks.SetToDownloadFinished();

        foreach (var tvShowDownloadTask in downloadTasks)
            tvShowDownloadTask.Children[0].Children[1].DownloadStatus = DownloadStatus.Queued;

        await DbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<int>(), It.IsAny<int>())).ReturnOk();

        // Act
        var result = await _sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(downloadTasks[0].Children[0].Children[1].Children[0].Id);
    }
}