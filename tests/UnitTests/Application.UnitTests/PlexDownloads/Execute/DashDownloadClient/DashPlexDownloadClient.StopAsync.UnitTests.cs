using System.IO.Abstractions;
using System.Reactive.Linq;
using Autofac;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.Settings.Contracts;
using DomainDownloadStatus = Reaparr.Domain.DownloadStatus;

namespace Reaparr.Application.UnitTests;

public class DashPlexDownloadClientStopAsyncUnitTests : BaseUnitTest<DashPlexDownloadClient>
{
    public DashPlexDownloadClientStopAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    private DashPlexDownloadClient CreateSut(Mock<IDashMpdCliWrapper> dashWrapperMock)
    {
        var directoryMock = new Mock<IDirectory>();
        directoryMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));

        return Mock.Create<DashPlexDownloadClient>(
            new NamedParameter("dashWrapper", dashWrapperMock.Object),
            new NamedParameter("directory", directoryMock.Object)
        );
    }

    private void SetupCommandExecutor()
    {
        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("https://plex.example/start.mpd"));

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    private void SetupSpeedLimit(string serverMachineIdentifier)
    {
        Mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimit(serverMachineIdentifier)).Returns(0);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(serverMachineIdentifier))
            .Returns(Observable.Return(0));
    }

    [Fact]
    public async Task ShouldPersistStoppedStatus_WhenStopAsyncIsCalled()
    {
        await SetupDatabase(
            13001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await IDbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimit(serverMachineIdentifier);
        SetupCommandExecutor();

        var exitTcs = new TaskCompletionSource<int>();
        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock
            .Setup(x => x.StopAsync())
            .Returns(() =>
            {
                exitTcs.TrySetResult(143);
                return Task.CompletedTask;
            });
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);

        await Task.Delay(100, TestContext.Current.CancellationToken);
        var stopResult = await sut.StopAsync();
        var startResult = await startTask;

        stopResult.IsSuccess.ShouldBeTrue();
        startResult.IsSuccess.ShouldBeTrue();

        var finalStatus = await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
            .Select(x => x.DownloadStatus)
            .FirstOrDefaultAsync(CancellationToken);

        finalStatus.ShouldBe(DomainDownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenStopAsyncIsCalledTwice()
    {
        await SetupDatabase(
            13002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var serverMachineIdentifier = await IDbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        SetupSpeedLimit(serverMachineIdentifier);
        SetupCommandExecutor();

        var exitTcs = new TaskCompletionSource<int>();
        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());
        dashWrapperMock.Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>())).ReturnsAsync(Result.Ok());
        dashWrapperMock
            .Setup(x => x.StopAsync())
            .Returns(() =>
            {
                exitTcs.TrySetResult(143);
                return Task.CompletedTask;
            });
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);
        var startTask = sut.Start(downloadTask.ToKey(), CancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        var firstStop = await sut.StopAsync();
        var secondStop = await sut.StopAsync();
        await startTask;

        firstStop.IsSuccess.ShouldBeTrue();
        secondStop.IsSuccess.ShouldBeTrue();
    }
}
