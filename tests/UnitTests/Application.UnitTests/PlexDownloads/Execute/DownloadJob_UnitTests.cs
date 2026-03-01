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
        Mock.Mock<IPlexDownloadClient>()
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(Result.Ok());

        // Act
        await Sut.Execute(Mock.Create<IJobExecutionContext>());

        // Assert
        var downloadTaskResult = await IDbContext.DownloadTaskMovieFile.FirstOrDefaultAsync(
            x => x.Id == testDownloadTask.Id,
            CancellationToken
        );
        downloadTaskResult.ShouldNotBeNull();

        var downloadFolder = await IDbContext.GetDownloadFolder();
        var destinationFolder = await IDbContext.GetDefaultDestinationFolderPath(
            PlexMediaType.Movie,
            CancellationToken
        );

        downloadTaskResult.DownloadDirectory.ShouldContain(downloadFolder.DirectoryPath);
        downloadTaskResult.DestinationDirectory.ShouldContain(destinationFolder.DirectoryPath);
    }
}
