using Quartz;
using Reaparr.Application;

namespace Reaparr.BackgroundJobs.UnitTests;

public class QueueCheckPlexLibraryUpdatesJobCommandHandlerUnitTests
    : BaseUnitTest<QueueCheckPlexLibraryUpdatesJobCommandHandler>
{
    [Test]
    public async Task ShouldScheduleCheckPlexLibrariesJobEveryThreeHours_WhenJobDoesNotExist()
    {
        // Arrange
        var jobKey = CheckPlexLibrariesForUpdatesJob.GetJobKey();
        var command = new QueueCheckPlexLibraryUpdatesJobCommand();

        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>().Verify(x => x.DeleteJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(jobKey)),
                        It.Is<ITrigger>(t =>
                            t.Key.Name == $"{jobKey.Name}_trigger"
                            && t.Key.Group == jobKey.Group
                            && t.JobKey.Equals(jobKey)
                            && ((ISimpleTrigger)t).RepeatCount == -1
                            && ((ISimpleTrigger)t).RepeatInterval == TimeSpan.FromHours(3)
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldDeleteExistingJobBeforeScheduling_WhenJobAlreadyExists()
    {
        // Arrange
        var jobKey = CheckPlexLibrariesForUpdatesJob.GetJobKey();
        var command = new QueueCheckPlexLibraryUpdatesJobCommand();

        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Mock.Mock<IScheduler>().Setup(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>().Verify(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(jobKey)),
                        It.Is<ITrigger>(t =>
                            t.Key.Name == $"{jobKey.Name}_trigger"
                            && t.Key.Group == jobKey.Group
                            && t.JobKey.Equals(jobKey)
                            && ((ISimpleTrigger)t).RepeatCount == -1
                            && ((ISimpleTrigger)t).RepeatInterval == TimeSpan.FromHours(3)
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
