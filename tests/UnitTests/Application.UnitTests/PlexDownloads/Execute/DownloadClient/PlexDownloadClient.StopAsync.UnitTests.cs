using System.Reactive.Linq;
using Autofac;
using ByteSizeLib;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi;
using Reaparr.PlexApi.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class PlexDownloadClientStopAsyncUnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClientStopAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldEmitStopDownloadStatus_WhenDownloadClientIsStoppedSuccessfully()
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

        SetupHttpClient(x => x.SetupDownloadFile(100));
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
        var statusList = new List<DownloadStatus>();

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

        // DownloadWorkerMocks
        var destinationStream = new MemoryStream();
        Mock.SetupCommand(It.IsAny<CreateDownloadFileStreamCommand>)
            .ReturnsAsync(Result.Ok<Stream>(destinationStream))
            .Verifiable(Times.Once);

        var downloadStream = new ThrottledStream(new MemoryStream(new byte[(int)ByteSize.FromMebiBytes(10).Bytes]));
        Mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(downloadStream)
            .Verifiable(Times.Once);

        // Act
        var sut = Mock.Create<PlexDownloadClient>(
            new NamedParameter(
                "downloadWorkerFactory",
                (DownloadWorkerTask task) => Mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", task))
            ),
            new NamedParameter(
                "clientFactory",
                (PlexApiClientOptions options) => Mock.Create<PlexApiClient>(new NamedParameter("options", options))
            )
        );

        await sut.Setup(downloadTask.ToKey(), CancellationToken);

        var startResult = sut.Start();
        await Task.Delay(1500, TestContext.Current.CancellationToken);
        var stopResult = await sut.StopAsync();

        // Wait for the process to complete
        await sut.DownloadProcessTask;

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        stopResult.IsSuccess.ShouldBeTrue();

        updateList.Count.ShouldBeGreaterThanOrEqualTo(2);
        statusList.Count.ShouldBeGreaterThanOrEqualTo(2);

        statusList.Last().ShouldBe(DownloadStatus.Stopped);
    }
}
