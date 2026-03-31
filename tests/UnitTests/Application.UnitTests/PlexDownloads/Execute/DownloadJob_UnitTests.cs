using System.Text.Json;
using Autofac;
using Autofac.Features.Indexed;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadJobUnitTests : BaseUnitTest<DownloadJob>
{
    public DownloadJobUnitTests()
        : base() { }

    [Test]
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
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());
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
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldDisposeDownloadClient_WhenJobExecutionCompletes()
    {
        // Arrange
        await SetupDatabase(
            39395,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
            }
        );

        var testDownloadTask = IDbContext.DownloadTaskMovieFile.First();
        IDictionary<string, object> dict = new Dictionary<string, object>
        {
            { DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(testDownloadTask.ToKey()) },
        };

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail.JobDataMap).Returns(new JobDataMap(dict));
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.CancellationToken).Returns(CancellationToken);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(PlexDownloadClientType.Direct))
            .Verifiable(Times.Once());

        var downloadClientMock = Mock.Mock<IPlexDownloadClient>();
        downloadClientMock
            .Setup(x => x.Start(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnsAsync(Result.Ok());
        downloadClientMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable(Times.Once);

        var downloadClientIndexMock = new Mock<IIndex<PlexDownloadClientType, IPlexDownloadClient>>();
        downloadClientIndexMock.Setup(x => x[It.IsAny<PlexDownloadClientType>()]).Returns(downloadClientMock.Object);

        var sut = Mock.Create<DownloadJob>(
            new NamedParameter("plexDownloadClientFactory", downloadClientIndexMock.Object)
        );

        // Act
        await sut.Execute(Mock.Create<IJobExecutionContext>());

        // Assert
        downloadClientMock.Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<DeterminePlexDownloadClientCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }
}
