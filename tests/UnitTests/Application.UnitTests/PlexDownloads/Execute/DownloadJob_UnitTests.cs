using System.Text.Json;
using Application.Contracts;
using Data.Contracts;
using Quartz;
using Settings.Contracts;

namespace PlexRipper.Application.UnitTests.Execute;

public class DownloadJob_UnitTests : BaseUnitTest<DownloadJob>
{
    public DownloadJob_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateDownloadWorkers_WhenDownloadWorkerTasksDoNotExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.MovieDownloadTasksCount = 5;
        });
        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(4);
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        mock.Mock<IPlexDownloadClient>()
            .Setup(x => x.Setup(It.IsAny<DownloadTaskKey>(), CancellationToken.None))
            .ReturnOk();
        mock.Mock<IPlexDownloadClient>().Setup(x => x.Start(It.IsAny<CancellationToken>())).Returns(Result.Ok());
        mock.Mock<IPlexDownloadClient>().SetupGet(x => x.DownloadProcessTask).Returns(Task.CompletedTask);
        mock.Mock<IPlexDownloadClient>()
            .SetupGet(x => x.ListenToDownloadWorkerLog)
            .Returns(new Mock<IObservable<IList<DownloadWorkerLog>>>().Object);

        // Act
        await _sut.Execute(mock.Create<IJobExecutionContext>());

        // Assert
        var downloadWorkerTasks = await IDbContext.DownloadWorkerTasks.ToListAsync();
        downloadWorkerTasks.Count.ShouldBe(4);
        downloadWorkerTasks.ShouldAllBe(x => x.DownloadTaskId == testDownloadTask.Id);
    }

    [Fact]
    public async Task ShouldSetDownloadAndDestinationPath_WhenDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.MovieDownloadTasksCount = 2;
        });
        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(4);
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        mock.Mock<IPlexDownloadClient>()
            .Setup(x => x.Setup(It.IsAny<DownloadTaskKey>(), CancellationToken.None))
            .ReturnOk();
        mock.Mock<IPlexDownloadClient>().Setup(x => x.Start(It.IsAny<CancellationToken>())).Returns(Result.Ok());
        mock.Mock<IPlexDownloadClient>().SetupGet(x => x.DownloadProcessTask).Returns(Task.CompletedTask);
        mock.Mock<IPlexDownloadClient>()
            .SetupGet(x => x.ListenToDownloadWorkerLog)
            .Returns(new Mock<IObservable<IList<DownloadWorkerLog>>>().Object);

        // Act
        await _sut.Execute(mock.Create<IJobExecutionContext>());

        // Assert
        var downloadTaskResult = await IDbContext
            .DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(x => x.Id == testDownloadTask.Id);
        downloadTaskResult.ShouldNotBeNull();
        downloadTaskResult.DownloadWorkerTasks.Count.ShouldBe(4);

        var downloadFolder = await IDbContext.GetDownloadFolder();
        var destinationFolder = await IDbContext.GetDefaultDestinationFolderPath(PlexMediaType.Movie);

        downloadTaskResult.DownloadDirectory.ShouldContain(downloadFolder.DirectoryPath);
        downloadTaskResult.DestinationDirectory.ShouldContain(destinationFolder.DirectoryPath);
    }
}
