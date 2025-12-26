using Reaparr.Application.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class RefreshLibraryProgressReporterUnitTests : BaseUnitTest<RefreshLibraryProgressReporter>
{
    public RefreshLibraryProgressReporterUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.TvShow, 1, 0, 0)] // Step 1, 0% = 0
    [InlineData(PlexMediaType.TvShow, 1, 0.5, 100)] // Step 1, 50% = 100
    [InlineData(PlexMediaType.TvShow, 1, 1, 200)] // Step 1, 100% = 200
    [InlineData(PlexMediaType.TvShow, 5, 0, 800)] // Step 5, 0% = 800
    [InlineData(PlexMediaType.TvShow, 5, 0.5, 900)] // Step 5, 50% = 900
    [InlineData(PlexMediaType.TvShow, 5, 1, 1000)] // Step 5, 100% = 1000 (not 1200!)
    [InlineData(PlexMediaType.Movie, 1, 0, 0)] // Step 1, 0% = 0
    [InlineData(PlexMediaType.Movie, 1, 1, 333)] // Step 1, 100% = 333 (1000/3)
    [InlineData(PlexMediaType.Movie, 3, 1, 1000)] // Step 3, 100% = 1000 (not 1333!)
    public async Task ShouldCalculateReceivedCorrectly_WhenProgressIsSent(
        PlexMediaType libraryType,
        int step,
        decimal percentage,
        int expectedReceived
    )
    {
        // Arrange
        LibraryProgress? capturedProgress = null;

        Mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Callback<LibraryProgress>(p => capturedProgress = p)
            .Returns(Task.CompletedTask);

        var update = new RefreshLibraryProgressUpdate
        {
            PlexLibraryId = 1,
            PlexLibraryType = libraryType,
            Step = step,
            Percentage = percentage,
            Action = _ => { },
        };

        // Act
        await Sut.SendProgress(update);

        // Assert
        capturedProgress.ShouldNotBeNull();
        capturedProgress.Received.ShouldBe(expectedReceived);
        capturedProgress.Total.ShouldBe(1000);
        capturedProgress.Received.ShouldBeLessThanOrEqualTo(capturedProgress.Total);

        Mock.Mock<ISignalRService>()
            .Verify(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()), Times.Once());
    }

    [Fact]
    public async Task ShouldNotExceedTotal_WhenAtFinalStepWithFullPercentage()
    {
        // Arrange
        LibraryProgress? capturedProgress = null;

        Mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Callback<LibraryProgress>(p => capturedProgress = p)
            .Returns(Task.CompletedTask);

        var update = new RefreshLibraryProgressUpdate
        {
            PlexLibraryId = 93,
            PlexLibraryType = PlexMediaType.TvShow,
            Step = 5,
            Percentage = 1.0m, // 100%
            TimeRemaining = TimeSpan.Zero,
            Action = _ => { },
        };

        // Act
        await Sut.SendProgress(update);

        // Assert
        capturedProgress.ShouldNotBeNull();
        capturedProgress.Received.ShouldBe(1000);
        capturedProgress.Total.ShouldBe(1000);
        capturedProgress.Percentage.ShouldBe(100m);
        capturedProgress.Received.ShouldBeLessThanOrEqualTo(capturedProgress.Total);

        Mock.Mock<ISignalRService>()
            .Verify(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()), Times.Once());
    }

    [Fact]
    public async Task ShouldInvokeActionCallback_WhenProgressIsSent()
    {
        // Arrange
        LibraryProgress? callbackProgress = null;

        Mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Returns(Task.CompletedTask);

        var update = new RefreshLibraryProgressUpdate
        {
            PlexLibraryId = 1,
            PlexLibraryType = PlexMediaType.TvShow,
            Step = 1,
            Percentage = 0.5m,
            Action = p => callbackProgress = p,
        };

        // Act
        await Sut.SendProgress(update);

        // Assert
        callbackProgress.ShouldNotBeNull();
        callbackProgress.Id.ShouldBe(1);
        callbackProgress.Step.ShouldBe(1);
        callbackProgress.TotalSteps.ShouldBe(5);

        Mock.Mock<ISignalRService>()
            .Verify(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()), Times.Once());
    }

    [Fact]
    public async Task ShouldSetCorrectTotalSteps_BasedOnLibraryType()
    {
        // Arrange
        LibraryProgress? tvShowProgress = null;
        LibraryProgress? movieProgress = null;

        Mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Callback<LibraryProgress>(p =>
            {
                if (p.TotalSteps == 5)
                    tvShowProgress = p;
                else if (p.TotalSteps == 3)
                    movieProgress = p;
            })
            .Returns(Task.CompletedTask);

        var tvShowUpdate = new RefreshLibraryProgressUpdate
        {
            PlexLibraryId = 1,
            PlexLibraryType = PlexMediaType.TvShow,
            Step = 1,
            Percentage = 0.5m,
            Action = _ => { },
        };

        var movieUpdate = new RefreshLibraryProgressUpdate
        {
            PlexLibraryId = 2,
            PlexLibraryType = PlexMediaType.Movie,
            Step = 1,
            Percentage = 0.5m,
            Action = _ => { },
        };

        // Act
        await Sut.SendProgress(tvShowUpdate);
        await Sut.SendProgress(movieUpdate);

        // Assert
        tvShowProgress.ShouldNotBeNull();
        tvShowProgress.TotalSteps.ShouldBe(5);

        movieProgress.ShouldNotBeNull();
        movieProgress.TotalSteps.ShouldBe(3);

        Mock.Mock<ISignalRService>()
            .Verify(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()), Times.Exactly(2));
    }
}
