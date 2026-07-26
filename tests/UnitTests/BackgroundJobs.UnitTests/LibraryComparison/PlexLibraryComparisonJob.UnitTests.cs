using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class PlexLibraryComparisonJobUnitTests : BaseUnitTest<PlexLibraryComparisonJob>
{
    [Test]
    public async Task ShouldDrainQueuedComparisonItems_WhenMultipleItemsAreQueued()
    {
        // Arrange
        await SetupDatabase(75, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var otherRemoteLibrary = libraries[1];
        var ownedLibrary = libraries[2];
        dbContext.LibraryComparisonJobQueues.AddRange(
            new LibraryComparisonJobQueue
            {
                RemotePlexLibraryId = remoteLibrary.Id,
                OwnedPlexLibraryId = ownedLibrary.Id,
                MediaType = PlexMediaType.Movie,
                Priority = 1,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = new DateTime(2026, 7, 22, 18, 0, 0, DateTimeKind.Utc),
            },
            new LibraryComparisonJobQueue
            {
                RemotePlexLibraryId = remoteLibrary.Id,
                OwnedPlexLibraryId = otherRemoteLibrary.Id,
                MediaType = PlexMediaType.Movie,
                Priority = 2,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = new DateTime(2026, 7, 22, 18, 1, 0, DateTimeKind.Utc),
            },
            new LibraryComparisonJobQueue
            {
                RemotePlexLibraryId = otherRemoteLibrary.Id,
                OwnedPlexLibraryId = ownedLibrary.Id,
                MediaType = PlexMediaType.Movie,
                Priority = 3,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = new DateTime(2026, 7, 22, 18, 2, 0, DateTimeKind.Utc),
            }
        );
        await dbContext.SaveChangesNewAsync(CancellationToken);
        var jobContext = Moq.Mock.Of<IJobExecutionContext>(x => x.CancellationToken == CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CompareMoviePlexLibraryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Exactly(3));
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendLibraryComparisonCompletedAsync(
                It.Is<LibraryComparisonCompletedDTO>(notification =>
                    notification.MediaType == PlexMediaType.Movie
                    && notification.AffectedLibraryIds.Count == 2
                    && notification.AffectedLibraryIds.Contains(remoteLibrary.Id)
                    && notification.AffectedLibraryIds.Contains(otherRemoteLibrary.Id)),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendLibraryComparisonCompletedAsync(
                It.Is<LibraryComparisonCompletedDTO>(notification =>
                    notification.MediaType == PlexMediaType.Movie
                    && notification.AffectedLibraryIds.Count == 2
                    && notification.AffectedLibraryIds.Contains(otherRemoteLibrary.Id)
                    && notification.AffectedLibraryIds.Contains(ownedLibrary.Id)),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        await Sut.Execute(jobContext);

        // Assert
        var queueItems = await IDbContext.LibraryComparisonJobQueues.ToListAsync(CancellationToken);
        queueItems.Count.ShouldBe(3);
        queueItems.ShouldAllBe(x => x.Status == LibrarySyncJobStatus.Completed);
        queueItems.ShouldAllBe(x => x.Attempts == 1);
        queueItems.ShouldAllBe(x => x.StartedAt.HasValue);
        queueItems.ShouldAllBe(x => x.CompletedAt.HasValue);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckQueuedLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<IProgressHubService>().Verify();
    }

    [Test]
    public async Task ShouldSendOneCompletionNotification_WhenRemoteAndOwnedComparisonsAreSettled()
    {
        // Arrange
        await SetupDatabase(76, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.TvShow,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = new DateTime(2026, 7, 22, 18, 0, 0, DateTimeKind.Utc),
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);
        var jobContext = Moq.Mock.Of<IJobExecutionContext>(x => x.CancellationToken == CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<CompareTvShowPlexLibraryCommand>(command =>
                    command.RemotePlexLibraryId == remoteLibrary.Id && command.OwnedPlexLibraryId == ownedLibrary.Id),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendLibraryComparisonCompletedAsync(
                It.Is<LibraryComparisonCompletedDTO>(notification =>
                    notification.MediaType == PlexMediaType.TvShow
                    && notification.AffectedLibraryIds.Count == 2
                    && notification.AffectedLibraryIds.Contains(remoteLibrary.Id)
                    && notification.AffectedLibraryIds.Contains(ownedLibrary.Id)
                    && notification.CompletedAt.Kind == DateTimeKind.Utc),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        await Sut.Execute(jobContext);

        // Assert
        var completedQueueItem = await IDbContext.LibraryComparisonJobQueues.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
        completedQueueItem.Status.ShouldBe(LibrarySyncJobStatus.Completed);
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CheckQueuedLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        Mock.Mock<IProgressHubService>().Verify();
    }

    [Test]
    public async Task ShouldSendCompletionNotification_WhenComparisonFailsAndNoRelatedComparisonsRemainPending()
    {
        // Arrange
        await SetupDatabase(77, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = new DateTime(2026, 7, 22, 18, 0, 0, DateTimeKind.Utc),
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);
        var jobContext = Moq.Mock.Of<IJobExecutionContext>(x => x.CancellationToken == CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<CompareMoviePlexLibraryCommand>(command =>
                    command.RemotePlexLibraryId == remoteLibrary.Id && command.OwnedPlexLibraryId == ownedLibrary.Id),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Fail("Comparison failed"))
            .Verifiable(Times.Once());
        Mock.Mock<IProgressHubService>()
            .Setup(x => x.SendLibraryComparisonCompletedAsync(
                It.Is<LibraryComparisonCompletedDTO>(notification =>
                    notification.MediaType == PlexMediaType.Movie
                    && notification.AffectedLibraryIds.Count == 2
                    && notification.AffectedLibraryIds.Contains(remoteLibrary.Id)
                    && notification.AffectedLibraryIds.Contains(ownedLibrary.Id)),
                It.IsAny<CancellationToken>()
            ))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        await Sut.Execute(jobContext);

        // Assert
        var failedQueueItem = await IDbContext.LibraryComparisonJobQueues.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        failedQueueItem.Status.ShouldBe(LibrarySyncJobStatus.Failed);
        failedQueueItem.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IProgressHubService>().Verify();
    }
}
