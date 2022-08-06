using Autofac;
using PlexRipper.DownloadManager;
using PlexRipper.DownloadManager.DownloadClient;

namespace DownloadManager.UnitTests;

public class DownloadWorker_Start_UnitTests : BaseUnitTest<DownloadWorker>
{
    public DownloadWorker_Start_UnitTests(ITestOutputHelper output) : base(output, true) { }

    [Fact]
    public async Task ShouldCallDownloadWorkerTaskUpdate_WhenInErrorState()
    {
        // Arrange
        mock.Mock<IDownloadFileStream>()
            .Setup(x => x.CreateDownloadFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(Result.Fail("Failed Error"));

        var sut = mock.Create<DownloadWorker>(new NamedParameter("downloadWorkerTask", new DownloadWorkerTask
        {
            Id = 999,
        }));
        DownloadWorkerTask downloadWorkerTaskResult = null;
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