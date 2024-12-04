using Autofac;
using ByteSizeLib;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class DownloadWorker_Start_UnitTests : BaseUnitTest<DownloadWorker>
{
    public DownloadWorker_Start_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDownloadFileSuccessfully_WhenNoErrorsHappen()
    {
        // Arrange
        var seed = await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        SetupHttpClient();

        var plexServer = IDbContext.PlexServers.First();

        var destinationStream = new MemoryStream();
        mock.SetupMediator(It.IsAny<CreateDownloadFileStreamCommand>)
            .ReturnsAsync(Result.Ok<Stream>(destinationStream))
            .Verifiable(Times.Once);

        var downloadStream = new ThrottledStream(new MemoryStream(new byte[(int)ByteSize.FromMebiBytes(10).Bytes]));
        mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(downloadStream)
            .Verifiable(Times.Once);

        var downloadWorkerTask = GetDownloadWorkerTask(seed, 1, plexServer.Id, downloadStream.Length);

        var sut = mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", downloadWorkerTask));

        var updateList = new List<DownloadWorkerTaskProgress>();
        sut.DownloadWorkerTaskUpdate.Subscribe(x => updateList.Add(x));

        // Act
        var result = sut.Start();
        await sut.DownloadProcessTask; // Wait for the process to complete

        // Assert
        result.ShouldNotBeNull();

        updateList.Count.ShouldBeGreaterThanOrEqualTo(2);
        for (var i = 0; i < updateList.Count - 2; i++)
            updateList[i].Status.ShouldBe(DownloadStatus.Downloading);

        updateList.Last().Status.ShouldBe(DownloadStatus.DownloadFinished);
    }

    [Fact]
    public async Task ShouldHaveDownloadStatusServerUnreachable_WhenPlexServerIsOfflineAndStarting()
    {
        // Arrange
        var seed = await SetupDatabase(
            26586,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        SetupHttpClient();

        var plexServer = IDbContext.PlexServers.First();
        await IDbContext.PlexServerStatuses.Where(x => x.Id > 0).ExecuteDeleteAsync();

        var sut = mock.Create<DownloadWorker>(
            new NamedParameter("downloadWorkerTask", FakeData.GetDownloadWorkerTask(seed, 1, plexServer.Id).Generate())
        );
        var updateList = new List<DownloadWorkerTaskProgress>();
        sut.DownloadWorkerTaskUpdate.Subscribe(x => updateList.Add(x));

        // Act
        var result = sut.Start();

        // Assert
        result.ShouldNotBeNull();
        updateList.First().Status.ShouldBe(DownloadStatus.ServerUnreachable);
    }

    [Fact]
    public async Task ShouldBeInErrorState_WhenDownloadStreamReturnsEmptyStream()
    {
        // Arrange
        var seed = await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        SetupHttpClient();

        var plexServer = IDbContext.PlexServers.First();

        mock.SetupMediator(It.IsAny<CreateDownloadFileStreamCommand>)
            .ReturnsAsync(Result.Ok<Stream>(new MemoryStream()))
            .Verifiable(Times.Once);

        mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => null)
            .Verifiable(Times.Once);

        var sut = mock.Create<DownloadWorker>(
            new NamedParameter("downloadWorkerTask", FakeData.GetDownloadWorkerTask(seed, 1, plexServer.Id).Generate())
        );

        var updateList = new List<DownloadWorkerTaskProgress>();
        sut.DownloadWorkerTaskUpdate.Subscribe(x => updateList.Add(x));

        // Act
        var result = sut.Start();

        // Assert
        result.ShouldNotBeNull();
        updateList[0].Status.ShouldBe(DownloadStatus.Error);
    }

    [Fact]
    public async Task ShouldRetryDownloadStream_WhenPrematureHttpIOExceptionIsThrown()
    {
        // Arrange
        var seed = await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        SetupHttpClient();

        var plexServer = IDbContext.PlexServers.First();

        mock.SetupMediator(It.IsAny<CreateDownloadFileStreamCommand>)
            .ReturnsAsync(Result.Ok<Stream>(new MemoryStream()))
            .Verifiable(Times.Once);

        var mockStream = new Mock<Stream>();
        var realStream = new MemoryStream(new byte[(int)ByteSize.FromMebiBytes(40).Bytes]);
        var callbackIndex = 0;
        mockStream
            .Setup(x =>
                x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .Returns(
                (byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                {
                    callbackIndex++;
                    if (callbackIndex == 3)
                    {
                        throw new HttpIOException(
                            HttpRequestError.InvalidResponse,
                            $"The response ended prematurely, with at least {(int)realStream.Length} additional bytes expected.",
                            new Exception("ResponseEnded")
                        );
                    }

                    return realStream.ReadAsync(buffer, offset, count, cancellationToken);
                }
            );

        mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new ThrottledStream(realStream))
            .Verifiable(Times.Once);

        var downloadWorkerTask = GetDownloadWorkerTask(seed, 1, plexServer.Id, realStream.Length);

        var sut = mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", downloadWorkerTask));

        var updateList = new List<DownloadWorkerTaskProgress>();
        sut.DownloadWorkerTaskUpdate.Subscribe(x => updateList.Add(x));

        // Act
        var result = sut.Start();
        await sut.DownloadProcessTask; // Wait for the process to complete

        // Assert
        result.ShouldNotBeNull();

        for (var i = 0; i < updateList.Count - 2; i++)
            updateList[i].Status.ShouldBe(DownloadStatus.Downloading);

        updateList.Last().Status.ShouldBe(DownloadStatus.DownloadFinished);
    }

    private DownloadWorkerTask GetDownloadWorkerTask(Seed seed, int id = 0, int plexServerId = 0, long totalSize = 0)
    {
        var task = FakeData.GetDownloadWorkerTask(seed, id, plexServerId).Generate();

        return new DownloadWorkerTask
        {
            Id = task.Id,
            StartByte = task.StartByte,
            EndByte = totalSize,
            DownloadStatus = task.DownloadStatus,
            BytesReceived = task.BytesReceived,
            ElapsedTime = task.ElapsedTime,
            FileLocationUrl = task.FileLocationUrl,
            DownloadTaskId = task.DownloadTaskId,
            PlexServer = task.PlexServer,
            PlexServerId = task.PlexServerId,
            FileName = task.FileName,
            DownloadWorkerTaskLogs = task.DownloadWorkerTaskLogs,
            PartIndex = task.PartIndex,
            DownloadDirectory = task.DownloadDirectory,
        };
    }
}
