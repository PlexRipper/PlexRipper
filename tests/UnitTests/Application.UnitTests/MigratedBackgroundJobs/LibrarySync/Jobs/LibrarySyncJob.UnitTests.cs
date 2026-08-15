using Quartz;

namespace Reaparr.Application.UnitTests;

public class LibrarySyncJobUnitTests : BaseUnitTest<LibrarySyncJob>
{
    private static IJobExecutionContext SetupJobContext(int serverId, int libraryId)
    {
        var jobDetail = new Mock<IJobDetail>();
        jobDetail
            .SetupGet(x => x.JobDataMap)
            .Returns(new LibrarySyncJobPayload(serverId, libraryId).ToJobDataMap());

        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(x => x.JobDetail).Returns(jobDetail.Object);
        context.SetupGet(x => x.CancellationToken).Returns(CancellationToken.None);
        return context.Object;
    }

    [Test]
    public async Task ShouldSkipDuplicateDelivery_WhenQueueItemHasAlreadyBeenClaimed()
    {
        // Arrange
        await SetupDatabase(
            55100,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var server = await IDbContext.PlexServers.FirstAsync(CancellationToken);
        var library = await IDbContext.PlexLibraries.FirstAsync(CancellationToken);
        await IDbContext.LibrarySyncJobQueues.AddAsync(
            new LibrarySyncJobQueue
            {
                PlexServerId = server.Id,
                PlexLibraryId = library.Id,
                Priority = 1,
                Status = LibrarySyncJobStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow,
            },
            CancellationToken
        );
        await IDbContext.SaveChangesAsync(CancellationToken);

        var context = SetupJobContext(server.Id, library.Id);

        // Act
        await Sut.Execute(context);

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<InvalidateLibraryComparisonJobsCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<RefreshLibraryMediaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldDequeueItself_WhenLibrarySyncFailsWithPlexUnauthorized()
    {
        // Arrange
        await SetupDatabase(
            55101,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var server = await dbContext.PlexServers.FirstAsync(CancellationToken);
        var library = await dbContext.PlexLibraries.FirstAsync(CancellationToken);
        var queueItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Processing,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        var expectedJobKey = LibrarySyncJob.GetJobKey(server.Id, library.Id);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<InvalidateLibraryComparisonJobsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<RefreshLibraryMediaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<PlexLibrary>("Unauthorized").Add401UnauthorizedError());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IScheduler>()
            .Setup(x =>
                x.DeleteJobs(
                    It.Is<IReadOnlyCollection<JobKey>>(jobKeys =>
                        jobKeys.Count == 1 && jobKeys.Contains(expectedJobKey)
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);
        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>()))
            .Returns(Task.CompletedTask);
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendJobStatusUpdateAsync(It.IsAny<JobStatusUpdate<LibrarySyncJobQueueDTO>>()))
            .Returns(Task.CompletedTask);

        // Act
        await Sut.Execute(SetupJobContext(server.Id, library.Id));

        // Assert
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.DeleteJobs(
                        It.Is<IReadOnlyCollection<JobKey>>(jobKeys =>
                            jobKeys.Count == 1 && jobKeys.Contains(expectedJobKey)
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

        var updatedQueueItem = await IDbContext.LibrarySyncJobQueues.FirstAsync(CancellationToken);
        updatedQueueItem.Status.ShouldBe(LibrarySyncJobStatus.Failed);
        updatedQueueItem.ErrorMessage.ShouldBe("Unauthorized");
        updatedQueueItem.CompletedAt.ShouldNotBeNull();
    }
}
