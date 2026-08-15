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
        var ownedJobDetail = new Mock<IJobDetail>();
        ownedJobDetail
            .SetupGet(x => x.JobDataMap)
            .Returns(new PlexLibraryComparisonJobPayload(affectedLibraryId, 34).ToJobDataMap());
        var remoteJobDetail = new Mock<IJobDetail>();
        remoteJobDetail
            .SetupGet(x => x.JobDataMap)
            .Returns(new PlexLibraryComparisonJobPayload(56, affectedLibraryId).ToJobDataMap());
        var unaffectedJobKey = PlexLibraryComparisonJob.GetJobKey(7, 8);
        var unaffectedJobDetail = new Mock<IJobDetail>();
        unaffectedJobDetail
            .SetupGet(x => x.JobDataMap)
            .Returns(new PlexLibraryComparisonJobPayload(7, 8).ToJobDataMap());

        scheduler
            .Setup(x =>
                x.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.LibraryComparisonJob)), CancellationToken)
            )
            .ReturnsAsync([ownedJobKey, remoteJobKey, unaffectedJobKey]);
        scheduler.Setup(x => x.GetJobDetail(ownedJobKey, CancellationToken)).ReturnsAsync(ownedJobDetail.Object);
        scheduler.Setup(x => x.GetJobDetail(remoteJobKey, CancellationToken)).ReturnsAsync(remoteJobDetail.Object);
        scheduler
            .Setup(x => x.GetJobDetail(unaffectedJobKey, CancellationToken))
            .ReturnsAsync(unaffectedJobDetail.Object);
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
