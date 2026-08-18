using Quartz;

namespace Reaparr.Application.UnitTests;

public class BackgroundJobTerminalOutcomeUnitTests
{
    [Test]
    public void ShouldUseExplicitJobResult_WhenJobSetsTerminalOutcome()
    {
        // Arrange
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.Result).Returns(new BackgroundJobResult(JobStatus.Cancelled, "cancelled"));

        // Act
        var result = BackgroundJobTerminalOutcome.From(context.Object, null);

        // Assert
        result.Status.ShouldBe(JobStatus.Cancelled);
        result.ErrorSummary.ShouldBe("cancelled");
    }

    [Test]
    public void ShouldMapUnhandledExceptionToFailed_WhenNoExplicitResultExists()
    {
        // Arrange
        var exception = new JobExecutionException(new InvalidOperationException("failed"));
        var context = new Mock<IJobExecutionContext>();

        // Act
        var result = BackgroundJobTerminalOutcome.From(context.Object, exception);

        // Assert
        result.Status.ShouldBe(JobStatus.Failed);
        result.ErrorSummary.ShouldBe("failed");
    }

    [Test]
    public void ShouldMapCancellationExceptionToCancelled_WhenNoExplicitResultExists()
    {
        // Arrange
        var exception = new JobExecutionException(new OperationCanceledException("cancelled"));
        var context = new Mock<IJobExecutionContext>();

        // Act
        var result = BackgroundJobTerminalOutcome.From(context.Object, exception);

        // Assert
        result.Status.ShouldBe(JobStatus.Cancelled);
        result.ErrorSummary.ShouldBe(exception.Message);
    }
}
