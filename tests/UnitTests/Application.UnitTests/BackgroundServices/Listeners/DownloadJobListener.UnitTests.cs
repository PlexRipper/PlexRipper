using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadJobListenerUnitTests : BaseUnitTest<DownloadJobListener>
{
    public DownloadJobListenerUnitTests()
        : base() { }

    [Test]
    public async Task ShouldCheckMoveQueueAndDownloadQueue_WhenStatusIsDownloadFinished()
    {
        // Arrange
        await SetupDatabase(
            90101,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await IDbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.DownloadFinished);

        var jobDetail = Mock.Mock<IJobDetail>().Object;
        var jobContext = Mock.Mock<IJobExecutionContext>().Object;
        var jobDataMap = new JobDataMap();
        jobDataMap.Put(DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(downloadTask.ToKey()));

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(jobDetail);
        Mock.Mock<IJobDetail>().SetupGet(x => x.JobDataMap).Returns(jobDataMap);
        Mock.Mock<IJobDetail>().SetupGet(x => x.Key).Returns(new JobKey("test", "download"));

        // Act
        await Sut.JobWasExecuted(jobContext, null, CancellationToken);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify();
        Mock.Mock<IEventPublisher>()
            .Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CheckDownloadQueueEvent>(evt =>
                            evt.PlexServerIds.SequenceEqual(new[] { downloadTask.PlexServerId })
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    [Arguments(DownloadStatus.Error)]
    public async Task ShouldCheckOnlyDownloadQueue_WhenStatusIsTerminalFailure(DownloadStatus status)
    {
        // Arrange
        await SetupDatabase(
            90102,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var downloadTask = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await IDbContext.SetDownloadStatus(downloadTask.ToKey(), status);

        var jobDetail = Mock.Mock<IJobDetail>().Object;
        var jobContext = Mock.Mock<IJobExecutionContext>().Object;
        var jobDataMap = new JobDataMap();
        jobDataMap.Put(DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(downloadTask.ToKey()));

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(jobDetail);
        Mock.Mock<IJobDetail>().SetupGet(x => x.JobDataMap).Returns(jobDataMap);
        Mock.Mock<IJobDetail>().SetupGet(x => x.Key).Returns(new JobKey("test", "download"));

        // Act
        await Sut.JobWasExecuted(jobContext, null, CancellationToken);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Never());
        Mock.Mock<IEventPublisher>()
            .Verify(
                x =>
                    x.PublishAsync(
                        It.Is<CheckDownloadQueueEvent>(evt =>
                            evt.PlexServerIds.SequenceEqual(new[] { downloadTask.PlexServerId })
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
