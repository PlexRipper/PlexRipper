using System.Text.Json;
using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Settings.Contracts;

namespace PlexRipper.Application.UnitTests.Execute;

public class DownloadJob_UnitTests : BaseUnitTest<DownloadJob>
{
    public DownloadJob_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldCreateDownloadWorkers_WhenDownloadWorkerTasksDoNotExist()
    {
        // Arrange
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 5; });
        var testDownloadTask = DbContext.DownloadTaskMovieFile.First();
        mock.Mock<IDownloadManagerSettingsModule>().Setup(x => x.DownloadSegments).Returns(4);
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        mock.Mock<IPlexDownloadClient>().Setup(x => x.Setup(It.IsAny<DownloadTaskKey>(), CancellationToken.None)).ReturnOk();
        mock.Mock<IPlexDownloadClient>().Setup(x => x.Start(It.IsAny<CancellationToken>())).Returns(Result.Ok());
        mock.Mock<IPlexDownloadClient>().SetupGet(x => x.DownloadProcessTask).Returns(Task.CompletedTask);
        mock.Mock<IPlexDownloadClient>().SetupGet(x => x.ListenToDownloadWorkerLog).Returns(new Mock<IObservable<IList<DownloadWorkerLog>>>().Object);

        // Act
        await _sut.Execute(mock.Create<IJobExecutionContext>());

        // Assert
        var downloadWorkerTasks = await DbContext.DownloadWorkerTasks.ToListAsync();
        downloadWorkerTasks.Count.ShouldBe(4);
        downloadWorkerTasks.ShouldAllBe(x => x.DownloadTaskId == testDownloadTask.Id);
    }
}