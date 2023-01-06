using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadQueue_AddToDownloadQueue_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_AddToDownloadQueue_UnitTests(ITestOutputHelper output) : base(output) { }


    [Fact]
    public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenEmptyListIsGiven()
    {
        // Act
        var result = await _sut.AddToDownloadQueueAsync(new List<DownloadTask>());

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenInvalidDownloadTasksAreGiven()
    {
        //Arrange
        mock.Mock<IDownloadTaskValidator>().Setup(x => x.ValidateDownloadTasks(It.IsAny<List<DownloadTask>>())).Returns(Result.Fail(""));

        // Act
        var result = await _sut.AddToDownloadQueueAsync(new List<DownloadTask>
        {
            new(),
            new(),
        });

        // Assert
        result.IsFailed.ShouldBeTrue();
    }
}