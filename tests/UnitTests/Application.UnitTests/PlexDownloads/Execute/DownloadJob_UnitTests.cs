using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadJobUnitTests : BaseUnitTest<DownloadJob>
{
    public DownloadJobUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateDownloadWorkers_WhenDownloadWorkerTasksDoNotExist()
    {
        // Arrange
        await SetupDatabase(
            7973,
            config =>
            {
                config.MovieDownloadTasksCount = 5;
            }
        );
        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(4);
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.Setup(It.IsAny<DownloadTaskKey>(), CancellationToken)).ReturnOk();
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.Start()).Returns(Result.Ok());
        Mock.Mock<IPlexDownloadClient>().SetupGet(x => x.DownloadProcessTask).Returns(Task.CompletedTask);
        Mock.Mock<IPlexDownloadClient>()
            .SetupGet(x => x.ListenToDownloadWorkerLog)
            .Returns(new Mock<IObservable<IList<DownloadWorkerLog>>>().Object);
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        // Act
        await Sut.Execute(Mock.Create<IJobExecutionContext>());

        // Assert
        var downloadWorkerTasks = await IDbContext.DownloadWorkerTasks.ToListAsync(CancellationToken);
        downloadWorkerTasks.Count.ShouldBe(4);
        downloadWorkerTasks.ShouldAllBe(x => x.DownloadTaskId == testDownloadTask.Id);
    }

    [Fact]
    public async Task ShouldSetDownloadAndDestinationPath_WhenDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            39394,
            config =>
            {
                config.MovieDownloadTasksCount = 2;
            }
        );
        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(4);
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.Setup(It.IsAny<DownloadTaskKey>(), CancellationToken)).ReturnOk();
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.Start()).Returns(Result.Ok());
        Mock.Mock<IPlexDownloadClient>().SetupGet(x => x.DownloadProcessTask).Returns(Task.CompletedTask);
        Mock.Mock<IPlexDownloadClient>()
            .SetupGet(x => x.ListenToDownloadWorkerLog)
            .Returns(new Mock<IObservable<IList<DownloadWorkerLog>>>().Object);
        Mock.Mock<IPlexDownloadClient>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        // Act
        await Sut.Execute(Mock.Create<IJobExecutionContext>());

        // Assert
        var downloadTaskResult = await IDbContext
            .DownloadTaskMovieFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(x => x.Id == testDownloadTask.Id, CancellationToken);
        downloadTaskResult.ShouldNotBeNull();
        downloadTaskResult.DownloadWorkerTasks.Count.ShouldBe(4);

        var downloadFolder = await IDbContext.GetDownloadFolder();
        var destinationFolder = await IDbContext.GetDefaultDestinationFolderPath(
            PlexMediaType.Movie,
            CancellationToken
        );

        downloadTaskResult.DownloadDirectory.ShouldContain(downloadFolder.DirectoryPath);
        downloadTaskResult.DestinationDirectory.ShouldContain(destinationFolder.DirectoryPath);
    }
}
