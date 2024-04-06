using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests.Execute.Notification;

public class DownloadTaskUpdatedHandler_UnitTests : BaseUnitTest<DownloadTaskUpdatedHandler>
{
    public DownloadTaskUpdatedHandler_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldSendDownloadTasksWithSignalR_WhenDownloadTaskUpdatedHasBeenCalled()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        var downloadTasks = await IDbContext.GetAllDownloadTasksAsync();
        mock.Mock<ISignalRService>()
            .Setup(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedNotification(downloadTasks[0].ToKey());
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        mock.Mock<ISignalRService>()
            .Verify(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldStartFileMergeJobAndDownloadQueue_WhenDownloadTaskHasFinished()
    {
        // Arrange
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 5; });

        var downloadTasks = await IDbContext.GetAllDownloadTasksAsync();
        var updatedDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(updatedDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        mock.Mock<ISignalRService>()
            .Setup(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.CreateFileTaskFromDownloadTask(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok(new FileTask()));

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<int>()))
            .ReturnsAsync(Result.Ok());

        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedNotification(downloadTasks[0].ToKey());
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        mock.Mock<ISignalRService>()
            .Verify(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyNotification(It.IsAny<CheckDownloadQueueNotification>, Times.Once);
    }

    [Fact]
    public async Task ShouldUpdateTvShowParentDownloadStatus_WhenDownloadTaskEpisodeHasUpdatedItsStatus()
    {
        // Arrange
        await SetupDatabase(config => { config.TvShowDownloadTasksCount = 1; });

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskTvShow.AsTracking().IncludeAll().ToListAsync();
        var tvShowDownloadTask = downloadTasks.First();
        foreach (var seasonDownloadTask in tvShowDownloadTask.Children)
        foreach (var episodeDownloadTask in seasonDownloadTask.Children)
        foreach (var episodeDownloadTaskFile in episodeDownloadTask.Children)
            episodeDownloadTaskFile.DownloadStatus = DownloadStatus.Completed;

        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Refresh the tvShowDownloadTask

        mock.Mock<ISignalRService>()
            .Setup(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.CreateFileTaskFromDownloadTask(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok(new FileTask()));

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<int>()))
            .ReturnsAsync(Result.Ok());

        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var updatedDownloadTask = tvShowDownloadTask.Children[0].Children[0].Children[0];
        var command = new DownloadTaskUpdatedNotification(updatedDownloadTask.ToKey());
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tvShowDownloadTaskDb = await IDbContext.GetDownloadTaskAsync(tvShowDownloadTask.ToKey());
        tvShowDownloadTaskDb.ShouldNotBeNull();
        tvShowDownloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
        foreach (var seasonDownloadTask in tvShowDownloadTaskDb.Children)
        {
            seasonDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
            foreach (var episodeDownloadTask in seasonDownloadTask.Children)
            {
                episodeDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Completed);
                foreach (var episodeDownloadTaskFile in episodeDownloadTask.Children)
                    episodeDownloadTaskFile.DownloadStatus.ShouldBe(DownloadStatus.Completed);
            }
        }

        mock.Mock<ISignalRService>()
            .Verify(x => x.SendDownloadProgressUpdateAsync(It.IsAny<int>(), It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}