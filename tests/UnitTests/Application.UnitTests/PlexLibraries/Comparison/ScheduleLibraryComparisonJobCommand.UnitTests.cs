namespace Reaparr.Application.UnitTests;

public class ScheduleLibraryComparisonJobCommandUnitTests : BaseUnitTest<ScheduleLibraryComparisonJobCommandHandler>
{
    [Test]
    public async Task ShouldReturnCancelledWithoutQueryingLibraries_WhenSchedulerIsStopping()
    {
        // Arrange
        var command = new ScheduleLibraryComparisonJobCommand(1, 2);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsCancelled.ShouldBeTrue();
        Mock.Mock<IBackgroundJobScheduler>()
            .Verify(
                x => x.ScheduleJob<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(
                    It.IsAny<JobKey>(),
                    It.IsAny<PlexLibraryComparisonJobPayload>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldReturnCancelledWithoutQueryingLibraries_WhenRequestIsCancelled()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var command = new ScheduleLibraryComparisonJobCommand(1, 2);

        // Act
        var result = await Sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsCancelled.ShouldBeTrue();
        Mock.Mock<IBackgroundJobScheduler>()
            .Verify(
                x => x.ScheduleJob<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(
                    It.IsAny<JobKey>(),
                    It.IsAny<PlexLibraryComparisonJobPayload>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
    }
}