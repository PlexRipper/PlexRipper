using Quartz;

namespace Reaparr.BackgroundJobs.UnitTests;

public class QueueCheckPlexLibraryUpdatesJobCommandHandlerUnitTests
    : BaseCommandUnitTest<QueueCheckPlexLibraryUpdatesJobCommand>
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
        var result = await TestHandlerExecuteAsync(command);

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
                            && t.StartTimeUtc > DateTimeOffset.UtcNow.AddHours(2).AddMinutes(59)
                            && ((ISimpleTrigger)t).RepeatCount == -1
                            && ((ISimpleTrigger)t).RepeatInterval == TimeSpan.FromHours(3)
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Test]
    public async Task ShouldReplaceExistingJobWithThreeHourSchedule_WhenJobExists()
    {
        // Arrange
        var jobKey = CheckPlexLibrariesForUpdatesJob.GetJobKey();
        var triggerKey = new TriggerKey($"{jobKey.Name}_trigger", jobKey.Group);
        var command = new QueueCheckPlexLibraryUpdatesJobCommand();

        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Mock.Mock<IScheduler>()
            .Setup(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        // Act
        var result = await TestHandlerExecuteAsync(command);

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
                            t.Key.Equals(triggerKey)
                            && t.JobKey.Equals(jobKey)
                            && t.StartTimeUtc > DateTimeOffset.UtcNow.AddHours(2).AddMinutes(59)
                            && ((ISimpleTrigger)t).RepeatCount == -1
                            && ((ISimpleTrigger)t).RepeatInterval == TimeSpan.FromHours(3)
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }
}
