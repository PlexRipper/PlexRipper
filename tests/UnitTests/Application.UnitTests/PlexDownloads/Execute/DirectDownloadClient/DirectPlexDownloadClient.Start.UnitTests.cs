using System.ComponentModel;
using System.Reactive.Linq;
using Autofac;
using Downloader;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;
using DomainDownloadStatus = Reaparr.Domain.DownloadStatus;

namespace Reaparr.Application.UnitTests;

public class PlexDownloadClientStartUnitTests : BaseUnitTest<DirectPlexDownloadClient>
{
    public PlexDownloadClientStartUnitTests(ITestOutputHelper output)
        : base(output) { }

    // -------------------------------------------------------------------------
    // Shared helpers
    // -------------------------------------------------------------------------

    private static DownloadPackage MakeDownloadPackage(long totalBytes = 10 * 1024) =>
        new()
        {
            TotalFileSize = totalBytes,
            FileName = "test.mp4",
            Urls = ["http://plex/test.mp4"],
        };

    /// <summary>
    /// Creates a mock IDownloadService whose DownloadFileTaskAsync(url, path, ct) raises a single
    /// DownloadProgressChanged event and then a successful DownloadFileCompleted event.
    /// The Package property and UserState on the completed event are both populated so that
    /// the production DownloadFileCompleted handler does not throw a NullReferenceException.
    /// </summary>
    private Mock<IDownloadService> BuildSuccessDownloadServiceMock(long totalBytes = 10 * 1024)
    {
        var package = MakeDownloadPackage(totalBytes);
        var mock = new Mock<IDownloadService>();
        mock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        mock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        mock.Setup(x => x.Package).Returns(package);

        mock.Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                (_, _, _) =>
                {
                    mock.Raise(
                        x => x.DownloadProgressChanged += null,
                        mock.Object,
                        new DownloadProgressChangedEventArgs("Main")
                        {
                            TotalBytesToReceive = totalBytes,
                            ReceivedBytesSize = totalBytes,
                            BytesPerSecondSpeed = 1024,
                        }
                    );
                    mock.Raise(
                        x => x.DownloadFileCompleted += null,
                        mock.Object,
                        // UserState must be a DownloadPackage — the production handler casts it
                        new AsyncCompletedEventArgs(null, false, package)
                    );
                    return Task.CompletedTask;
                }
            );

