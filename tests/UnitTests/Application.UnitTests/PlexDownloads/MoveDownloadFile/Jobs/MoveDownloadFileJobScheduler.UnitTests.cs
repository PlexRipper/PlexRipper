using Quartz;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobSchedulerUnitTests : BaseUnitTest<MoveDownloadFileJobScheduler>
{
    [Test]
    public async Task ShouldSucceed_WhenJobCompletesBeforeInterrupt()
    {
        // Arrange
        var key = new DownloadTaskKey
        {
            Type = DownloadTaskType.MovieData,
            Id = Guid.Parse("f2fbaba3-9073-4294-8041-0b6d76a00e84"),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };
        var jobKey = MoveDownloadFileJob.GetJobKey(key.Id);
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.SetupGet(x => x.Key).Returns(jobKey);
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        var runningChecks = 0;

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => runningChecks++ == 0 ? [context.Object] : [])
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IScheduler>()
            .Setup(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.StopMoveDownloadFileJob(key, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldFail_WhenJobRemainsRunningAfterInterruptFails()
    {
        // Arrange
        var key = new DownloadTaskKey
        {
            Type = DownloadTaskType.MovieData,
            Id = Guid.Parse("3e280449-84b0-442a-84c5-d31e02b3cbd2"),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };
        var jobKey = MoveDownloadFileJob.GetJobKey(key.Id);
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.SetupGet(x => x.Key).Returns(jobKey);
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([context.Object])
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IScheduler>()
            .Setup(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.StopMoveDownloadFileJob(key, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
        Mock.Mock<IScheduler>().Verify();
    }
}
