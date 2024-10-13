using Autofac;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.PlexApi;

namespace PlexRipper.Application.UnitTests.Execute;

public class DownloadWorker_Start_UnitTests : BaseUnitTest<DownloadWorker>
{
    public DownloadWorker_Start_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldStopDownloadAndCallErrorState_WhenPlexServerIsOfflineAndStarting()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
        });

        var plexServer = IDbContext.PlexServers.First();
        await IDbContext.PlexServerStatuses.Where(x => x.Id > 0).ExecuteDeleteAsync();

        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Fail("Failed Error"));

        var task = FakeData.GetDownloadWorkerTask().Generate();

        var sut = mock.Create<DownloadWorker>(
            new NamedParameter(
                "downloadWorkerTask",
                new DownloadWorkerTask
                {
                    Id = 1,
                    StartByte = task.StartByte,
                    EndByte = task.EndByte,
                    DownloadStatus = task.DownloadStatus,
                    BytesReceived = task.BytesReceived,
                    ElapsedTime = task.ElapsedTime,
                    FileLocationUrl = task.FileLocationUrl,
                    DownloadTaskId = task.DownloadTaskId,
                    PlexServer = task.PlexServer,
                    PlexServerId = plexServer.Id,
                    FileName = task.FileName,
                    DownloadWorkerTaskLogs = task.DownloadWorkerTaskLogs,
                    PartIndex = task.PartIndex,
                    DownloadDirectory = task.DownloadDirectory,
                }
            )
        );
        DownloadWorkerTask? downloadWorkerTaskResult = null;
        sut.DownloadWorkerTaskUpdate.Subscribe(task => downloadWorkerTaskResult = task);

        // Act
        var result = sut.Start();

        // Assert
        result.ShouldNotBeNull();
        downloadWorkerTaskResult.ShouldNotBeNull();
        downloadWorkerTaskResult.DownloadStatus.ShouldBe(DownloadStatus.Error);
    }

    [Fact]
    public async Task ShouldBeInErrorState_DownloadStreamReturnsEmptyStream()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
        });

        var plexServer = IDbContext.PlexServers.First();

        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Ok<Stream>(new MemoryStream()));
        mock.Mock<IPlexApiClient>()
            .Setup(x => x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());

        var task = FakeData.GetDownloadWorkerTask().Generate();

        var sut = mock.Create<DownloadWorker>(
            new NamedParameter(
                "downloadWorkerTask",
                new DownloadWorkerTask
                {
                    Id = 1,
                    StartByte = task.StartByte,
                    EndByte = task.EndByte,
                    DownloadStatus = task.DownloadStatus,
                    BytesReceived = task.BytesReceived,
                    ElapsedTime = task.ElapsedTime,
                    FileLocationUrl = task.FileLocationUrl,
                    DownloadTaskId = task.DownloadTaskId,
                    PlexServer = task.PlexServer,
                    PlexServerId = plexServer.Id,
                    FileName = task.FileName,
                    DownloadWorkerTaskLogs = task.DownloadWorkerTaskLogs,
                    PartIndex = task.PartIndex,
                    DownloadDirectory = task.DownloadDirectory,
                }
            )
        );

        var downloadWorkerTaskResult = new List<DownloadWorkerTask>();
        sut.DownloadWorkerTaskUpdate.Subscribe(task => downloadWorkerTaskResult.Add(task));

        // Act
        var result = sut.Start();

        // Assert
        result.ShouldNotBeNull();
        downloadWorkerTaskResult.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Error);

        mock.Mock<IPlexApiClient>()
            .Verify(
                x => x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
    }
}
