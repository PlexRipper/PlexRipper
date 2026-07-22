using Quartz;

namespace Reaparr.BackgroundJobs.UnitTests;

public class CheckQueuedLibraryComparisonJobCommandHandlerUnitTests
    : BaseUnitTest<CheckQueuedLibraryComparisonJobCommandHandler>
{
    [Test]
    public async Task ShouldNotTriggerExistingJob_WhenComparisonWorkerIsAlreadyRunning()
    {
        // Arrange
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
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
        await dbContext.SaveChangesNewAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
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
        Mock.Mock<IScheduler>().Verify(x => x.TriggerJob(jobKey, It.IsAny<CancellationToken>()), Times.Never());
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
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
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
            Status = LibrarySyncJobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.CheckExists(jobKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());
        Mock.Mock<IScheduler>()
            .Setup(x => x.TriggerJob(jobKey, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<IScheduler>().Verify();
        Mock.Mock<IScheduler>()
            .Verify(
                x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }
}
