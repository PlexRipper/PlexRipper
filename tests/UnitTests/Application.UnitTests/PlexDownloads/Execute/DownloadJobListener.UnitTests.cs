using Quartz;

namespace Reaparr.Application.UnitTests;

public class DownloadJobListenerUnitTests : BaseUnitTest<DownloadJobListener>
{
    [Test]
    public async Task ShouldCheckQueues_WhenDownloadFinishes()
    {
        // Arrange
        await SetupDatabase(39397, config => config.MovieDownloadTasksCount = 1);
        var downloadTask = IDbContext.DownloadTaskMovieFile.First();
        await IDbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.DownloadFinished);

        var jobDetail = new Mock<IJobDetail>();
        jobDetail.SetupGet(x => x.Key).Returns(DownloadJob.GetJobKey(downloadTask.Id));
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        context.SetupGet(x => x.MergedJobDataMap).Returns(new DownloadJobPayload(downloadTask.ToKey()).ToJobDataMap());

        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue(CancellationToken))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.Mock<IEventPublisher>()
            .Setup(x => x.PublishAsync(It.IsAny<CheckDownloadQueueEvent>(), CancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        await Sut.JobWasExecuted(context.Object, null, CancellationToken);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify();
        Mock.Mock<IEventPublisher>().Verify();
    }
}
