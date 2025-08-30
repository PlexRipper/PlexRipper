using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.BaseTests;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class StartDownloadTaskCommandUnitTests : BaseUnitTest<StartDownloadTaskCommandHandler>
{
    public StartDownloadTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(63209);

        // Act
        var result = await _sut.ExecuteAsync(new StartDownloadTaskCommand(Guid.Empty), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotPauseDownloadTasksInFileTransfer_WhenADownloadTaskIsAlreadyTransferring()
    {
        // Arrange
        await SetupDatabase(
            96318,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        var alreadyMergingTask = tvShowDownloadTasks.First();
        var pausedMergeTask = tvShowDownloadTasks.Last();

        alreadyMergingTask.SetDownloadStatus(DownloadStatus.Merging);
        pausedMergeTask.SetDownloadStatus(DownloadStatus.MergePaused);
        await dbContext.SaveChangesAsync(CancellationToken);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.IsDownloadTaskMerging(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await _sut.ExecuteAsync(new StartDownloadTaskCommand(pausedMergeTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotPauseTheActiveDownloads_WhenThatActiveDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            31917,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);
        var downloadingTask = lastDownloadTask.Children.First().Children.First().Children.First();
        downloadingTask.DownloadStatus = DownloadStatus.Downloading;
        await dbContext.SaveChangesAsync(CancellationToken);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        mock.SetupCommand(It.IsAny<PauseDownloadTaskCommand>).ReturnOk();
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey(), CancellationToken);
        for (var i = 0; i < downloadTasks.Count; i++)
        {
            if (i > 0)
            {
                downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        mock.VerifyEventPublished(It.IsAny<PauseDownloadTaskCommand>, Times.Never());
        mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }

    [Fact]
    public async Task ShouldPauseTheActiveDownloads_WhenAnotherDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            76276,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken.None);

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);

        var downloadingTask = lastDownloadTask.Children.ElementAt(0).Children.ElementAt(1).Children.ElementAt(0);

        downloadingTask.DownloadStatus = DownloadStatus.Downloading;
        await dbContext.SaveChangesAsync(CancellationToken);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        mock.SetupCommand(It.IsAny<PauseDownloadTaskCommand>)
            .ReturnsAsync(
                (PauseDownloadTaskCommand command, CancellationToken ct) =>
                {
                    var key = IDbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, ct).Result;
                    key.ShouldNotBeNull();
                    IDbContext.SetDownloadStatus(key, DownloadStatus.Queued).Wait(ct);
                    return Result.Ok();
                }
            );
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey(), CancellationToken);
        for (var i = 0; i < downloadTasks.Count; i++)
        {
            if (i > 0)
            {
                downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        mock.VerifyEventPublished(It.IsAny<PauseDownloadTaskCommand>, Times.Once());
        mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }
}
