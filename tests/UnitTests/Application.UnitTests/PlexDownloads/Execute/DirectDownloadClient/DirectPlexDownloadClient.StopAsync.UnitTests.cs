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

public class DirectPlexDownloadClientStopAsyncUnitTests : BaseUnitTest<DirectPlexDownloadClient>
{
    public DirectPlexDownloadClientStopAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    // -------------------------------------------------------------------------
    // Shared helpers
    // -------------------------------------------------------------------------

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

    /// <summary>
    /// Builds a download service mock that simulates an in-progress download.
    /// Calling CancelTaskAsync() on the mock cancels the internal CTS, which unblocks
    /// the infinite Task.Delay in DownloadFileTaskAsync and raises DownloadFileCompleted(cancelled=true).
    /// </summary>
    private static (Mock<IDownloadService> Mock, CancellationTokenSource Cts) BuildInProgressDownloadServiceMock()
    {
        var cts = new CancellationTokenSource();
        var mock = new Mock<IDownloadService>();
        mock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
        mock.Setup(x => x.CancelTaskAsync())
            .Returns(() =>
            {
                cts.Cancel();
                return Task.CompletedTask;
            });

        mock.Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, _) =>
                {
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);
                    }
                    catch (OperationCanceledException) { }

                    mock.Raise(
                        x => x.DownloadFileCompleted += null,
                        mock.Object,
                        new AsyncCompletedEventArgs(null, true, null)
                    );
                }
            );

        return (mock, cts);
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ShouldReturnSuccessResultAndPersistPausedStatus_WhenDownloadClientIsStoppedSuccessfully()
    {
        // Arrange
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

        var (downloadServiceMock, _) = BuildInProgressDownloadServiceMock();

        // Act
        var sut = CreateSut(downloadServiceMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);

        // Give the download time to start before stopping
        await Task.Delay(200, TestContext.Current.CancellationToken);
        var stopResult = await sut.StopAsync();

        // Wait for the start task to complete (it unblocks once DownloadFileCompleted fires)
        var startResult = await startTask;

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        stopResult.IsSuccess.ShouldBeTrue();

        var finalStatus = await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
            .Select(x => x.DownloadStatus)
            .FirstOrDefaultAsync(CancellationToken);
        finalStatus.ShouldBe(DomainDownloadStatus.Paused);
    }

    [Fact]
    public async Task ShouldPersistPausedStatus_WhenDownloadIsCancelledByStop()
    {
        // Arrange
        await SetupDatabase(
            11110,
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

        var (downloadServiceMock, _) = BuildInProgressDownloadServiceMock();

        // Act
        var sut = CreateSut(downloadServiceMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);

        // Give the download time to start before stopping
        await Task.Delay(200, TestContext.Current.CancellationToken);
        await sut.StopAsync();
        await startTask;

        // Assert — Paused status must be written to DB by the DownloadFileCompleted handler
        var finalStatus = await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
            .Select(x => x.DownloadStatus)
            .FirstOrDefaultAsync(CancellationToken);
        finalStatus.ShouldBe(DomainDownloadStatus.Paused);
    }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenStopAsyncIsCalledTwice()
    {
        // Arrange
        await SetupDatabase(
            22220,
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

        var (downloadServiceMock, _) = BuildInProgressDownloadServiceMock();

        var sut = CreateSut(downloadServiceMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);
        await Task.Delay(200, TestContext.Current.CancellationToken);

        // Act — stop twice
        var firstStop = await sut.StopAsync();
        await startTask;
        var secondStop = await sut.StopAsync();

        // Assert — both calls must succeed without throwing
        firstStop.IsSuccess.ShouldBeTrue();
        secondStop.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCallCancelTaskAsync_WhenStopAsyncIsCalled()
    {
        // Arrange
        await SetupDatabase(
            33330,
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

        var (downloadServiceMock, _) = BuildInProgressDownloadServiceMock();

        // Re-setup CancelTaskAsync with Verifiable after BuildInProgressDownloadServiceMock wires it
        var cancelCallCount = 0;
        downloadServiceMock
            .Setup(x => x.CancelTaskAsync())
            .Returns(() =>
            {
                cancelCallCount++;
                // Still need to unblock the infinite delay — grab the internal CTS via closure
                return Task.CompletedTask;
            });

        // Use a fresh, self-contained approach with a local CTS
        var localCts = new CancellationTokenSource();
        downloadServiceMock
            .Setup(x => x.CancelTaskAsync())
            .Returns(() =>
            {
                cancelCallCount++;
                localCts.Cancel();
                return Task.CompletedTask;
            });

        downloadServiceMock
            .Setup(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>(
                async (_, _, _) =>
                {
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, localCts.Token);
                    }
                    catch (OperationCanceledException) { }

                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, true, null)
                    );
                }
            );

        var sut = CreateSut(downloadServiceMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);
        await Task.Delay(200, TestContext.Current.CancellationToken);

        // Act
        await sut.StopAsync();
        await startTask;

        // Assert — CancelTaskAsync must have been invoked exactly once on the underlying service
        cancelCallCount.ShouldBe(1);
    }
}
