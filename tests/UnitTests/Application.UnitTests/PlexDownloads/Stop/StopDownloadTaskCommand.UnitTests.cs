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
    public async Task ShouldDeleteDownloadFile_WhenTaskIsInFileTransferPhase()
    {
        // Arrange — task is in MoveError (FileTransfer phase); stopping deletes temp files
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

        // The downloaded file should be deleted on stop
        var file = Mock.Create<IFile>();
        foreach (var fileTask in movieDownloadFileTasks)
            file.Exists(fileTask.DownloadFilePath).ShouldBeFalse();

        var downloadTasks = await dbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldOnlyStopActiveTvShowChildren_WhenStoppingTvShow()
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
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once);

        // The first episode file the handler processes (IsDownloading=true) should be deleted; others are in default Queued phase
        var file = Mock.Create<IFile>();
        var firstProcessedFileTask = episodeFileTasks.First(f => f.Id == downloadableTasks.First().Id);
        file.Exists(firstProcessedFileTask.DownloadFilePath).ShouldBeFalse();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(
            tvShowDownloadTasks.First().ToKey(),
            CancellationToken
        );

        var stoppedTaskIds = downloadTasks
            .Where(x => x.DownloadStatus == DownloadStatus.Stopped)
            .Select(x => x.Id)
            .ToList();
        stoppedTaskIds.Count.ShouldBe(1);
        stoppedTaskIds.ShouldContain(downloadableTasks.First().Id);

        downloadTasks
            .Where(x => x.Id != downloadableTasks.First().Id)
            .All(x => x.DownloadStatus == DownloadStatus.Queued)
            .ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldOnlyStopActiveSeasonChildren_WhenStoppingSeason()
    {
        // Arrange
        await SetupDatabase(
            52814,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var seasonTask = await IDbContext.DownloadTaskTvShowSeason.FirstAsync(CancellationToken);
        var seasonChildTasks = await IDbContext.GetDownloadableChildTaskKeys(seasonTask.ToKey(), CancellationToken);

        seasonChildTasks.Count.ShouldBe(2);

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
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(seasonTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Exactly(2));
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once);

        var file = Mock.Create<IFile>();
        var firstProcessedFileTask = episodeFileTasks.First(f => f.Id == seasonChildTasks.First().Id);
        var secondProcessedFileTask = episodeFileTasks.First(f => f.Id == seasonChildTasks.Last().Id);
        file.Exists(firstProcessedFileTask.DownloadFilePath).ShouldBeFalse();
        file.Exists(secondProcessedFileTask.DownloadFilePath).ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(seasonTask.ToKey(), CancellationToken);
        downloadTasks.First(x => x.Id == seasonChildTasks.First().Id).DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        downloadTasks.First(x => x.Id == seasonChildTasks.Last().Id).DownloadStatus.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenStopMoveDownloadFileJobFails()
    {
        // Arrange
        await SetupDatabase(72952, config => config.MovieDownloadTasksCount = 1);
        var movieTask = await IDbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        SetupFileSystem();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(true);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Fail("Move stop error"));
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Never);
    }

    [Fact]
    public async Task ShouldNotDeleteFiles_WhenDeleteFilesIsFalse()
    {
        // Arrange
        await SetupDatabase(72953, config => config.MovieDownloadTasksCount = 1);
        var dbContext = IDbContext;
        var movieFileTasks = await dbContext.DownloadTaskMovieFile.ToListAsync(CancellationToken);
        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var fileTask in movieFileTasks)
                fs.AddFile(fileTask.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act — pass DeleteFiles: false explicitly
        var result = await Sut.ExecuteAsync(
            new StopDownloadTaskCommand(movieTask.Id, DeleteFiles: false),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        foreach (var fileTask in movieFileTasks)
            file.Exists(fileTask.DownloadFilePath).ShouldBeTrue();

        var downloadTasksAfter = await IDbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var t in downloadTasksAfter)
            t.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldNotDeleteFiles_WhenDownloadTaskPhaseIsCompleted()
    {
        // Arrange — completed task: DeleteFiles=true but phase guard must prevent deletion
        await SetupDatabase(72954, config => config.MovieDownloadTasksCount = 1);
        var dbContext = IDbContext;
        var movieFileTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieFileTasks.SetDownloadStatus(DownloadStatus.Completed);
        await dbContext.SaveChangesAsync(CancellationToken);

        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var fileTask in movieFileTasks)
                fs.AddFile(fileTask.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act — DeleteFiles defaults to true, but completed phase should block deletion
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        foreach (var fileTask in movieFileTasks)
            file.Exists(fileTask.DownloadFilePath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotFail_WhenDownloadFileDoesNotExistOnDisk()
    {
        // Arrange — file is missing from disk; stop should still succeed and log a warning
        await SetupDatabase(72955, config => config.MovieDownloadTasksCount = 1);
        var movieTask = await IDbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        // Empty filesystem — no files present
        SetupFileSystem();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act — DeleteFiles=true but the file simply doesn't exist
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert — handler must not fail; it logs a warning and continues
        result.IsSuccess.ShouldBeTrue();
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once);
    }

    [Fact]
    public async Task ShouldStopBothDownloadingAndMovingChildren_WhenTvShowHasOneChildDownloadingAndOneMoving()
    {
        // Arrange
        await SetupDatabase(
            72956,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var tvShow = await IDbContext.DownloadTaskTvShow.FirstAsync(CancellationToken);
        var childKeys = await IDbContext.GetDownloadableChildTaskKeys(tvShow.ToKey(), CancellationToken);
        childKeys.Count.ShouldBe(2);

        var episodeFileTasks = await IDbContext.DownloadTaskTvShowEpisodeFile.ToListAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var t in episodeFileTasks)
                fs.AddFile(t.DownloadFilePath, new MockFileData([]));
        });

        // First child is downloading; second child is moving
        Mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        Mock.Mock<IMoveDownloadFileScheduler>()
            .SetupSequence(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnOk();
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(tvShow.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Exactly(2));

        var downloadTasksAfter = await IDbContext.GetDownloadableChildTasks(tvShow.ToKey(), CancellationToken);
        downloadTasksAfter.Count(x => x.DownloadStatus == DownloadStatus.Stopped).ShouldBe(2);
    }

    [Fact]
    public async Task ShouldStopMovingTask_WhenMovieTaskIsMovingAndNotDownloading()
    {
        // Arrange
        await SetupDatabase(72957, config => config.MovieDownloadTasksCount = 1);
        var dbContext = IDbContext;
        var movieFileTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieFileTasks.SetDownloadStatus(DownloadStatus.Moving);
        await dbContext.SaveChangesAsync(CancellationToken);

        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        SetupFileSystem(fs =>
        {
            foreach (var t in movieFileTasks)
                fs.AddFile(t.DownloadFilePath, new MockFileData([]));
        });

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(true);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnOk();
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(
                x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);

        var downloadTasksAfter = await IDbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var t in downloadTasksAfter)
            t.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}
