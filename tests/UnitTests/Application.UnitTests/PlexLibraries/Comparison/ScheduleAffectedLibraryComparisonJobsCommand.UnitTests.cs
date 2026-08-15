using Quartz;
using Quartz.Impl.Matchers;

namespace Reaparr.Application.UnitTests;

public class ScheduleAffectedLibraryComparisonJobsCommandUnitTests
    : BaseUnitTest<ScheduleAffectedLibraryComparisonJobsCommandHandler>
{
    [Test]
    public async Task ShouldNotDispatchComparison_WhenPairAlreadyHasActiveTicker()
    {
        // Arrange
        await SetupDatabase(
            91112,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var libraries = await IDbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var jobKey = PlexLibraryComparisonJob.GetJobKey(ownedLibrary.Id, remoteLibrary.Id);
        Mock.Mock<IScheduler>()
            .Setup(x =>
                x.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.LibraryComparisonJob)), CancellationToken)
            )
            .ReturnsAsync([jobKey]);

        // Act
        var result = await Sut.ExecuteAsync(
            new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJobs(
                        It.IsAny<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(),
                        false,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
    }

    [Test]
    public async Task ShouldBatchComparisonJobsAndInvalidateDistinctLibraries_WhenRemoteLibraryChanges()
    {
        // Arrange
        await SetupDatabase(
            79,
            config =>
            {
                config.PlexServerCount = 3;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var libraries = await IDbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibraries = libraries.Skip(1).ToList();
        var ownedServerIds = ownedLibraries.Select(x => x.PlexServerId).ToList();

        await IDbContext
            .PlexServers.Where(x => x.Id == remoteLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await IDbContext
            .PlexServers.Where(x => ownedServerIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id);
        Mock.Mock<IScheduler>()
            .Setup(x =>
                x.ScheduleJobs(
                    It.IsAny<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(),
                    false,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJobs(
                        It.Is<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(jobs =>
                            jobs.Count == ownedLibraries.Count
                            && jobs.Keys.All(job =>
                                job.Key.Group == nameof(JobTypes.LibraryComparisonJob)
                                && job.JobDataMap.GetPayload<PlexLibraryComparisonJobPayload>()!.OwnedPlexLibraryId
                                    != remoteLibrary.Id
                                && job.JobDataMap.GetPayload<PlexLibraryComparisonJobPayload>()!.RemotePlexLibraryId
                                    == remoteLibrary.Id
                            )
                        ),
                        false,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Test]
    public async Task ShouldBatchRemoteLibraries_WhenOwnedLibraryChanges()
    {
        // Arrange
        await SetupDatabase(
            81,
            config =>
            {
                config.PlexServerCount = 3;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var libraries = await IDbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibraries = libraries.Take(2).ToList();
        var ownedLibrary = libraries[2];
        var remoteServerIds = remoteLibraries.Select(x => x.PlexServerId).ToList();

        await IDbContext
            .PlexServers.Where(x => remoteServerIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await IDbContext
            .PlexServers.Where(x => x.Id == ownedLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(ownedLibrary.Id);
        Mock.Mock<IScheduler>()
            .Setup(x =>
                x.ScheduleJobs(
                    It.IsAny<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(),
                    false,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJobs(
                        It.Is<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(jobs =>
                            jobs.Count == remoteLibraries.Count
                            && jobs.Keys.All(job =>
                                job.Key.Group == nameof(JobTypes.LibraryComparisonJob)
                                && job.JobDataMap.GetPayload<PlexLibraryComparisonJobPayload>()!.OwnedPlexLibraryId
                                    == ownedLibrary.Id
                                && remoteLibraries
                                    .Select(x => x.Id)
                                    .Contains(job.JobDataMap.GetPayload<PlexLibraryComparisonJobPayload>()!.RemotePlexLibraryId)
                            )
                        ),
                        false,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Test]
    public async Task ShouldNotQueueOrInvalidate_WhenNoCompatibleOwnedLibraryExists()
    {
        // Arrange
        await SetupDatabase(
            82,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );
        var libraries = await IDbContext.PlexLibraries.ToListAsync(CancellationToken);
        var serverIds = libraries.Select(x => x.PlexServerId).ToList();
        await IDbContext
            .PlexServers.Where(x => serverIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        var command = new ScheduleAffectedLibraryComparisonJobsCommand(libraries[0].Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>()
            .Verify(
                x =>
                    x.ScheduleJobs(
                        It.IsAny<IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>>(),
                        false,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool isOwned)
    {
        await IDbContext
            .PlexServers.Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, isOwned), CancellationToken);
    }
}
