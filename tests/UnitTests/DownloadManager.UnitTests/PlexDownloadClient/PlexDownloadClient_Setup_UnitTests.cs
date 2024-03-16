using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using Settings.Contracts;

namespace DownloadManager.UnitTests;

public class PlexDownloadClient_Setup_UnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        await SetupDatabase();

        // Act
        var result = await _sut.Setup(null);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldCreateDownloadWorkers_WhenSetupIsCalledWithValidDownloadTask()
    {
        //Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieDownloadTasksCount = 1;
        });

        var downloadTask = await DbContext.DownloadTaskMovieFile.Include(x => x.PlexLibrary).FirstAsync();
        mock.Mock<IDownloadManagerSettingsModule>().Setup(x => x.DownloadSegments).Returns(4);
        mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimit(It.IsAny<string>())).Returns(0);
        mock.Mock<IServerSettingsModule>().Setup(x => x.GetDownloadSpeedLimitObservable(It.IsAny<string>())).Returns(Observable.Return(0));

        // Act
        var result = await _sut.Setup(downloadTask.ToKey());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadWorkerTasks = await DbContext.DownloadWorkerTasks.ToListAsync();
        downloadWorkerTasks.Count.ShouldBe(4);
        downloadWorkerTasks.ShouldAllBe(x => x.DownloadTaskId == downloadTask.Id);
    }
}