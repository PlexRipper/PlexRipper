using Microsoft.EntityFrameworkCore;
using Reaparr.BackgroundJobs.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class QueueLibrarySyncJobCommandHandlerUnitTests : BaseUnitTest<QueueLibrarySyncJobCommandHandler>
{
    public QueueLibrarySyncJobCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldQueueNewLibraries_WhenLibrariesDoNotExistInQueue()
    {
        // Arrange
        await SetupDatabase(
            3001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 2;
            }
        );

        var libraries = IDbContext.PlexLibraries.ToList();
        var libraryIds = libraries.Select(x => x.Id).ToList();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand(libraryIds);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItems = await IDbContext.LibrarySyncJobQueues.ToListAsync(CancellationToken);
        queueItems.Count.ShouldBe(2);
        queueItems.All(x => x.Status == LibrarySyncJobStatus.Queued).ShouldBeTrue();
        queueItems.All(x => libraryIds.Contains(x.PlexLibraryId)).ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldResetCompletedLibraries_WhenCompletedLibrariesExist()
    {
        // Arrange
        await SetupDatabase(
            3002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        var server = IDbContext.PlexServers.First();

        var completedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
        };

        await IDbContext.LibrarySyncJobQueues.AddAsync(completedItem, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItem = await IDbContext.LibrarySyncJobQueues.FirstAsync(CancellationToken);
        queueItem.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        queueItem.PlexLibraryId.ShouldBe(library.Id);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldResetFailedLibraries_WhenFailedLibrariesExist()
    {
        // Arrange
        await SetupDatabase(
            3003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        var server = IDbContext.PlexServers.First();

        var failedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Failed,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ErrorMessage = "Test error",
        };

        await IDbContext.LibrarySyncJobQueues.AddAsync(failedItem, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItem = await IDbContext.LibrarySyncJobQueues.FirstAsync(CancellationToken);
        queueItem.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        queueItem.PlexLibraryId.ShouldBe(library.Id);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSkipQueuedLibraries_WhenLibrariesAlreadyQueued()
    {
        // Arrange
        await SetupDatabase(
            3004,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        var server = IDbContext.PlexServers.First();

        var queuedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await IDbContext.LibrarySyncJobQueues.AddAsync(queuedItem, CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItems = await IDbContext.LibrarySyncJobQueues.ToListAsync(CancellationToken);
        queueItems.Count.ShouldBe(1);
        queueItems[0].Status.ShouldBe(LibrarySyncJobStatus.Queued);
        queueItems[0].PlexLibraryId.ShouldBe(library.Id);
        // Should still call CheckQueuedPlexLibraryToSyncCommand even when skipping
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSkipProcessingLibraries_WhenLibrariesAlreadyProcessing()
    {
        // Arrange
        await SetupDatabase(
            3005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        var server = IDbContext.PlexServers.First();

        // Use a single context instance to ensure the item is saved and can be queried
        var dbContext = IDbContext;
        var processingItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddAsync(processingItem, CancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(CancellationToken);
        saveResult.ShouldBeGreaterThan(0); // Ensure save actually happened

        // Verify the Processing item was saved using the same context instance
        var savedItems = await dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.PlexLibraryId == library.Id && x.Status == LibrarySyncJobStatus.Processing
            )
            .ToListAsync(CancellationToken);
        savedItems.Count.ShouldBe(1);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Use AsNoTracking to see the actual database state
        var queueItems = await IDbContext.LibrarySyncJobQueues.AsNoTracking().ToListAsync(CancellationToken);
        queueItems.Count.ShouldBe(1);
        queueItems[0].Status.ShouldBe(LibrarySyncJobStatus.Processing); // Should remain processing (not reset)
        queueItems[0].PlexLibraryId.ShouldBe(library.Id);
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSetCorrectPriority_WhenQueuingMovies()
    {
        // Arrange
        await SetupDatabase(
            3006,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        // Use a library already created as a Movie type from SetupDatabase
        var library = IDbContext.PlexLibraries.First(x => x.Type == PlexMediaType.Movie);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItem = await IDbContext.LibrarySyncJobQueues.FirstAsync(CancellationToken);
        queueItem.Priority.ShouldBe(1); // Movies should have priority 1
    }

    [Fact]
    public async Task ShouldSetCorrectPriority_WhenQueuingTvShows()
    {
        // Arrange
        await SetupDatabase(
            3007,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
            }
        );

        // Library is already created as a TvShow type from SetupDatabase config
        var library = IDbContext.PlexLibraries.First(x => x.Type == PlexMediaType.TvShow);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItem = await IDbContext.LibrarySyncJobQueues.FirstAsync(CancellationToken);
        queueItem.Priority.ShouldBe(2); // TV shows should have priority 2
    }

    [Fact]
    public async Task ShouldCallCheckQueuedCommand_WhenItemsAreQueued()
    {
        // Arrange
        await SetupDatabase(
            3009,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand([library.Id]);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldReturnOk_WhenNoLibrariesFound()
    {
        // Arrange
        await SetupDatabase(
            3010,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var command = new QueueLibrarySyncJobCommand([99999]); // Non-existent library ID

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItems = await IDbContext.LibrarySyncJobQueues.ToListAsync(CancellationToken);
        queueItems.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldHandleMixedScenarios_WhenMultipleConditionsExist()
    {
        // Arrange
        await SetupDatabase(
            3011,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 3;
                config.PlexTvShowLibraryCount = 2;
            }
        );

        var server = IDbContext.PlexServers.First();
        var libraries = IDbContext.PlexLibraries.ToList();
        var movieLibrary1 = libraries.First(x => x.Type == PlexMediaType.Movie);
        var movieLibrary2 = libraries.Skip(1).First(x => x.Type == PlexMediaType.Movie);
        var movieLibrary3 = libraries.Skip(2).First(x => x.Type == PlexMediaType.Movie);
        var tvShowLibrary1 = libraries.First(x => x.Type == PlexMediaType.TvShow);
        var tvShowLibrary2 = libraries.Skip(1).First(x => x.Type == PlexMediaType.TvShow);

        // Setup existing queue items with different statuses
        var completedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = movieLibrary1.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
        };

        var failedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = movieLibrary2.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Failed,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ErrorMessage = "Error",
        };

        var queuedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = movieLibrary3.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await IDbContext.LibrarySyncJobQueues.AddRangeAsync([completedItem, failedItem, queuedItem], CancellationToken);
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Queue all libraries (some new, some existing with different statuses)
        var allLibraryIds = libraries.Select(x => x.Id).ToList();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var command = new QueueLibrarySyncJobCommand(allLibraryIds);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var queueItems = await IDbContext.LibrarySyncJobQueues.ToListAsync(CancellationToken);

        // Should have: 2 new libraries (tvShowLibrary1, tvShowLibrary2), 1 reset (completedItem), 1 reset (failedItem), 1 skipped (queuedItem)
        queueItems.Count.ShouldBe(5);

        // Verify the completed item was reset
        var resetCompleted = queueItems.First(x => x.PlexLibraryId == movieLibrary1.Id);
        resetCompleted.Status.ShouldBe(LibrarySyncJobStatus.Queued);

        // Verify the failed item was reset
        var resetFailed = queueItems.First(x => x.PlexLibraryId == movieLibrary2.Id);
        resetFailed.Status.ShouldBe(LibrarySyncJobStatus.Queued);

        // Verify queued item remains queued
        var skippedQueued = queueItems.First(x => x.PlexLibraryId == movieLibrary3.Id);
        skippedQueued.Status.ShouldBe(LibrarySyncJobStatus.Queued);

        // Verify new items were added
        var newTvShow1 = queueItems.First(x => x.PlexLibraryId == tvShowLibrary1.Id);
        newTvShow1.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        newTvShow1.Priority.ShouldBe(2); // TV shows have priority 2

        var newTvShow2 = queueItems.First(x => x.PlexLibraryId == tvShowLibrary2.Id);
        newTvShow2.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        newTvShow2.Priority.ShouldBe(2);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<CheckQueuedPlexLibraryToSyncCommand>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }
}
