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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        await SetupDatabase(84168, config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);

        SetupFileSystem();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);

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
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
    }

    [Fact]
    public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        // File deletion is now delegated to DeleteDownloadTaskFilesCommand — verify dispatch instead of filesystem state.
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        var downloadTasks = await dbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenDeleteDownloadTaskFilesCommandFails()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        await SetupDatabase(90427, config => config.MovieDownloadTasksCount = 1);

        var dbContext = IDbContext;
        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);
        var movieFileTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        movieFileTask.DownloadStatus = DownloadStatus.MoveError;
        await dbContext.SaveChangesAsync(CancellationToken);

        SetupFileSystem(fs => fs.AddFile(movieFileTask.DownloadFilePath, new MockFileData([])));

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("delete failed"))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(
                x =>
                    x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()),
                Times.Never()
            );
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );

        var after = await dbContext.GetDownloadTaskFileAsync(movieFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.MoveError);
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        // File deletion is delegated to DeleteDownloadTaskFilesCommand for the one actively downloading child.
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        // File deletion is delegated to DeleteDownloadTaskFilesCommand for the one actively downloading child only.
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(seasonTask.ToKey(), CancellationToken);
        downloadTasks.First(x => x.Id == seasonChildTasks.First().Id).DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        downloadTasks.First(x => x.Id == seasonChildTasks.Last().Id).DownloadStatus.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenStopMoveDownloadFileJobFails()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
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

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);

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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act — DeleteFiles=true; file deletion is delegated to DeleteDownloadTaskFilesCommand (handles missing file gracefully)
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert — handler must not fail; file deletion command is dispatched
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        // Both children (downloading + moving) are in the FileTransfer phase so file deletion is dispatched for each.
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(tvShow.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StopMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2)
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );
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
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnOk()
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

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
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        var downloadTasksAfter = await IDbContext.GetDownloadableChildTasks(movieTask.ToKey(), CancellationToken);
        foreach (var t in downloadTasksAfter)
            t.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldResetProgress_WhenFileTaskIsCompleted()
    {
        // Regression: stop must be a full reset operation, even for Completed tasks,
        // so restart has a clean state and cannot reuse stale DirectDownloadSnapshot data.
        // Arrange

        await SetupDatabase(73001, config => config.MovieDownloadTasksCount = 1);
        var dbContext = IDbContext;
        var fileTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        fileTask.DownloadStatus = DownloadStatus.Completed;
        fileTask.DataTotal = 300_000_000;
        fileTask.DataReceived = 300_000_000;
        fileTask.FileDataTransferred = 300_000_000;
        fileTask.CurrentFileTransferBytesOffset = 300_000_000;
        await dbContext.SaveChangesAsync(CancellationToken);

        SetupFileSystem();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(fileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        after.DataReceived.ShouldBe(0L);
        after.FileDataTransferred.ShouldBe(0L);
        after.CurrentFileTransferBytesOffset.ShouldBe(0L);
        after.DirectDownloadSnapshot.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldResetProgress_WhenFileTaskIsMoveFinished()
    {
        // Regression: stop must also fully reset MoveFinished tasks so a subsequent
        // restart always starts from a clean slate.
        // Arrange
        await SetupDatabase(73002, config => config.MovieDownloadTasksCount = 1);
        var dbContext = IDbContext;
        var fileTask = await dbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        fileTask.DownloadStatus = DownloadStatus.MoveFinished;
        fileTask.DataTotal = 200_000_000;
        fileTask.DataReceived = 200_000_000;
        fileTask.FileDataTransferred = 200_000_000;
        fileTask.CurrentFileTransferBytesOffset = 200_000_000;
        await dbContext.SaveChangesAsync(CancellationToken);

        SetupFileSystem(fs => fs.AddFile(fileTask.DownloadFilePath, new MockFileData([])));

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.AtLeastOnce);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new StopDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<DeleteDownloadTaskFilesCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        var after = await IDbContext.GetDownloadTaskFileAsync(fileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        after.DataReceived.ShouldBe(0L);
        after.FileDataTransferred.ShouldBe(0L);
        after.CurrentFileTransferBytesOffset.ShouldBe(0L);
        after.DirectDownloadSnapshot.ShouldBeNull();
    }
}
