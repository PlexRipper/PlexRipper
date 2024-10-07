using Autofac;
using FileSystem.Contracts;

namespace PlexRipper.Application.UnitTests.Execute;

public class DownloadWorker_Start_UnitTests : BaseUnitTest<DownloadWorker>
{
    public DownloadWorker_Start_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCallDownloadWorkerTaskUpdate_WhenInErrorState()
    {
        // Arrange
        await SetupDatabase();

        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Fail("Failed Error"));

        var sut = mock.Create<DownloadWorker>(
            new NamedParameter(
                "downloadWorkerTask",
                new DownloadWorkerTask
                {
                    Id = 999,
                    StartByte = 0,
                    EndByte = 0,
                    DownloadStatus = DownloadStatus.Unknown,
                    BytesReceived = 0,
                    ElapsedTime = 0,
                    FileLocationUrl = string.Empty,
                    DownloadTaskId = default,
                    PlexServer = null,
                    PlexServerId = 0,
                    FileName = "test.part1.mp4",
                    DownloadWorkerTaskLogs = [],
                    PartIndex = 1,
                    DownloadDirectory = "/",
                }
            )
        );
        DownloadWorkerTask? downloadWorkerTaskResult = null;
        sut.DownloadWorkerTaskUpdate.Subscribe(task => downloadWorkerTaskResult = task);

        // Act
        var result = sut.Start();
        await Task.Delay(300);

        // Assert
        result.ShouldNotBeNull();
        downloadWorkerTaskResult.ShouldNotBeNull();
        downloadWorkerTaskResult.DownloadStatus.ShouldBe(DownloadStatus.Error);
    }
}
