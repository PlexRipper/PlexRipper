using Quartz;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadJobListenerUnitTests : BaseUnitTest<MoveDownloadJobListener>
{
    [Test]
    public async Task ShouldCheckMoveQueue_AfterJobExecuted()
    {
        // Arrange
        Mock.Mock<IMoveDownloadFileQueue>().Setup(x => x.CheckMoveDownloadFileJobQueue()).ReturnsAsync(Result.Ok());

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act
        await Sut.JobWasExecuted(Mock.Mock<IJobExecutionContext>().Object, null, CancellationToken);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Once);
        Mock.Mock<IJobExecutionContext>().VerifyGet(x => x.JobDetail, Times.Never());
    }

    [Test]
    public async Task ShouldCheckMoveQueue_EvenWhenPreviousJobFailed()
    {
        // Arrange — simulate the scenario where a move job ended in error
        // The listener must still trigger the queue so remaining DownloadFinished tasks are not stuck
        Mock.Mock<IMoveDownloadFileQueue>().Setup(x => x.CheckMoveDownloadFileJobQueue()).ReturnsAsync(Result.Ok());

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        var jobException = new JobExecutionException("Move failed");

        // Act
        await Sut.JobWasExecuted(Mock.Mock<IJobExecutionContext>().Object, jobException, CancellationToken);

        // Assert: queue check is called regardless of whether the job threw an exception
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Once);
        Mock.Mock<IJobExecutionContext>().VerifyGet(x => x.JobDetail, Times.Never());
    }

    [Test]
    public async Task ShouldNotThrow_WhenCheckMoveQueueThrows()
    {
        // Arrange — listener must never throw (Quartz requirement)
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act
        var act = async () =>
            await Sut.JobWasExecuted(Mock.Mock<IJobExecutionContext>().Object, null, CancellationToken);

        // Assert
        await act.ShouldNotThrowAsync();
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Once);
        Mock.Mock<IJobExecutionContext>().VerifyGet(x => x.JobDetail, Times.Once());
    }

    [Test]
    public async Task ShouldNotCheckQueue_WhenJobToBeExecutedIsCalled()
    {
        // Arrange
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act
        await Sut.JobToBeExecuted(Mock.Mock<IJobExecutionContext>().Object, CancellationToken);

        // Assert — JobToBeExecuted is a no-op; the queue must never be triggered here
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Never);
        Mock.Mock<IJobExecutionContext>().VerifyGet(x => x.JobDetail, Times.Never());
    }
}
