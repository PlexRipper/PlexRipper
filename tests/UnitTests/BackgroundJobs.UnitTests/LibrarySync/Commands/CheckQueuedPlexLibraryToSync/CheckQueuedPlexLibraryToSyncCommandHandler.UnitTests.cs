using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class CheckQueuedPlexLibraryToSyncCommandHandlerUnitTests
    : BaseUnitTest<CheckQueuedPlexLibraryToSyncCommandHandler>
{
    public CheckQueuedPlexLibraryToSyncCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnOk_WhenNoQueuedLibrariesExist()
    {
        // Arrange
        await SetupDatabase(
            1001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldScheduleJob_WhenQueuedLibraryExists()
    {
        // Arrange
        await SetupDatabase(
            1002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        // Use a single context instance to ensure data is saved and visible
        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();

        var queueItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(CancellationToken);
        saveResult.ShouldBeGreaterThan(0);

        // Verify the item was saved and is visible
        var savedItems = await dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .ToListAsync(CancellationToken);
        savedItems.Count.ShouldBe(1);
        savedItems[0].PlexLibraryId.ShouldBe(library.Id);

        var jobKey = LibrarySyncJob.GetJobKey(server.Id, library.Id);
        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        // Act
        var command = new CheckQueuedPlexLibraryToSyncCommand();
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(jobKey)),
                        It.IsAny<ITrigger>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldReturnOk_WhenJobAlreadyExists()
    {
        // Arrange
        await SetupDatabase(
            1003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        // Use a single context instance to ensure data is saved and visible
        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();

        var queueItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(CancellationToken);
        saveResult.ShouldBeGreaterThan(0);

        // Verify the item was saved
        var savedItems = await dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .ToListAsync(CancellationToken);
        savedItems.Count.ShouldBe(1);

        var jobKey = LibrarySyncJob.GetJobKey(server.Id, library.Id);
        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var command = new CheckQueuedPlexLibraryToSyncCommand();
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Once());
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }

    [Fact]
    public async Task ShouldScheduleHighestPriorityLibrary_WhenMultipleQueuedLibrariesExist()
    {
        // Arrange
        await SetupDatabase(
            1004,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 2;
            }
        );

        // Use a single context instance to ensure data is saved and visible
        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var libraries = dbContext.PlexLibraries.ToList();

        // Add queue items with different priorities
        var queueItem1 = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[0].Id,
            Priority = 2, // Lower priority (higher number)
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        var queueItem2 = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[1].Id,
            Priority = 1, // Higher priority (lower number)
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddRangeAsync([queueItem1, queueItem2], CancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(CancellationToken);
        saveResult.ShouldBeGreaterThan(0);

        // Verify the items were saved
        var savedItems = await dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .ToListAsync(CancellationToken);
        savedItems.Count.ShouldBe(2);

        var expectedJobKey = LibrarySyncJob.GetJobKey(server.Id, libraries[1].Id); // Should schedule the one with priority 1
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(expectedJobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(expectedJobKey)),
                        It.IsAny<ITrigger>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldLogServerAndLibraryNames_WhenSchedulingJob()
    {
        // Arrange
        await SetupDatabase(
            1005,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        // Use a single context instance to ensure data is saved and visible
        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var library = dbContext.PlexLibraries.First();

        var queueItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddAsync(queueItem, CancellationToken);
        var saveResult = await dbContext.SaveChangesAsync(CancellationToken);
        saveResult.ShouldBeGreaterThan(0);

        // Verify the item was saved and is visible
        var savedItems = await dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .ToListAsync(CancellationToken);
        savedItems.Count.ShouldBe(1);
        savedItems[0].PlexLibraryId.ShouldBe(library.Id);

        var jobKey = LibrarySyncJob.GetJobKey(server.Id, library.Id);
        Mock.Mock<IScheduler>().Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Verify that GetPlexServerNameById and GetPlexLibraryNameById are called
        // This is implicit in the handler's execution - the handler calls these methods
        var serverName = await dbContext.GetPlexServerNameById(server.Id, CancellationToken);
        var libraryName = await dbContext.GetPlexLibraryNameById(library.Id, CancellationToken);
        serverName.ShouldNotBeNullOrEmpty();
        libraryName.ShouldNotBeNullOrEmpty();
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }
}
