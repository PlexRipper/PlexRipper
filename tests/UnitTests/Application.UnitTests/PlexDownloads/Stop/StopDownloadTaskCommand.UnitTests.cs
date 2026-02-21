using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class StopDownloadTaskCommandUnitTests : BaseUnitTest<StopDownloadTaskCommandHandler>
{
    public StopDownloadTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(72951);

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(Guid.Empty), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(
                x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never());
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenTheDownloadTaskCouldNotBeStopped()
    {
        // Arrange
        await SetupDatabase(84168, config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);

        SetupFileSystem();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(
            new StopDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Never);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Never);
    }

    [Fact]
    public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(90425, config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );

        var dbContext = IDbContext;
        var allMovieFileTasks = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        var movieFileTasks = allMovieFileTasks.Where(f => f.ParentId == movieDownloadTasks.First().Id).ToList();

        SetupFileSystem(fs =>
        {
            foreach (var fileTask in allMovieFileTasks)
                fs.AddFile(fileTask.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(
            new StopDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once);

        var file = Mock.Create<IFile>();
        Output.WriteLine($"movieFileTasks count: {movieFileTasks.Count}");
        foreach (var fileTask in movieFileTasks)
        {
            Output.WriteLine($"Checking: {fileTask.DownloadFilePath} exists={file.Exists(fileTask.DownloadFilePath)}");
            file.Exists(fileTask.DownloadFilePath).ShouldBeFalse();
        }

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(
            movieDownloadTasks.First().ToKey(),
            CancellationToken
        );
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldNotDeleteDownloadFile_WhenTaskIsInFileTransferPhase()
    {
        // Arrange — task is in MoveError (FileTransfer phase); the downloaded file must not be deleted
        await SetupDatabase(90426, config => config.MovieDownloadTasksCount = 1);

        var dbContext = IDbContext;
        var movieDownloadFileTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieDownloadFileTasks.SetDownloadStatus(DownloadStatus.MoveError);
        await dbContext.SaveChangesAsync(CancellationToken);

        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var fileTask in movieDownloadFileTasks)
                fs.AddFile(fileTask.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(
                x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once);

        // The downloaded file must still exist — FileTransfer-phase tasks preserve it
        var file = Mock.Create<IFile>();
        foreach (var fileTask in movieDownloadFileTasks)
            file.Exists(fileTask.DownloadFilePath).ShouldBeTrue();

        var downloadTasks = await dbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldHaveSetTvShowDownloadTasksToStop_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(
            81582,
            config =>
            {
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );
        var tvShowDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        var testDownloadTask = tvShowDownloadTasks.First();
        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(
            testDownloadTask.ToKey(),
            CancellationToken
        );

        downloadableTasks.Count.ShouldBe(4);

        var dbContext = IDbContext;
        var episodeFileTasks = await dbContext.DownloadTaskTvShowEpisodeFile.ToListAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var fileTask in episodeFileTasks)
                fs.AddFile(fileTask.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(
            new StopDownloadTaskCommand(tvShowDownloadTasks.First().Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Exactly(4));
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Exactly(4));

        // The first episode file the handler processes (IsDownloading=true) should be deleted; others are in default Queued phase
        var file = Mock.Create<IFile>();
        var firstProcessedFileTask = episodeFileTasks.First(f => f.Id == downloadableTasks.First().Id);
        file.Exists(firstProcessedFileTask.DownloadFilePath).ShouldBeFalse();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(
            tvShowDownloadTasks.First().ToKey(),
            CancellationToken
        );
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}
