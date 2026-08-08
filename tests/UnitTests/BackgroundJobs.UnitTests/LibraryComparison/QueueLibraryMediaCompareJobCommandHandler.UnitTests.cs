using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class QueueLibraryMediaCompareJobCommandHandlerUnitTests
    : BaseUnitTest<QueueLibraryMediaCompareJobCommandHandler>
{
    [Test]
    public async Task ShouldInvalidateComparisonScope_WhenCompletedQueueItemIsRequeued()
    {
        // Arrange
        await SetupDatabase(78, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        var command = new QueueLibraryMediaCompareJobCommand(
            remoteLibrary.Id,
            ownedLibrary.Id,
            PlexMediaType.Movie
        );
        var completedAt = new DateTime(2026, 7, 22, 19, 10, 0, DateTimeKind.Utc);
        var remoteUpdatedAt = new DateTime(2026, 7, 22, 19, 0, 0, DateTimeKind.Utc);
        var ownedUpdatedAt = new DateTime(2026, 7, 22, 19, 5, 0, DateTimeKind.Utc);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Completed,
            CreatedAt = new DateTime(2026, 7, 22, 18, 0, 0, DateTimeKind.Utc),
            StartedAt = new DateTime(2026, 7, 22, 18, 1, 0, DateTimeKind.Utc),
            CompletedAt = completedAt,
            ErrorMessage = "previous error",
        });
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            CompletedAt = completedAt,
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var queueItem = await IDbContext.LibraryComparisonJobQueues.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id
                 && x.OwnedPlexLibraryId == ownedLibrary.Id
                 && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        queueItem.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        queueItem.StartedAt.ShouldBeNull();
        queueItem.CompletedAt.ShouldBeNull();
        queueItem.ErrorMessage.ShouldBeNull();
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id
                 && x.OwnedPlexLibraryId == ownedLibrary.Id
                 && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        scope.RemoteLibraryUpdatedAt.ShouldBeNull();
        scope.OwnedLibraryUpdatedAt.ShouldBeNull();
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()),
            Times.Never()
        );
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldKeepCurrentComparisonScope_WhenQueueItemIsAlreadyQueued()
    {
        // Arrange
        await SetupDatabase(79, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        var command = new QueueLibraryMediaCompareJobCommand(
            remoteLibrary.Id,
            ownedLibrary.Id,
            PlexMediaType.Movie
        );
        var remoteUpdatedAt = new DateTime(2026, 7, 22, 20, 0, 0, DateTimeKind.Utc);
        var ownedUpdatedAt = new DateTime(2026, 7, 22, 20, 5, 0, DateTimeKind.Utc);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = new DateTime(2026, 7, 22, 20, 10, 0, DateTimeKind.Utc),
        });
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            CompletedAt = new DateTime(2026, 7, 22, 20, 9, 0, DateTimeKind.Utc),
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id
                 && x.OwnedPlexLibraryId == ownedLibrary.Id
                 && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        scope.RemoteLibraryUpdatedAt.ShouldBe(remoteUpdatedAt);
        scope.OwnedLibraryUpdatedAt.ShouldBe(ownedUpdatedAt);
        Mock.Mock<IMediaQueryCache>()
            .Verify(
                x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()),
                Times.Never
            );
        Mock.Mock<ICommandExecutor>().Verify();
    }
}
