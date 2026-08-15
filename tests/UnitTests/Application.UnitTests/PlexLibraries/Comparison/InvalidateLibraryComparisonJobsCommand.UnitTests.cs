using Quartz;
using Quartz.Impl.Matchers;

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
        var scheduler = Mock.Mock<IScheduler>();
        scheduler
            .Setup(x =>
                x.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.LibraryComparisonJob)), CancellationToken)
            )
            .ReturnsAsync([ownedJobKey, remoteJobKey, PlexLibraryComparisonJob.GetJobKey(7, 8)]);
        scheduler
            .Setup(x =>
                x.DeleteJobs(
                    It.Is<IReadOnlyCollection<JobKey>>(keys =>
                        keys.Count == 2 && keys.Any(x => x == ownedJobKey) && keys.Any(x => x == remoteJobKey)
                    ),
                    CancellationToken
                )
            )
            .ReturnsAsync(true);

        // Act
        var result = await Sut.ExecuteAsync(
            new InvalidateLibraryComparisonJobsCommand([affectedLibraryId]),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        scheduler.VerifyAll();
    }
}
