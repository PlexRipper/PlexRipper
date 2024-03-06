using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadQueue_GetNextDownloadTask_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_GetNextDownloadTask_UnitTests(ITestOutputHelper output) : base(output) { }

    private List<DownloadTaskGeneric> TestDownloadTasks(int count)
    {
        var downloadTasks = new List<DownloadTaskGeneric>();
        var index = 1;
        for (var i = 0; i < count; i++)
        {
            var downloadTask_i = new DownloadTaskGeneric
            {
                Id = Guid.NewGuid(),
                DownloadStatus = DownloadStatus.Queued,
                Children = new List<DownloadTaskGeneric>(),
            };
            for (var j = 0; j < count; j++)
            {
                var downloadTask_j = new DownloadTaskGeneric
                {
                    Id = Guid.NewGuid(),
                    DownloadStatus = DownloadStatus.Queued,
                    Children = new List<DownloadTaskGeneric>(),
                };
                for (var k = 0; k < count; k++)
                {
                    var downloadTask_k = new DownloadTaskGeneric
                    {
                        Id = Guid.NewGuid(),
                        DownloadStatus = DownloadStatus.Queued,
                        Children = new List<DownloadTaskGeneric>(),
                    };
                    for (var l = 0; l < count; l++)
                    {
                        var downloadTask_l = new DownloadTaskGeneric
                        {
                            Id = Guid.NewGuid(),
                            DownloadStatus = DownloadStatus.Queued,
                            Children = new List<DownloadTaskGeneric>(),
                        };
                        downloadTask_k.Children.Add(downloadTask_l);
                    }

                    downloadTask_j.Children.Add(downloadTask_k);
                }

                downloadTask_i.Children.Add(downloadTask_j);
            }

            downloadTasks.Add(downloadTask_i);
        }

        return downloadTasks;
    }

    [Fact]
    public async Task ShouldHaveNextDownloadTask_WhenAllAreQueued()
    {
        // Arrange
        await SetupDatabase();

        var downloadTasks = TestDownloadTasks(2);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        nextDownloadTask.Value.Id.ShouldBe(downloadTasks[4].Id);
    }

    [Fact]
    public async Task ShouldHaveNextDownloadTask_WhenADownloadTaskHasBeenCompleted()
    {
        // Arrange
        await SetupDatabase();

        var downloadTasks = TestDownloadTasks(3);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        nextDownloadTask.Value.Id.ShouldBe(downloadTasks[44].Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTaskInDownloadingTask_WhenAParentDownloadTaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase();

        var downloadTasks = TestDownloadTasks(3);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        nextDownloadTask.Value.Id.ShouldBe(downloadTasks[0].Id);
    }

    [Fact]
    public async Task ShouldHaveNoDownloadTask_WhenADownloadTaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase();

        var downloadTasks = TestDownloadTasks(2);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeFalse();
    }
}