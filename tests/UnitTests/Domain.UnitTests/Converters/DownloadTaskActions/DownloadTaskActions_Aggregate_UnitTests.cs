namespace Domain.UnitTests.Converters;

public class DownloadTaskActions_Aggregate_UnitTests : BaseUnitTest
{
    public DownloadTaskActions_Aggregate_UnitTests(ITestOutputHelper output)
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
        status.ShouldBe(DownloadStatus.Downloading);
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
        status.ShouldBe(DownloadStatus.Downloading);
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
        status.ShouldBe(DownloadStatus.Downloading);
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

    [Theory]
    [InlineData(DownloadStatus.ServerUnreachable)]
    [InlineData(DownloadStatus.Error)]
    [InlineData(DownloadStatus.MoveError)]
    [InlineData(DownloadStatus.MergeError)]
    [InlineData(DownloadStatus.MergeFinished)]
    [InlineData(DownloadStatus.MoveFinished)]
    [InlineData(DownloadStatus.Downloading)]
    [InlineData(DownloadStatus.Paused)]
    [InlineData(DownloadStatus.Stopped)]
    [InlineData(DownloadStatus.Merging)]
    [InlineData(DownloadStatus.Moving)]
    public void ShouldBeStatusXFinished_WhenAllButOneAreCompletedAndX(DownloadStatus anyStatus)
    {
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

    [Theory]
    [InlineData(DownloadStatus.Queued)]
    [InlineData(DownloadStatus.Downloading)]
    [InlineData(DownloadStatus.DownloadFinished)]
    [InlineData(DownloadStatus.Completed)]
    [InlineData(DownloadStatus.Deleted)]
    [InlineData(DownloadStatus.MergePaused)]
    [InlineData(DownloadStatus.MovePaused)]
    [InlineData(DownloadStatus.Unknown)]
    public void ShouldBeStatusX_WhenAllAreStatusX(DownloadStatus allStatus)
    {
        // Arrange
        var downloadStatusList = new List<DownloadStatus> { allStatus, allStatus, allStatus, allStatus, allStatus };

        // Act
        var status = DownloadTaskActions.Aggregate(downloadStatusList);

        // Assert
        status.ShouldBe(allStatus);
    }
}
