using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.DownloadManager.DownloadClient;

namespace DownloadManager.UnitTests;

public class PlexDownloadClient_Setup_UnitTests
{
    public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        using var mock = AutoMock.GetStrict();
        mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
        var _sut = mock.Create<PlexDownloadClient>();

        // Act
        var result = await _sut.Setup(null);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDownloadTaskPlexServerIsNull()
    {
        //Arrange
        using var mock = AutoMock.GetStrict();
        var _sut = mock.Create<PlexDownloadClient>();
        var downloadTask = FakeData.GetMovieDownloadTask().Generate();
        downloadTask.PlexServer = null;

        // Act
        var result = await _sut.Setup(downloadTask);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldGenerateDownloadWorkerTasks_WhenDownloadTaskHasNoDownloadWorkerTasks()
    {
        //Arrange
        await using PlexRipperDbContext context = await MockDatabase.GetMemoryDbContext().Setup(config =>
        {
            config.MovieDownloadTasksCount = 1;
        });

        using var mock = AutoMock.GetStrict();
        mock.SetupMediator(It.IsAny<AddDownloadWorkerTasksCommand>).ReturnsAsync(Result.Ok());
        mock.Mock<IDownloadManagerSettingsModule>().SetupGet(x => x.DownloadSegments).Returns(4);
        mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimit(It.IsAny<int>())).Returns(4000);
        mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimitObservable(It.IsAny<int>()))
            .Returns(new Subject<int>().AsObservable());

        var downloadTask = context.DownloadTasks.Include(x => x.PlexServer).First(x => x.DownloadTaskType == DownloadTaskType.Movie);

        // Act
        var result = await mock.Create<PlexDownloadClient>().Setup(downloadTask);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.DownloadTask.DownloadWorkerTasks.Count.ShouldBe(4);
    }
}