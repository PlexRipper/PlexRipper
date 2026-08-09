using System.Reactive.Subjects;
using ByteSizeLib;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileFromFileTaskCommandUnitTests : BaseUnitTest<MoveDownloadFileFromFileTaskCommandHandler>
{
    [Test]
    public void MoveDownloadFileFromFileTaskCommandValidator_ShouldRejectNullKey()
    {
        // Arrange
        var validator = new MoveDownloadFileFromFileTaskCommandValidator();
        var command = new MoveDownloadFileFromFileTaskCommand(null!);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(MoveDownloadFileFromFileTaskCommand.Key));
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenDirectoryNameIsEmpty()
    {
        // Arrange
        await SetupDatabase(
            10001,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldBeAbleToPauseTheDownloadTask_WhenCancellationTokenIsCalled()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress
            .AsObservable()
            .Subscribe(x =>
            {
                progressList.Add(x);

                // Cancel shortly after the transfer starts
                if (
                    x.FileDataTransferred > (long)ByteSize.FromMebiBytes(1).Bytes
                    && !cancellationTokenSource.Token.IsCancellationRequested
                )
                {
                    cancellationTokenSource.CancelAsync();
                }
            });

        var content = new byte[fileSizeInMb * 1024 * 1024];
        new Random(2).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        // Will publish if an error occurs
        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .Returns<MoveFileWithResumeCommand, CancellationToken>(
                (moveCommand, _) =>
                {
                    moveCommand.Progress(
                        new MoveFileTransferProgressDTO
                        {
                            Transferred = content.LongLength / 2,
                            DataTotal = content.LongLength,
                            FileTransferSpeed = 1024,
                        }
                    );

                    cancellationTokenSource.Cancel();
                    return Task.FromResult(Result.Ok());
                }
            )
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);

        // Source should remain since operation paused before completion
        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldSetMoveFinished_WhenCancellationIsRequestedAfterMoveCommandReturnsSuccess()
    {
        // Arrange
        await SetupDatabase(
            52224,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var cancellationTokenSource = new CancellationTokenSource();
        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[2 * 1024 * 1024];
        new Random(13).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .ReturnsAsync(() =>
            {
                cancellationTokenSource.Cancel();
                return Result.Ok();
            })
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            cancellationTokenSource.Token
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MovePaused),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
    }

    [Test]
    public async Task ShouldEmitProgressAndCompleteSubject_WhenCancellationIsRequestedAfterMoveCommandReturnsSuccess()
    {
        // Arrange
        await SetupDatabase(
            52226,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var cancellationTokenSource = new CancellationTokenSource();
        var progress = new Subject<IDownloadFileTransferProgress>();
        var progressUpdates = new List<IDownloadFileTransferProgress>();
        var wasCompleted = false;

        progress.AsObservable().Subscribe(x => progressUpdates.Add(x), _ => { }, () => wasCompleted = true);

        var content = new byte[2 * 1024 * 1024];
        new Random(14).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .Returns<MoveFileWithResumeCommand, CancellationToken>(
                (moveCommand, _) =>
                {
                    moveCommand.Progress(
                        new MoveFileTransferProgressDTO
                        {
                            Transferred = content.LongLength / 2,
                            DataTotal = content.LongLength,
                            FileTransferSpeed = 1024,
                        }
                    );

                    cancellationTokenSource.Cancel();
                    return Task.FromResult(Result.Ok());
                }
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            cancellationTokenSource.Token
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressUpdates.Count.ShouldBe(1);
        wasCompleted.ShouldBeTrue();

        var fileTaskAfterPause = await dbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskAfterPause.ShouldNotBeNull();
        progressUpdates[0].FileDataTransferred.ShouldBe(fileTaskAfterPause.FileDataTransferred);
        fileTaskAfterPause.FileDataTransferred.ShouldBeGreaterThan(0);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MovePaused),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
    }

    [Test]
    public async Task ShouldBeAbleToResumeAndFinish_WhenPreviousFileTaskHasBeenPaused()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        downloadFileTask.CurrentFileTransferBytesOffset = 2348;
        await dbContext.SaveChangesAsync(CancellationToken);

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        var content = new byte[fileSizeInMb * 1024 * 1024];
        new Random(3).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .Returns<MoveFileWithResumeCommand, CancellationToken>(
                (moveCommand, _) =>
                {
                    moveCommand.Progress(
                        new MoveFileTransferProgressDTO
                        {
                            Transferred = moveCommand.DataTotal,
                            DataTotal = moveCommand.DataTotal,
                            FileTransferSpeed = moveCommand.DataTotal,
                        }
                    );

                    return Task.FromResult(Result.Ok());
                }
            )
            .Verifiable(Times.Once);
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await dbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task ShouldCreateProgressResourcesPerExecution_WhenSameHandlerExecutesMultipleMoves()
    {
        // Arrange
        await SetupDatabase(
            52225,
            config =>
            {
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTasks = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .Take(2)
            .ToListAsync(CancellationToken);

        downloadFileTasks.Count.ShouldBe(2);

        var firstTask = downloadFileTasks[0];
        var secondTask = downloadFileTasks[1];

        var firstContent = new byte[2 * 1024 * 1024];
        new Random(41).NextBytes(firstContent);

        var secondContent = new byte[2 * 1024 * 1024];
        new Random(42).NextBytes(secondContent);

        SetupFileSystem(fs =>
        {
            fs.AddFile(firstTask.DownloadFilePath, new MockFileData(firstContent));
            fs.AddFile(secondTask.DownloadFilePath, new MockFileData(secondContent));
        });

        firstTask.DataTotal = firstContent.LongLength;
        secondTask.DataTotal = secondContent.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        var firstProgress = new Subject<IDownloadFileTransferProgress>();
        var secondProgress = new Subject<IDownloadFileTransferProgress>();

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(4));
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.NotifyFileTransferProgress(It.IsAny<DownloadTaskKey>()))
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IReaparrDbContextFactory>()
            .SetupSequence(x => x.Create())
            .Returns(() => IDbContext)
            .Returns(() => IDbContext);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<MoveFileWithResumeCommand, CancellationToken>(
                async (moveCommand, cancellationToken) =>
                {
                    await Task.Delay(1100, cancellationToken);
                    moveCommand.Progress(
                        new MoveFileTransferProgressDTO
                        {
                            Transferred = moveCommand.DataTotal,
                            DataTotal = moveCommand.DataTotal,
                            FileTransferSpeed = moveCommand.DataTotal,
                        }
                    );

                    return Result.Ok();
                }
            );

        var sut = Sut;

        // Act
        var firstResult = await sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(firstTask.ToKey(), firstProgress),
            CancellationToken
        );

        var secondResult = await sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(secondTask.ToKey(), secondProgress),
            CancellationToken
        );

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();

        Mock.Mock<IReaparrDbContextFactory>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();

        var updatedTasks = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .Take(2)
            .ToListAsync(CancellationToken);
        updatedTasks.ShouldAllBe(x => x.FileDataTransferred == x.DataTotal);
    }

    [Test]
    public async Task ShouldRenameAndComplete_WhenKeepCompletedInDownloadsIsTrue()
    {
        // Arrange
        await SetupDatabase(
            334455,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[2 * 1024 * 1024];
        new Random(11).NextBytes(content);

        SetupFileSystem(fs =>
        {
            // Source exists with .reaptemp suffix
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Keep in downloads triggers rename flow, not MoveFileWithResume
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(true);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeFalse();
        file.Exists(downloadFileTask.DownloadFilePath.RemoveReapTempSuffix()).ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldRenameAndComplete_WhenDestinationMatchesSourceWithoutReapTempSuffix()
    {
        // Arrange
        await SetupDatabase(
            334456,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var inPlaceDestinationRootPath = Path.Combine(downloadFileTask.DirectoryMeta.DownloadRootPath, "TvShows");
        dbContext.Entry(downloadFileTask).Property(nameof(downloadFileTask.DirectoryMeta)).CurrentValue =
            downloadFileTask.DirectoryMeta with
            {
                DestinationRootPath = inPlaceDestinationRootPath,
            };
        dbContext.Entry(downloadFileTask).Property(nameof(downloadFileTask.DirectoryMeta)).IsModified = true;

        var progress = new Subject<IDownloadFileTransferProgress>();
        var content = new byte[2 * 1024 * 1024];
        new Random(12).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        var persistedTask = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        persistedTask.DownloadFilePath.RemoveReapTempSuffix().ShouldBe(persistedTask.DestinationFilePath);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeFalse();
        file.Exists(downloadFileTask.DestinationFilePath).ShouldBeTrue();

        var after = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldRenameAndComplete_WhenInPlaceDestinationMatchesSourceWithoutReapTempSuffix()
    {
        // Arrange
        await SetupDatabase(
            334457,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var inPlaceDestinationRootPath = Path.Combine(downloadFileTask.DirectoryMeta.DownloadRootPath, "TvShows");
        dbContext.Entry(downloadFileTask).Property(nameof(downloadFileTask.DirectoryMeta)).CurrentValue =
            downloadFileTask.DirectoryMeta with
            {
                DestinationRootPath = inPlaceDestinationRootPath,
            };
        dbContext.Entry(downloadFileTask).Property(nameof(downloadFileTask.DirectoryMeta)).IsModified = true;

        var content = new byte[2 * 1024 * 1024];
        new Random(63).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        var persistedTask = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        persistedTask.DownloadFilePath.RemoveReapTempSuffix().ShouldBe(persistedTask.DestinationFilePath);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey()),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeFalse();
        file.Exists(downloadFileTask.DestinationFilePath).ShouldBeTrue();

        var after = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenDownloadTaskKeyDoesNotExist()
    {
        // Arrange
        await SetupDatabase(
            778899,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var existing = await IDbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .FirstOrDefaultAsync(CancellationToken);
        existing.ShouldNotBeNull();

        var missingKey = new DownloadTaskKey
        {
            Type = existing.DownloadTaskType,
            Id = Guid.NewGuid(),
            PlexServerId = existing.PlexServerId,
            PlexLibraryId = existing.PlexLibraryId,
        };

        var progress = new Subject<IDownloadFileTransferProgress>();
        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(missingKey, progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFailedResultAndPublishNotification_WhenSourceFileDoesNotExist()
    {
        // Arrange
        await SetupDatabase(
            991122,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        // No source file added
        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveError),
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldResumeMove_WhenReapTempFileIsMissingButRenamedFileExistsInDownloads()
    {
        // Arrange
        // Simulates the crash/restart scenario: .reaptemp was renamed but the file was never
        // moved to the destination. The handler should fall through and move the file instead
        // of incorrectly declaring the move finished.
        await SetupDatabase(
            556677,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var renamedInDownloadsPath = downloadFileTask.DownloadFilePath.RemoveReapTempSuffix();

        var content = new byte[2 * 1024 * 1024];
        new Random(42).NextBytes(content);

        SetupFileSystem(fs =>
        {
            // .reaptemp file is gone, only the renamed file exists — not yet at destination
            fs.AddFile(renamedInDownloadsPath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        var progress = new Subject<IDownloadFileTransferProgress>();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldTreatMoveAsFinished_WhenReapTempFileIsMissingAndRenamedFileExistsInDownloadsWithKeepInDownloads()
    {
        // Arrange
        // Simulates the same crash/restart scenario but with keep-in-downloads enabled.
        // The renamed file in downloads is the final resting place, so move is finished.
        await SetupDatabase(
            667788,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var renamedInDownloadsPath = downloadFileTask.DownloadFilePath.RemoveReapTempSuffix();

        var content = new byte[2 * 1024 * 1024];
        new Random(43).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(renamedInDownloadsPath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(true);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        var progress = new Subject<IDownloadFileTransferProgress>();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldTreatMoveAsFinished_WhenSourceIsMissingAndDestinationExists()
    {
        // Arrange
        await SetupDatabase(
            889900,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var content = new byte[1024 * 1024];
        new Random(61).NextBytes(content);

        SetupFileSystem(fs =>
        {
            // Source .reaptemp is missing, destination already exists.
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        // Reconciliation must finish from durable filesystem state even if no move is required. The
        // handler should not be invoked with an already-cancelled token now that cancellation is
        // propagated through its initial database lookup.
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey()),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var after = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveFinished),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldUpdateFileTransferPercentageToOneHundred_WhenMoveCompletesSuccessfully()
    {
        // Arrange — verifies that UpdateDownloadFileTransferProgress now writes Percentage to the DB,
        // fixing the bug where the column kept the stale download-phase value (e.g. 75%) after the move.
        await SetupDatabase(
            777001,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        // Simulate a stale Percentage left over from the download phase
        downloadFileTask.Percentage = 75m;
        downloadFileTask.DataTotal = 5 * 1024 * 1024;
        await dbContext.SaveChangesAsync(CancellationToken);

        var content = new byte[1024];
        new Random(55).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        var progress = new Subject<IDownloadFileTransferProgress>();

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .Returns<MoveFileWithResumeCommand, CancellationToken>(
                (moveCommand, _) =>
                {
                    moveCommand.Progress(
                        new MoveFileTransferProgressDTO
                        {
                            Transferred = moveCommand.DataTotal,
                            DataTotal = moveCommand.DataTotal,
                            FileTransferSpeed = moveCommand.DataTotal,
                        }
                    );

                    return Task.FromResult(Result.Ok());
                }
            )
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var after = await dbContext.DownloadTaskTvShowEpisodeFile.AsNoTracking().FirstAsync(CancellationToken);
        after.Percentage.ShouldBe(100m);
    }

    [Test]
    public async Task ShouldHaveFailedResult_WhenSettingMovingStatusFails()
    {
        // Arrange
        await SetupDatabase(
            445566,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[2 * 1024 * 1024];
        new Random(77).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.Is<DownloadStatus>(s => s == DownloadStatus.Moving),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("moving status update boom"))
            .Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.Is<DownloadStatus>(s => s != DownloadStatus.Moving),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("moving status update boom"));

        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<MoveFileWithResumeCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IDownloadTaskUpdateDispatcher>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResultAndPublishNotification_WhenMoveWithResumeFails()
    {
        // Arrange
        await SetupDatabase(
            112233,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[3 * 1024 * 1024];
        new Random(7).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Fail("boom")).Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveError),
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );
    }

    [Test]
    public async Task ShouldSetMovePaused_WhenMoveWithResumeIsCancelled()
    {
        // Arrange
        await SetupDatabase(
            112235,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var downloadFileTask = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[3 * 1024 * 1024];
        new Random(9).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>)
            .ReturnsAsync(ResultExtensions.TaskIsCancelled(nameof(MoveFileWithResumeCommandHandler)))
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsCancelled.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MovePaused),
                        It.IsAny<CancellationToken>()
                    ),
                Times.AtLeastOnce()
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadStatus>(s => s == DownloadStatus.MoveError),
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
    }
}
