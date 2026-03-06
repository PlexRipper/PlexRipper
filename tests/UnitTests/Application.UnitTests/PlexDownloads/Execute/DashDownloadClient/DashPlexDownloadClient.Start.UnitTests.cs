using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.Settings.Contracts;
using DomainDownloadStatus = Reaparr.Domain.DownloadStatus;

namespace Reaparr.Application.UnitTests;

public class DashPlexDownloadClientStartUnitTests : BaseUnitTest<DashPlexDownloadClient>
{
    public DashPlexDownloadClientStartUnitTests(ITestOutputHelper output)
        : base(output) { }

    private DashPlexDownloadClient CreateSut(
        Mock<IDashMpdCliWrapper> dashWrapperMock,
        Mock<IDirectory>? directoryMock = null
    )
    {
        var dirMock = directoryMock ?? new Mock<IDirectory>();
        dirMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));

        return Mock.Create<DashPlexDownloadClient>(
            new NamedParameter("dashWrapper", dashWrapperMock.Object),
            new NamedParameter("directory", dirMock.Object)
        );
    }

    private void SetupCommandExecutor(
        Result<string>? getUrlResult = null,
        Func<DownloadTaskUpdatedCommand, Task>? onUpdate = null
    )
    {
        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getUrlResult ?? Result.Ok("https://plex.example/start.mpd"));

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback<ICommand<Result>, CancellationToken>(
                (command, _) =>
                {
                    if (command is DownloadTaskUpdatedCommand notification)
                        onUpdate?.Invoke(notification).GetAwaiter().GetResult();
                }
            );
    }

    private void SetupSpeedLimit(string serverMachineIdentifier, int speedLimit = 2000)
    {
        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimit(serverMachineIdentifier))
            .Returns(speedLimit);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(serverMachineIdentifier))
            .Returns(Observable.Return(speedLimit));
    }

    [Fact]
    public async Task ShouldReturnSuccessAndPersistDownloadFinished_WhenProcessExitsZero()
    {
        await SetupDatabase(
            12001,
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

        SetupSpeedLimit(serverMachineIdentifier, 2000);

        var exitTcs = new TaskCompletionSource<int>();
        var progressSubject = new Subject<DashDownloadProgress>();
        var outputSubject = new Subject<string>();
        DashMpdCliOptions? capturedOptions = null;

        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(progressSubject.AsObservable());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(outputSubject.AsObservable());
        dashWrapperMock
            .Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>()))
            .Returns<DashMpdCliOptions>(options =>
            {
                capturedOptions = options;
                exitTcs.TrySetResult(0);
                return Task.FromResult(Result.Ok());
            });
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        SetupCommandExecutor();

        var sut = CreateSut(dashWrapperMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        capturedOptions.ShouldNotBeNull();
        capturedOptions!.LimitRate.ShouldBe("2000K");

        var finalStatus = await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadTask.Id)
            .Select(x => x.DownloadStatus)
            .FirstOrDefaultAsync(CancellationToken);

        finalStatus.ShouldBe(DomainDownloadStatus.DownloadFinished);
    }

    [Fact]
    public async Task ShouldReturnEntityNotFoundError_WhenDownloadTaskKeyDoesNotExist()
    {
        await SetupDatabase(
            12002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        SetupCommandExecutor();

        var key = new DownloadTaskKey
        {
            Type = DownloadTaskType.MovieData,
            Id = Guid.NewGuid(),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };

        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());

        var sut = CreateSut(dashWrapperMock);
        var result = await sut.Start(key, CancellationToken);

        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains(nameof(DownloadTaskGeneric)));
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenGetDashDownloadUrlFails()
    {
        await SetupDatabase(
            12003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        SetupCommandExecutor(Result.Fail<string>("Could not get DASH URL"));

        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(Observable.Empty<DashDownloadProgress>());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(Observable.Empty<string>());

        var sut = CreateSut(dashWrapperMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        result.IsFailed.ShouldBeTrue();
        dashWrapperMock.Verify(x => x.StartAsync(It.IsAny<DashMpdCliOptions>()), Times.Never);
    }

    [Fact]
    public async Task ShouldPersistProgress_WhenDashProgressEmits()
    {
        await SetupDatabase(
            12004,
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

        SetupSpeedLimit(serverMachineIdentifier, 0);
        SetupCommandExecutor();

        var exitTcs = new TaskCompletionSource<int>();
        var progressSubject = new Subject<DashDownloadProgress>();
        var outputSubject = new Subject<string>();

        var dashWrapperMock = new Mock<IDashMpdCliWrapper>();
        dashWrapperMock.Setup(x => x.Progress).Returns(progressSubject.AsObservable());
        dashWrapperMock.Setup(x => x.StandardOutput).Returns(outputSubject.AsObservable());
        dashWrapperMock
            .Setup(x => x.StartAsync(It.IsAny<DashMpdCliOptions>()))
            .Returns(() =>
            {
                _ = Task.Run(async () =>
                {
                    progressSubject.OnNext(
                        new DashDownloadProgress
                        {
                            ETA = TimeSpan.FromSeconds(1),
                            Percent = 50,
                            DownloadedBytes = downloadTask.DataTotal / 2,
                            TotalBytes = downloadTask.DataTotal,
                            DownloadSpeedInBytes = 1024,
                            RawOutput = "{}",
                        }
                    );

                    await Task.Delay(700);
                    exitTcs.TrySetResult(0);
                });

                return Task.FromResult(Result.Ok());
            });
        dashWrapperMock.Setup(x => x.StopAsync()).ReturnsAsync(Result.Ok());
        dashWrapperMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var sut = CreateSut(dashWrapperMock);
        var result = await sut.Start(downloadTask.ToKey(), CancellationToken);

        result.IsSuccess.ShouldBeTrue();

        var updatedTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(
            x => x.Id == downloadTask.Id,
            CancellationToken
        );
        updatedTask.DataReceived.ShouldBeGreaterThan(0);
        updatedTask.DownloadSpeed.ShouldBeGreaterThanOrEqualTo(0);
    }
}
