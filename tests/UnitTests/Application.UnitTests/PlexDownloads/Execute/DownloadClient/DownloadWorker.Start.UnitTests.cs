using Autofac;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class DownloadWorkerStartUnitTests : BaseUnitTest<DownloadWorker>
{
    public DownloadWorkerStartUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDownloadFileSuccessfully_WhenNoErrorsHappen()
    {
        // Arrange
        await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
                config.DownloadFileSizeInMb = 10;
            }
        );

        SetupHttpClient();

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

        var downloadWorkerTask = IDbContext.DownloadWorkerTasks.First();

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
        await SetupDatabase(
            26586,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
                config.DownloadFileSizeInMb = 10;
            }
        );

        SetupHttpClient();

        await IDbContext.PlexServerStatuses.Where(x => x.Id > 0).ExecuteDeleteAsync();
        var downloadWorkerTask = IDbContext.DownloadWorkerTasks.First();

        var sut = mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", downloadWorkerTask));
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
        await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
                config.DownloadFileSizeInMb = 10;
            }
        );

        SetupHttpClient();

        mock.SetupMediator(It.IsAny<CreateDownloadFileStreamCommand>)
            .ReturnsAsync(Result.Ok<Stream>(new MemoryStream()))
            .Verifiable(Times.Once);

        mock.Mock<IPlexApiClient>()
            .Setup(x =>
                x.DownloadStreamAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => null)
            .Verifiable(Times.AtLeastOnce);

        var downloadWorkerTask = IDbContext.DownloadWorkerTasks.First();
        var sut = mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", downloadWorkerTask));

        var updateList = new List<DownloadWorkerTaskProgress>();
        sut.DownloadWorkerTaskUpdate.Subscribe(x => updateList.Add(x));

        // Act
        var result = sut.Start();
        await sut.DownloadProcessTask; // Wait for the process to complete

        // Assert
        result.ShouldNotBeNull();
        updateList[0].Status.ShouldBe(DownloadStatus.Error);
    }

    [Fact]
    public async Task ShouldRetryDownloadStream_WhenPrematureHttpIOExceptionIsThrown()
    {
        // Arrange
        await SetupDatabase(
            37820,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
                config.DownloadFileSizeInMb = 10;
            }
        );

        SetupHttpClient();

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
                    if (callbackIndex % 2 == 0)
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
            .ReturnsAsync(new ThrottledStream(mockStream.Object))
            .Verifiable(Times.Once);

        var downloadWorkerTask = IDbContext.DownloadWorkerTasks.First();

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
}
