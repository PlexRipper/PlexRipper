using Quartz;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobRetryUnitTests : BaseUnitTest<MoveDownloadFileJob>
{
    [Test]
    [Arguments("first", 1)]
    [Arguments("second", 2)]
    public async Task ShouldRetainOneDelayedRetryPerFailedExecution_WhenCurrentMoveKeyIsStillRunning(
        string secondExecutionId, int expectedRetries)
    {
        // Arrange
        await SetupDatabase(78181, config => config.MovieDownloadTasksCount = 2);
        var dbContext = IDbContext;
        var tasks = await dbContext.DownloadTaskMovieFile.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var target = tasks[0];
        var control = tasks[1];
        target.DownloadStatus = DownloadStatus.MoveError;
        dbContext.Entry(target).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(CancellationToken);
        var key = target.ToKey();
        var controlBefore = (control.Id, control.DownloadStatus, control.FileDataTransferred,
            control.CurrentFileTransferBytesOffset, control.Percentage);
        var jobKey = MoveDownloadFileJob.GetJobKey(key.Id);
        var detail = JobBuilder.Create<MoveDownloadFileJob>().WithIdentity(jobKey)
            .UsingJobData(new MoveDownloadFileJobPayload(key).ToJobDataMap()).Build();
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(detail);
        context.SetupGet(x => x.MergedJobDataMap).Returns(detail.JobDataMap);
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken);
        context.SetupGet(x => x.FireInstanceId).Returns("first");
        context.SetupGet(x => x.Scheduler).Returns(Mock.Mock<IScheduler>().Object);
        context.SetupProperty(x => x.Result);
        var pending = new List<ITrigger>();
        var moveFailure = Result.Fail("retryable move failure");
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<MoveDownloadFileFromFileTaskCommand>(c => c.Key == key), CancellationToken))
            .ReturnsAsync(moveFailure).Verifiable(Times.Exactly(2));
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue(CancellationToken))
            .ReturnsAsync(Result.Ok()).Verifiable(Times.Exactly(2));
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.Is<TriggerKey>(k => k.Group == jobKey.Group
                && (k.Name == $"{jobKey.Name}_Retry_first" || k.Name == $"{jobKey.Name}_Retry_second")), CancellationToken))
            .ReturnsAsync((TriggerKey triggerKey, CancellationToken _) => pending.Any(x => x.Key.Equals(triggerKey)))
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.Is<ITrigger>(t => t.JobKey.Equals(jobKey)), CancellationToken))
            .Callback<ITrigger, CancellationToken>((trigger, _) => pending.Add(trigger))
            .ReturnsAsync(DateTimeOffset.UtcNow).Verifiable(Times.Exactly(expectedRetries));
        var before = DateTimeOffset.UtcNow;

        // Act
        await Sut.Execute(context.Object);
        context.SetupGet(x => x.FireInstanceId).Returns(secondExecutionId);
        await Sut.Execute(context.Object);

        // Assert
        var outcome = context.Object.Result.ShouldBeOfType<BackgroundJobResult>();
        outcome.Status.ShouldBe(JobStatus.Failed);
        pending.Select(x => x.Key.Name).ShouldBe(expectedRetries == 1
            ? [$"{jobKey.Name}_Retry_first"]
            : new[] { $"{jobKey.Name}_Retry_first", $"{jobKey.Name}_Retry_second" });
        foreach (var trigger in pending)
        {
            trigger.JobKey.ShouldBe(jobKey);
            trigger.StartTimeUtc.ShouldBeInRange(before.AddMinutes(1), DateTimeOffset.UtcNow.AddMinutes(1));
            ((ISimpleTrigger)trigger).RepeatCount.ShouldBe(0);
            trigger.MisfireInstruction.ShouldBe(MisfireInstruction.SimpleTrigger.FireNow);
        }
        var retained = await dbContext.DownloadTaskMovieFile.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        retained.Select(x => x.Id).ShouldBe(tasks.Select(x => x.Id));
        retained.Single(x => x.Id == target.Id).DownloadStatus.ShouldBe(DownloadStatus.MoveError);
        var retainedControl = retained.Single(x => x.Id == control.Id);
        (retainedControl.Id, retainedControl.DownloadStatus, retainedControl.FileDataTransferred,
            retainedControl.CurrentFileTransferBytesOffset, retainedControl.Percentage).ShouldBe(controlBefore);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IMoveDownloadFileQueue>().Verify();
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CleanUpDownloadTaskFoldersCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>()
            .Verify(x => x.DeleteJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
