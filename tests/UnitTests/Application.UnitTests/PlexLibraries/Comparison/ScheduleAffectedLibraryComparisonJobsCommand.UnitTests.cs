using TickerQ.Utilities.Models;

namespace Reaparr.Application.UnitTests;

public class ScheduleAffectedLibraryComparisonJobsCommandUnitTests
    : BaseUnitTest<ScheduleAffectedLibraryComparisonJobsCommandHandler>
{
    [Test]
    public async Task ShouldNotDispatchComparison_WhenPairAlreadyHasActiveTicker()
    {
        // Arrange
        await SetupDatabase(91112, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });

        var libraries = await IDbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var jobKey = PlexLibraryComparisonJob.GetJobKey(ownedLibrary.Id, remoteLibrary.Id);
        var ticker = new JobTimeTicker
        {
            Function = nameof(PlexLibraryComparisonJob),
            Request = [],
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };
        await IDbContext.BulkInsertAsync([ticker], cancellationToken: CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(
            new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool isOwned)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, isOwned), CancellationToken);
    }
}