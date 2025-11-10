namespace Reaparr.Domain.UnitTests;

public class DownloadTaskActionsAggregateUnitTests : BaseUnitTest
{
    public DownloadTaskActionsAggregateUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldBeStatusUnknown_WhenListIsEmpty()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>();

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Unknown);
    }

    [Fact]
    public void ShouldBeStatusDownloading_WhenSomeAreDownloadFinished()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Downloading,
            DownloadStatus.Downloading,
            DownloadStatus.DownloadFinished,
            DownloadStatus.DownloadFinished,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Downloading);
    }

    [Fact]
    public void ShouldBeStatusDownloading_WhenOneIsQueuedAndOneIsDownloadFinished()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.DownloadFinished,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Queued,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public void ShouldBeStatusDownloading_WhenSomeAreDownloadFinishedAndQueued()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.DownloadFinished,
            DownloadStatus.DownloadFinished,
            DownloadStatus.Queued,
            DownloadStatus.Queued,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public void ShouldBeStatusDownloading_WhenSomeAreDownloadFinishedQueuedAndCompleted()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.DownloadFinished,
            DownloadStatus.Queued,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public void ShouldBeStatusDownloadFinished_WhenSomeAreDownloadFinishedAndCompleted()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.DownloadFinished,
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.DownloadFinished,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.DownloadFinished);
    }

    [Fact]
    public void ShouldBeStatusQueued_WhenSomeAreQueuedAndCompleted()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Completed,
            DownloadStatus.Completed,
            DownloadStatus.Queued,
            DownloadStatus.Queued,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Queued);
    }

    [Fact]
    public void ShouldPrioritizeAnyStatuses_WhenMixedWithAllStatuses()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Queued,
            DownloadStatus.Downloading,
            DownloadStatus.Error, // This is from `anyStatuses`
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Error); // `anyStatuses` should take precedence.
    }

    [Fact]
    public void ShouldHandleDuplicateStatusesCorrectly()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Downloading,
            DownloadStatus.Downloading,
            DownloadStatus.Error,
            DownloadStatus.Error,
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.Error); // First matching `anyStatus`.
    }

    [Fact]
    public void ShouldReturnFirstMatchingAnyStatus_WhenMultipleAnyStatusesArePresent()
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus>
        {
            DownloadStatus.Downloading,
            DownloadStatus.Error,
            DownloadStatus.ServerUnreachable, // Appears earlier in `anyStatuses`
        };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(DownloadStatus.ServerUnreachable); // First match in `anyStatuses`
    }

    [Fact]
    public void ShouldBeStatusXFinished_WhenAllButOneAreCompletedAndX()
    {
        var allStatuses = Enum.GetValues<DownloadStatus>().ToList();

        foreach (var anyStatus in allStatuses)
        {
            if (anyStatus == DownloadStatus.Unknown)
            {
                // Skip Unknown status as it should always be wrong
                continue;
            }

            // Arrange
            var downloadStatusList = new List<DownloadStatus>
            {
                DownloadStatus.Completed,
                DownloadStatus.Completed,
                DownloadStatus.Completed,
                DownloadStatus.Completed,
                anyStatus,
            };

            // Act
            var aggregateStatus = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            aggregateStatus.ShouldBe(anyStatus);
        }
    }

    [Fact]
    public void ShouldBeStatusX_WhenAllAreStatusX()
    {
        var allStatuses = Enum.GetValues<DownloadStatus>().ToList();

        foreach (var status in allStatuses)
        {
            // Arrange
            var downloadStatusList = new List<DownloadStatus> { status, status, status, status, status };

            // Act
            var aggregateStatus = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            aggregateStatus.ShouldBe(status);
        }
    }
}
