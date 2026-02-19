using Quartz;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadJobListenerUnitTests : BaseUnitTest<MoveDownloadJobListener>
{
    public MoveDownloadJobListenerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCheckMoveQueue_AfterJobExecuted()
    {
        // Arrange
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act
        await Sut.JobWasExecuted(Mock.Create<IJobExecutionContext>(), null, CancellationToken);

        // Assert
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Once);
    }

    [Fact]
    public async Task ShouldCheckMoveQueue_EvenWhenPreviousJobFailed()
    {
        // Arrange — simulate the scenario where a move job ended in error
        // The listener must still trigger the queue so remaining DownloadFinished tasks are not stuck
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        var jobException = new JobExecutionException("Move failed");

        // Act
        await Sut.JobWasExecuted(Mock.Create<IJobExecutionContext>(), jobException, CancellationToken);

        // Assert: queue check is called regardless of whether the job threw an exception
        Mock.Mock<IMoveDownloadFileQueue>().Verify(x => x.CheckMoveDownloadFileJobQueue(), Times.Once);
    }

    [Fact]
    public async Task ShouldNotThrow_WhenCheckMoveQueueThrows()
    {
        // Arrange — listener must never throw (Quartz requirement)
        Mock.Mock<IMoveDownloadFileQueue>()
            .Setup(x => x.CheckMoveDownloadFileJobQueue())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"))
            .Verifiable(Times.Once);

        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act
        var act = async () => await Sut.JobWasExecuted(Mock.Create<IJobExecutionContext>(), null, CancellationToken);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public Task ShouldReturnCompleted_WhenJobToBeExecutedIsCalled()
    {
        // Arrange
        Mock.Mock<IJobExecutionContext>().SetupGet(x => x.JobDetail).Returns(Mock.Mock<IJobDetail>().Object);

        // Act & Assert — no-op, must not throw
        var act = async () => await Sut.JobToBeExecuted(Mock.Create<IJobExecutionContext>(), CancellationToken);
        return act.ShouldNotThrowAsync();
    }
}
