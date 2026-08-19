using Quartz;

namespace Reaparr.Application.UnitTests;

public class CancelLibrarySyncJobCommandHandlerUnitTests : BaseCommandUnitTest<CancelLibrarySyncJobCommand>
{
    [Test]
    public async Task ShouldCancelQueuedJob_WhenQuartzJobChangedStateBeforeDeletion()
    {
        // Arrange
        await SetupDatabase(
            3100,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );
        var dbContext = IDbContext;
        var library = dbContext.PlexLibraries.Single();
        var queueItem = new LibrarySyncJobQueue
        {
            PlexServerId = library.PlexServerId,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };
        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);
        var command = new CancelLibrarySyncJobCommand(library.Id);
        var jobKey = LibrarySyncJob.GetJobKey(library.PlexServerId, library.Id);

        Mock.Mock<IScheduler>()
            .Setup(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Exactly(2));
        Mock.Mock<IScheduler>()
            .Setup(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var updatedQueueItem = await dbContext.LibrarySyncJobQueues.AsNoTracking().SingleAsync(CancellationToken);
        updatedQueueItem.Status.ShouldBe(LibrarySyncJobStatus.Cancelled);
        updatedQueueItem.CompletedAt.ShouldNotBeNull();
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<INotificationHubService>().Verify();
    }
}
