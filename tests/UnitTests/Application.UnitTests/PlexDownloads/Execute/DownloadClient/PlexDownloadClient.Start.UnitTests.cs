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

public class PlexDownloadClientStartUnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClientStartUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenSetupAndStartedSuccessfully()
    {
        //Arrange
        await SetupDatabase(
            82345,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadSpeedLimit = 1000;
        var dbContext = IDbContext;
        var downloadTask = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext.DownloadWorkerTasks.AddRangeAsync(
            downloadTask.GenerateDownloadWorkerTasks(1),
            CancellationToken
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        // Get the actual machine identifier from the database
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            CancellationToken
        );

        // PlexDownloadClientMocks
        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimit(serverMachineIdentifier))
            .Returns(downloadSpeedLimit)
            .Verifiable(Times.Once);

        Mock.Mock<IServerSettingsModule>()
            .Setup(x => x.GetDownloadSpeedLimitObservable(serverMachineIdentifier))
            .Returns(Observable.Return(downloadSpeedLimit))
            .Verifiable(Times.Once);

        var updateList = new List<IDownloadTaskProgress>();
        var statusList = new List<DomainDownloadStatus>();

        async Task AddDownloadTaskUpdateAsync(DownloadTaskUpdatedCommand command)
        {
            var task = await IDbContext.GetDownloadTaskAsync(command.Key);
            task.ShouldNotBeNull();
            updateList.Add(task);
            statusList.Add(task.DownloadStatus);
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(m => m.Send(It.IsAny<ICommand<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback<ICommand<Result>, CancellationToken>(
                (command, _) =>
                {
                    if (command is DownloadTaskUpdatedCommand notification)
                    {
                        AddDownloadTaskUpdateAsync(notification).GetAwaiter().GetResult();
                    }
                }
            )
            .Verifiable(Times.AtLeastOnce);

        var downloadServiceMock = new Mock<IDownloadService>();
        downloadServiceMock.Setup(x => x.Clear()).Returns(Task.CompletedTask);
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
                            TotalBytesToReceive = 10 * 1024,
                            ReceivedBytesSize = 10 * 1024,
                            BytesPerSecondSpeed = 1024,
                        }
                    );

                    downloadServiceMock.Raise(
                        x => x.DownloadFileCompleted += null,
                        downloadServiceMock.Object,
                        new AsyncCompletedEventArgs(null, false, null)
                    );

                    await Task.CompletedTask;
                }
            );

        // Act
        var sut = Mock.Create<PlexDownloadClient>(
            new NamedParameter(
                "downloadServiceFactory",
                (Func<DownloadConfiguration, IDownloadService>)(_ => downloadServiceMock.Object)
            )
        );

        var startResult = await sut.Start(downloadTask.ToKey(), CancellationToken);

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        updateList.Count.ShouldBeGreaterThanOrEqualTo(1);
        statusList.Count.ShouldBeGreaterThanOrEqualTo(1);
        statusList.Last().ShouldBe(DomainDownloadStatus.DownloadFinished);
    }
}
