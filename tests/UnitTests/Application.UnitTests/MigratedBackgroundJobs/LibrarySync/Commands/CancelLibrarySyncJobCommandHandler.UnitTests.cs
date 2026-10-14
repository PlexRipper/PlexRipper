using Quartz;

namespace Reaparr.Application.UnitTests;

public class CancelLibrarySyncJobCommandHandlerUnitTests : BaseCommandUnitTest<CancelLibrarySyncJobCommand>
{
    [Test]
    [Arguments(LibrarySyncJobStatus.Completed)]
    [Arguments(LibrarySyncJobStatus.Cancelled)]
    [Arguments(LibrarySyncJobStatus.Failed)]
    public async Task ShouldPreserveTerminalHistory_WhenCancelling(LibrarySyncJobStatus status)
    {
        // Arrange
        await SetupDatabase(
            3102,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );
        var dbContext = IDbContext;
        var library = dbContext.PlexLibraries.Single();
        var completedAt = new DateTime(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);
        dbContext.LibrarySyncJobQueues.Add(
            new LibrarySyncJobQueue
            {
                PlexServerId = library.PlexServerId,
                PlexLibraryId = library.Id,
                Priority = 1,
                Status = status,
                CreatedAt = completedAt.AddMinutes(-1),
                CompletedAt = completedAt,
                ErrorMessage = "Retained sync history",
                IsServerOffline = true,
            }
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await TestHandlerExecuteAsync(new CancelLibrarySyncJobCommand(library.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var retainedHistory = await dbContext.LibrarySyncJobQueues.AsNoTracking().SingleAsync(CancellationToken);
        retainedHistory.Status.ShouldBe(status);
        retainedHistory.CompletedAt.ShouldBe(completedAt);
        retainedHistory.ErrorMessage.ShouldBe("Retained sync history");
        retainedHistory.IsServerOffline.ShouldBeTrue();
        Mock.Mock<IScheduler>().VerifyNoOtherCalls();
        Mock.Mock<INotificationHubService>().VerifyNoOtherCalls();
    }

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
        Mock.Mock<IScheduler>().Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(
                    It.Is<List<RefreshDataType>>(types =>
                        types.Count == 1 && types[0] == RefreshDataType.PlexLibrarySyncStatus
                    )
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Result.Ok())
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
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldWaitForJobCompletion_WhenDeleteJobFailsButSecondInterruptSucceeds()
    {
        // Arrange
        await SetupDatabase(
            3101,
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
            Status = LibrarySyncJobStatus.Processing,
            CreatedAt = DateTime.UtcNow,
        };
        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);
        var command = new CancelLibrarySyncJobCommand(library.Id);
        var jobKey = LibrarySyncJob.GetJobKey(library.PlexServerId, library.Id);

        Mock.Mock<IScheduler>()
            .SetupSequence(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);
        Mock.Mock<IScheduler>()
            .Setup(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var updatedQueueItem = await dbContext.LibrarySyncJobQueues.AsNoTracking().SingleAsync(CancellationToken);
        updatedQueueItem.Status.ShouldBe(LibrarySyncJobStatus.Processing);
        updatedQueueItem.CompletedAt.ShouldBeNull();
        Mock.Mock<IScheduler>().Verify(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()), Times.Exactly(2));
        Mock.Mock<IScheduler>().Verify(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>().Verify();
    }
    [Test]
    public async Task ShouldCancelOnlyTargetedLibrary_AndAdvanceQueue()
    {
        // Arrange
        await SetupDatabase(
            3103,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = dbContext.PlexLibraries.OrderBy(x => x.Id).ToList();
        await dbContext.LibrarySyncJobQueues.AddRangeAsync(
            libraries.Select((library, index) => new LibrarySyncJobQueue
                {
                    PlexServerId = library.PlexServerId,
                    PlexLibraryId = library.Id,
                    Priority = index + 1,
                    Status = LibrarySyncJobStatus.Queued,
                    CreatedAt = DateTime.UtcNow,
                })
                .ToList(),
            CancellationToken
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        var targetLibrary = libraries[0];
        var retainedLibrary = libraries[1];
        var jobKey = LibrarySyncJob.GetJobKey(targetLibrary.PlexServerId, targetLibrary.Id);
        Mock.Mock<IScheduler>()
            .Setup(x => x.Interrupt(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.DeleteJob(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
        Mock.Mock<INotificationHubService>()
            .Setup(x =>
                x.SendRefreshNotificationAsync(
                    It.Is<List<RefreshDataType>>(types =>
                        types.Count == 1 && types[0] == RefreshDataType.PlexLibrarySyncStatus
                    )
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new CancelLibrarySyncJobCommand(targetLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var queueItems = await dbContext
            .LibrarySyncJobQueues.AsNoTracking()
            .OrderBy(x => x.PlexLibraryId)
            .Select(x => new { x.PlexLibraryId, x.Status })
            .ToListAsync(CancellationToken);
        queueItems.ShouldBe([
            new { PlexLibraryId = targetLibrary.Id, Status = LibrarySyncJobStatus.Cancelled },
            new { PlexLibraryId = retainedLibrary.Id, Status = LibrarySyncJobStatus.Queued },
        ]);
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<INotificationHubService>().Verify();
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
