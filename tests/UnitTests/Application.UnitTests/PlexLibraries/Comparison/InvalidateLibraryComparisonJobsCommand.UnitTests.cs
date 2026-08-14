using TickerQ.Utilities.Models;

namespace Reaparr.Application.UnitTests;

public class InvalidateLibraryComparisonJobsCommandUnitTests
    : BaseUnitTest<InvalidateLibraryComparisonJobsCommandHandler>
{
    [Test]
    public async Task ShouldDeleteExactComparisonJobKeysForAffectedLibrary()
    {
        // Arrange
        await SetupDatabase(91114);

        var affectedLibraryId = 12;
        var ownedJobKey = PlexLibraryComparisonJob.GetJobKey(affectedLibraryId, 34);
        var remoteJobKey = PlexLibraryComparisonJob.GetJobKey(56, affectedLibraryId);
        var dbContext = IDbContext;
        dbContext.TimeTickers.AddRange(
            CreateTicker(ownedJobKey, affectedLibraryId, 34),
            CreateTicker(remoteJobKey, 56, affectedLibraryId),
            CreateTicker(PlexLibraryComparisonJob.GetJobKey(7, 8), 7, 8)
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IBackgroundJobScheduler>()
            .Setup(x =>
                x.DeleteBatchJobs(
                    It.Is<IReadOnlyCollection<JobKey>>(keys =>
                        keys.Count == 2 && keys.Any(x => x == ownedJobKey) && keys.Any(x => x == remoteJobKey)
                    ),
                    CancellationToken
                )
            )
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(
            new InvalidateLibraryComparisonJobsCommand([affectedLibraryId]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IBackgroundJobScheduler>().VerifyAll();
    }

    private static JobTimeTicker CreateTicker(JobKey jobKey, int ownedPlexLibraryId, int remotePlexLibraryId) =>
        new()
        {
            Function = nameof(PlexLibraryComparisonJob),
            Request = [],
            RequestJson = new JobTimeTickerRequestProperties
            {
                OwnedPlexLibraryId = ownedPlexLibraryId,
                RemotePlexLibraryId = remotePlexLibraryId,
            },
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };
}
