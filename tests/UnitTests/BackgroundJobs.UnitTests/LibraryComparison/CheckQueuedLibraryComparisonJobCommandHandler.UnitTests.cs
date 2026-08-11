using Quartz;
using Reaparr.Application;

namespace Reaparr.BackgroundJobs.UnitTests;

public class CheckQueuedLibraryComparisonJobCommandHandlerUnitTests
    : BaseUnitTest<CheckQueuedLibraryComparisonJobCommandHandler>
{
    [Test]
    public async Task ShouldNotTriggerExistingJob_WhenComparisonWorkerIsAlreadyRunning()
    {
        // Arrange
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
        var triggerKey = new TriggerKey($"{jobKey.Name}_trigger", jobKey.Group);
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(71, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([Moq.Mock.Of<IJobExecutionContext>(x => x.JobDetail == Moq.Mock.Of<IJobDetail>(y => y.Key == jobKey))])
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(triggerKey, It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>().Verify(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldRequeueProcessingItemsAndTriggerExistingJob_WhenNoComparisonWorkerIsRunning()
    {
        // Arrange
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(72, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            ErrorMessage = "Container stopped while processing",
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var queueItem = await IDbContext.LibraryComparisonJobQueues.SingleAsync(CancellationToken);
        queueItem.Status.ShouldBe(LibrarySyncJobStatus.Queued);
        queueItem.StartedAt.ShouldBeNull();
        queueItem.ErrorMessage.ShouldBeNull();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldNotTriggerExistingJob_WhenComparisonWorkerTriggerAlreadyExists()
    {
        // Arrange
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(80, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>().Verify(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldTriggerExistingJob_WhenComparisonWorkerExistsAndIsNotRunning()
    {
        // Arrange
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(81, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldFailProcessingItemsAtMaxAttemptsAndNotTriggerJob_WhenNoQueuedItemsRemain()
    {
        // Arrange
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(83, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Processing,
            Attempts = 3,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var queueItem = await IDbContext.LibraryComparisonJobQueues.SingleAsync(CancellationToken);
        queueItem.Status.ShouldBe(LibrarySyncJobStatus.Failed);
        queueItem.CompletedAt.ShouldNotBeNull();
        queueItem.ErrorMessage.ShouldBe("Library comparison exceeded retry attempts while processing");
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<IScheduler>().Verify(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IScheduler>().Verify(x => x.ScheduleJob(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Test]
    public async Task ShouldScheduleSingleJobAndTrigger_WhenComparisonWorkerDoesNotExist()
    {
        // Arrange
        var command = new CheckQueuedLibraryComparisonJobCommand();
        await SetupDatabase(82, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        dbContext.LibraryComparisonJobQueues.Add(new LibraryComparisonJobQueue
        {
            RemotePlexLibraryId = libraries[0].Id,
            OwnedPlexLibraryId = libraries[1].Id,
            MediaType = PlexMediaType.Movie,
            Priority = 1,
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
    }
}
