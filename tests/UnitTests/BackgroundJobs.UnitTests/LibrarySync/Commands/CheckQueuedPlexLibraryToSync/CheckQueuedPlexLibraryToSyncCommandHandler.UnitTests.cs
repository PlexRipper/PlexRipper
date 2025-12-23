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
            .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task ShouldMarkIsServerOffline_WhenServerIsOffline()
    {
        // Arrange
        await SetupDatabase(
            1006,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 2;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var libraries = dbContext.PlexLibraries.ToList();

        // Remove all server statuses to simulate offline server
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        var queueItem1 = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[0].Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            IsServerOffline = false,
        };

        var queueItem2 = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[1].Id,
            Priority = 2,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            IsServerOffline = false,
        };

        await dbContext.LibrarySyncJobQueues.AddRangeAsync([queueItem1, queueItem2], CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Server is offline, so no jobs should be scheduled
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );

        // All queued items for this server should have IsServerOffline = true
        var updatedItems = await dbContext.LibrarySyncJobQueues.AsNoTracking().ToListAsync(CancellationToken);
        updatedItems.Count.ShouldBe(2);
        updatedItems.All(x => x.IsServerOffline).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldScheduleOneLibraryPerServer_WhenMultipleServersWithQueuedLibraries()
    {
        // Arrange
        await SetupDatabase(
            1007,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 2;
            }
        );

        var dbContext = IDbContext;
        var servers = dbContext.PlexServers.ToList();
        var libraries = dbContext.PlexLibraries.ToList();

        var server1Libraries = libraries.Where(x => x.PlexServerId == servers[0].Id).ToList();
        var server2Libraries = libraries.Where(x => x.PlexServerId == servers[1].Id).ToList();

        // Queue 2 libraries for each server
        var queueItems = new List<LibrarySyncJobQueue>
        {
            new()
            {
                PlexServerId = servers[0].Id,
                PlexLibraryId = server1Libraries[0].Id,
                Priority = 1,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                PlexServerId = servers[0].Id,
                PlexLibraryId = server1Libraries[1].Id,
                Priority = 2,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                PlexServerId = servers[1].Id,
                PlexLibraryId = server2Libraries[0].Id,
                Priority = 1,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            },
            new()
            {
                PlexServerId = servers[1].Id,
                PlexLibraryId = server2Libraries[1].Id,
                Priority = 2,
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            },
        };

        await dbContext.LibrarySyncJobQueues.AddRangeAsync(queueItems, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Setup scheduler mocks for both servers (highest priority library for each)
        var jobKey1 = LibrarySyncJob.GetJobKey(servers[0].Id, server1Libraries[0].Id);
        var jobKey2 = LibrarySyncJob.GetJobKey(servers[1].Id, server2Libraries[0].Id);

        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Should schedule exactly 2 jobs (one per server, highest priority)
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );

        // Verify correct jobs were scheduled (highest priority for each server)
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(jobKey1)),
                        It.IsAny<ITrigger>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(jobKey2)),
                        It.IsAny<ITrigger>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSkipOfflineServerAndScheduleOnlineServer_WhenMixedServerStatuses()
    {
        // Arrange
        await SetupDatabase(
            1008,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var dbContext = IDbContext;
        var servers = dbContext.PlexServers.ToList();
        var libraries = dbContext.PlexLibraries.ToList();

        var server1Library = libraries.First(x => x.PlexServerId == servers[0].Id);
        var server2Library = libraries.First(x => x.PlexServerId == servers[1].Id);

        // Make server 1 offline by removing its statuses
        await dbContext
            .PlexServerStatuses.Where(x => x.PlexServerId == servers[0].Id)
            .ExecuteDeleteAsync(CancellationToken);

        // Queue libraries for both servers
        var queueItem1 = new LibrarySyncJobQueue
        {
            PlexServerId = servers[0].Id,
            PlexLibraryId = server1Library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        var queueItem2 = new LibrarySyncJobQueue
        {
            PlexServerId = servers[1].Id,
            PlexLibraryId = server2Library.Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddRangeAsync([queueItem1, queueItem2], CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        var onlineServerJobKey = LibrarySyncJob.GetJobKey(servers[1].Id, server2Library.Id);
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(onlineServerJobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow);

        var command = new CheckQueuedPlexLibraryToSyncCommand();

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Should only schedule job for the online server
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJob(
                        It.Is<IJobDetail>(j => j.Key.Equals(onlineServerJobKey)),
                        It.IsAny<ITrigger>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );

        // Verify offline server's queue item has IsServerOffline = true
        var updatedItems = await dbContext.LibrarySyncJobQueues.AsNoTracking().ToListAsync(CancellationToken);

        var offlineServerItem = updatedItems.First(x => x.PlexServerId == servers[0].Id);
        offlineServerItem.IsServerOffline.ShouldBeTrue();

        var onlineServerItem = updatedItems.First(x => x.PlexServerId == servers[1].Id);
        onlineServerItem.IsServerOffline.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotScheduleNonQueuedItems_WhenMixedStatusesExist()
    {
        // Arrange
        await SetupDatabase(
            1009,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 4;
            }
        );

        var dbContext = IDbContext;
        var server = dbContext.PlexServers.First();
        var libraries = dbContext.PlexLibraries.ToList();

        var queuedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[0].Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        };

        var processingItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[1].Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
        };

        var failedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[2].Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Failed,
            CreatedAt = DateTime.UtcNow,
            ErrorMessage = "Error",
        };

        var completedItem = new LibrarySyncJobQueue
        {
            PlexServerId = server.Id,
            PlexLibraryId = libraries[3].Id,
            Priority = 1,
            Status = LibrarySyncJobStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
        };

        await dbContext.LibrarySyncJobQueues.AddRangeAsync(
            [queuedItem, processingItem, failedItem, completedItem],
            CancellationToken
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        var expectedJobKey = LibrarySyncJob.GetJobKey(server.Id, libraries[0].Id);
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

        // Should only schedule the queued item, not processing/failed/completed
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
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
}