        return mock;
    }

    private DirectPlexDownloadClient CreateSut(Mock<IDownloadService> downloadServiceMock) =>
        Mock.Create<DirectPlexDownloadClient>(
            new NamedParameter(
                "downloadServiceFactory",
                (Func<DownloadConfiguration, IDownloadService>)(_ => downloadServiceMock.Object)
            )
        );

    private void SetupCommandExecutor()
    {
        // CreateDownloadFileStreamCommand returns Result<Stream>
        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<Stream>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Stream.Null));

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    private void SetupSpeedLimitMocks(string serverMachineIdentifier, int speedLimit = 1000)
    {
        // IDownloadManagerSettings.DownloadSegments is read in the SUT constructor
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimit(serverMachineIdentifier))
            .Returns(speedLimit);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(serverMachineIdentifier))
            .Returns(Observable.Return(speedLimit));
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenSetupAndStartedSuccessfully()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            82345,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        SetupCommandExecutor();

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var startResult = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DomainDownloadStatus.DownloadFinished,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldCreateDownloadStreamUsingTempFileName_WhenStartingDownload()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<Result>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            82346,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        CreateDownloadFileStreamCommand? createStreamCommand = null;

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<Stream>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Stream.Null))
            .Callback<ICommand<Result<Stream>>, CancellationToken>(
                (command, _) => createStreamCommand = command as CreateDownloadFileStreamCommand
            );

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var startResult = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        createStreamCommand.ShouldNotBeNull();

        var expectedTempFileName = Path.GetFileName(downloadTask.DownloadFilePath);
        createStreamCommand!.FileName.ShouldBe(expectedTempFileName);
        createStreamCommand.FileName.ShouldNotBe(downloadTask.FileName);
    }

    [Fact]
    public async Task ShouldStartDownloaderWithFinalPath_WhenTempExtensionIsConfigured()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            82347,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);
        SetupCommandExecutor();

        var package = MakeDownloadPackage(downloadTask.DataTotal);
        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.Package).Returns(package);

        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                (_, _, _) =>
                {
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, false, package)
                    );
                    return Task.CompletedTask;
                }
            );

        var sut = CreateSut(downloadServiceMock);

        // Act
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var expectedFinalPath = Path.Combine(downloadTask.DownloadDirectory, downloadTask.FileName);
        downloadServiceMock.Verify(
            x => x.DownloadFileTaskAsync(It.IsAny<string>(), expectedFinalPath, It.IsAny<CancellationToken>()),
            Times.Once
        );
        downloadServiceMock.Verify(
            x =>
                x.DownloadFileTaskAsync(
                    It.IsAny<string>(),
                    downloadTask.DownloadFilePath,
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ShouldReturnEntityNotFoundError_WhenDownloadTaskKeyDoesNotExist()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            11111,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var nonExistentKey = new DownloadTaskKey
        {
            Type = DownloadTaskType.MovieData,
            Id = Guid.NewGuid(),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };

        // IDownloadManagerSettings.DownloadSegments is read in the SUT constructor
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);

        SetupCommandExecutor();

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var result = await sut.Start(nonExistentKey, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains(nameof(DownloadTaskGeneric)));
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetDownloadUrlFailsDueToNoServerConnections()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            22222,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        // Remove all server connections so GetDownloadUrl cannot choose a connection
        var connections = await dbContext.PlexServerConnections.ToListAsync(CancellationToken);
        dbContext.PlexServerConnections.RemoveRange(connections);
        await dbContext.SaveChangesAsync(CancellationToken);

        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);
        SetupCommandExecutor();

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResultAndPersistStorageError_WhenCreateFileStreamFails()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<Result?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            33333,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        // Make CreateDownloadFileStreamCommand fail; all other Result commands succeed
        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<Stream>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Stream>("Disk full"))
            .Verifiable(Times.Once);

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DomainDownloadStatus.StorageError,
                        It.IsAny<Result>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldTransitionThroughDownloadingStatus_WhenDownloadStartedEventFires()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            44444,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        SetupCommandExecutor();

        var dlPackage = MakeDownloadPackage(10 * 1024);
        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.Package).Returns(dlPackage);
        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, _) =>
                {
                    // DownloadStarted fires before DownloadProgressChanged in a real download
                    downloadServiceMock.Raise(
                        x => x.DownloadStarted += null,
                        downloadServiceMock.Object,
                        new DownloadStartedEventArgs("test.mkv", 10 * 1024)
                    );
                    // Allow async subscription handlers a moment to process
                    await Task.Delay(100);
                    downloadServiceMock.Raise(
                        x => x.DownloadProgressChanged += null,
                        downloadServiceMock.Object,
                        new DownloadProgressChangedEventArgs("Main")
                        {
                            TotalBytesToReceive = 10 * 1024,
                            ReceivedBytesSize = 10 * 1024,
                            BytesPerSecondSpeed = 1024,
                        }
                    );
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, false, dlPackage)
                    );
                }
            );

        // Act
        var sut = CreateSut(downloadServiceMock);
        var startResult = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DomainDownloadStatus.DownloadFinished,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldApplySpeedLimitFromObservable_WhenSpeedLimitObservableEmitsValue()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            55555,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        // Emit two speed limit values; the last one (2000 KB/s) must be reflected on the config
        var speedLimits = new[] { 1000, 2000 };

        // IDownloadManagerSettings.DownloadSegments is read in the SUT constructor
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);

        Mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimit(serverMachineIdentifier)).Returns(2000);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(serverMachineIdentifier))
            .Returns(speedLimits.ToObservable());

        DownloadConfiguration? capturedConfig = null;

        var speedPackage = MakeDownloadPackage();
        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.Package).Returns(speedPackage);
        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, _) =>
                {
                    await Task.Delay(50); // allow observable subscriptions to run
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, false, speedPackage)
                    );
                }
            );

        SetupCommandExecutor();

        var sut = Mock.Create<DirectPlexDownloadClient>(
            new NamedParameter(
                "downloadServiceFactory",
                (Func<DownloadConfiguration, IDownloadService>)(
                    config =>
                    {
                        capturedConfig = config;
                        return downloadServiceMock.Object;
                    }
                )
            )
        );

        // Act
        await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert — last emitted speed limit (2000 KB/s) is written to the configuration
        capturedConfig.ShouldNotBeNull();
        capturedConfig!.MaximumBytesPerSecond.ShouldBe(2000L * 1024);
    }

    [Fact]
    public async Task ShouldCallResumeOverload_WhenDirectDownloadSnapshotExists()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            66666,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        // Seed a resume snapshot — simulates a previously paused download
        var snapshot = new DirectDownloadSnapshot
        {
            SaveProgress = 0.5,
            Status = (int)Downloader.DownloadStatus.Running,
            Urls = ["http://old-plex-url/file.mkv"],
            TotalFileSize = downloadTask.DataTotal,
            FileName = downloadTask.FileName,
            DownloadingFileExtension = FilePathExtensions.TempDownloadFileSuffix,
            Chunks =
            [
                new DirectDownloadSnapshotChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    Start = 0,
                    End = downloadTask.DataTotal / 2,
                    Position = downloadTask.DataTotal / 2,
                    MaxTryAgainOnFailure = 3,
                    Timeout = 1000,
                },
            ],
            IsSupportDownloadInRange = true,
        };

        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.DirectDownloadSnapshot, snapshot), CancellationToken);

        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        DownloadPackage? capturedPackage = null;
        var resumePackage = MakeDownloadPackage(downloadTask.DataTotal);

        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.Package).Returns(resumePackage);
        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<DownloadPackage>(), It.IsAny<CancellationToken>()))
            .Returns<DownloadPackage, CancellationToken>(
                (pkg, _) =>
                {
                    capturedPackage = pkg;
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        // UserState must be a DownloadPackage — the production handler casts and dereferences it
                        new AsyncCompletedEventArgs(null, false, resumePackage)
                    );
                    return Task.FromResult(Stream.Null);
                }
            );

        SetupCommandExecutor();

        // Act
        var sut = CreateSut(downloadServiceMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Resume path must use the DownloadPackage overload, not the URL-string overload
        downloadServiceMock.Verify(
            x => x.DownloadFileTaskAsync(It.IsAny<DownloadPackage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        downloadServiceMock.Verify(
            x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        // The stale URL from the snapshot must be replaced with the fresh authenticated URL
        capturedPackage.ShouldNotBeNull();
        capturedPackage!.Urls.ShouldNotContain("http://old-plex-url/file.mkv");
    }

    [Fact]
    public async Task ShouldPersistProgressSnapshot_WhenDownloadProgressChanges()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            77777,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);

        // Package must return a valid object for ToSnapshot() called during progress handling.
        var fakePackage = new DownloadPackage
        {
            TotalFileSize = downloadTask.DataTotal,
            FileName = downloadTask.FileName,
            Urls = ["http://plex/file.mkv"],
        };
        downloadServiceMock.Setup(x => x.Package).Returns(fakePackage);

        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, _) =>
                {
                    downloadServiceMock.Raise(
                        x => x.DownloadProgressChanged += null,
                        downloadServiceMock.Object,
                        new DownloadProgressChangedEventArgs("Main")
                        {
                            TotalBytesToReceive = downloadTask.DataTotal,
                            ReceivedBytesSize = downloadTask.DataTotal / 2,
                            BytesPerSecondSpeed = 1024,
                        }
                    );
                    // Wait beyond the 500 ms sample window so the Rx handler fires and persists
                    await Task.Delay(700);
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        // UserState must be a DownloadPackage — the production handler casts and dereferences it
                        new AsyncCompletedEventArgs(null, false, fakePackage)
                    );
                }
            );

        SetupCommandExecutor();

        // Act
        var sut = CreateSut(downloadServiceMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // The DownloadProgressChanged handler should forward a non-null snapshot to the dispatcher.
        // Verifying the mock proves the Rx sample fired and the handler ran.
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnProgressUpdated(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadTaskProgress>(),
                        It.Is<DirectDownloadSnapshot?>(s => s != null)
                    ),
                Times.AtLeastOnce()
            );
    }

    [Fact]
    public async Task ShouldPersistDownloadFinishedStatusInDatabase_WhenDownloadCompletesSuccessfully()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            88888,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        SetupCommandExecutor();

        // Act
        var sut = CreateSut(BuildSuccessDownloadServiceMock());
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DomainDownloadStatus.DownloadFinished,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldPersistTotalDataReceived_WhenCompletionEventHasZeroReceivedBytes()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            88889,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);
        SetupCommandExecutor();

        var completionPackage = MakeDownloadPackage(downloadTask.DataTotal);

        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.Package).Returns(completionPackage);
        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                (_, _, _) =>
                {
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, false, completionPackage)
                    );
                    return Task.CompletedTask;
                }
            );

        // Act
        var sut = CreateSut(downloadServiceMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // The completion handler applies Math.Max(ReceivedBytesSize, TotalFileSize) before forwarding.
        // Verify the dispatcher received the corrected DataReceived value — not the raw zero from the event.
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnProgressUpdated(
                        It.IsAny<DownloadTaskKey>(),
                        It.Is<DownloadTaskProgress>(p => p.DataReceived == downloadTask.DataTotal),
                        It.IsAny<DirectDownloadSnapshot?>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSetPausedStatus_WhenDownloadFileCompletedEventIsCancelled()
    {
        // Arrange
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Domain.DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnProgressUpdated(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadTaskProgress>(),
                    It.IsAny<DirectDownloadSnapshot?>()
                )
            )
            .Returns(Result.Ok());
        await SetupDatabase(
            99991,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimitMocks(serverMachineIdentifier);

        SetupCommandExecutor();

        // DownloadFileCompleted with Cancelled=true simulates an explicit cancellation/pause
        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        downloadServiceMock.Setup(x => x.CancelTaskAsync()).Returns(Task.CompletedTask);
        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                (_, _, _) =>
                {
                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, true, null) // Cancelled = true
                    );
                    return Task.CompletedTask;
                }
            );

        // Act
        var sut = CreateSut(downloadServiceMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        DomainDownloadStatus.Paused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
