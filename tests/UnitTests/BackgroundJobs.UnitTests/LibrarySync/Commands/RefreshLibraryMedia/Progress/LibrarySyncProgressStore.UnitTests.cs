using Reaparr.BackgroundJobs.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class LibrarySyncProgressStoreUnitTests : BaseUnitTest<LibrarySyncProgressStore>
{
    public LibrarySyncProgressStoreUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSendInitialProgressUpdate_WhenStartAsyncIsCalled()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        // Act
        await Sut.StartAsync(1, PlexMediaType.Movie, TestContext.Current.CancellationToken);

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.PlexLibraryId.ShouldBe(1);
        capturedDto.Items.Count.ShouldBe(1);
        capturedDto.Items[0].MediaType.ShouldBe(PlexMediaType.Movie);

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Once()
            );
    }

    [Fact]
    public async Task ShouldSendProgressUpdate_WhenUpdateItemAsyncIsCalled()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await Sut.StartAsync(1, PlexMediaType.Movie, TestContext.Current.CancellationToken);

        var movieItem = new LibraryProgressItem
        {
            MediaType = PlexMediaType.Movie,
            Received = 50,
            Total = 200,
            TimeRemaining = TimeSpan.Zero,
        };

        // Act
        await Sut.UpdateItemAsync(1, movieItem, TestContext.Current.CancellationToken);

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.Items.ShouldHaveSingleItem();
        capturedDto.Items[0].MediaType.ShouldBe(PlexMediaType.Movie);
        capturedDto.Items[0].Received.ShouldBe(50);
        capturedDto.Items[0].Total.ShouldBe(200);

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );
    }

    [Fact]
    public async Task ShouldAggregateReceivedAndTotal_FromAllItems()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await Sut.StartAsync(2, PlexMediaType.TvShow, TestContext.Current.CancellationToken);

        // Pre-populate items via StartAsync and then update each one
        await Sut.UpdateItemAsync(
            2,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.TvShow,
                Received = 3,
                Total = 10,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            2,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Season,
                Received = 20,
                Total = 40,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            2,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Episode,
                Received = 100,
                Total = 500,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.Received.ShouldBe(123); // 3 + 20 + 100
        capturedDto.Total.ShouldBe(550); // 10 + 40 + 500

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4)
            );
    }

    [Fact]
    public async Task ShouldReportIsComplete_WhenAllItemsAreComplete()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await Sut.StartAsync(3, PlexMediaType.TvShow, TestContext.Current.CancellationToken);

        await Sut.UpdateItemAsync(
            3,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.TvShow,
                Received = 5,
                Total = 5,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            3,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Season,
                Received = 20,
                Total = 20,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            3,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Episode,
                Received = 200,
                Total = 200,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.IsComplete.ShouldBeTrue();
        capturedDto.Received.ShouldBe(capturedDto.Total);

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4)
            );
    }

    [Fact]
    public async Task ShouldNotReportIsComplete_WhenAnyItemIsIncomplete()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        await Sut.StartAsync(4, PlexMediaType.TvShow, TestContext.Current.CancellationToken);

        await Sut.UpdateItemAsync(
            4,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.TvShow,
                Received = 5,
                Total = 5,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            4,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Season,
                Received = 20,
                Total = 20,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );
        await Sut.UpdateItemAsync(
            4,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Episode,
                Received = 150,
                Total = 200,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.IsComplete.ShouldBeFalse();

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(4)
            );
    }

    [Fact]
    public async Task ShouldSendProgressThreeTimes_WhenStartAndTwoUpdateItemAsyncCalled()
    {
        // Arrange
        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        await Sut.StartAsync(5, PlexMediaType.TvShow, TestContext.Current.CancellationToken);

        var tvShowItem = new LibraryProgressItem
        {
            MediaType = PlexMediaType.TvShow,
            Received = 1,
            Total = 5,
            TimeRemaining = TimeSpan.Zero,
        };

        var episodeItem = new LibraryProgressItem
        {
            MediaType = PlexMediaType.Episode,
            Received = 50,
            Total = 100,
            TimeRemaining = TimeSpan.Zero,
        };

        // Act
        await Sut.UpdateItemAsync(5, tvShowItem, TestContext.Current.CancellationToken);
        await Sut.UpdateItemAsync(5, episodeItem, TestContext.Current.CancellationToken);

        // Assert — 1 from StartAsync + 2 from UpdateItemAsync
        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3)
            );
    }

    [Fact]
    public async Task ShouldMaintainCorrectPlexLibraryId_InProgressDTO()
    {
        // Arrange
        LibrarySyncProgressDTO? capturedDto = null;

        Mock.Mock<IProgressHubService>()
            .Setup(x =>
                x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>())
            )
            .Callback<LibrarySyncProgressDTO, CancellationToken>((dto, _) => capturedDto = dto)
            .Returns(Task.CompletedTask);

        const int plexLibraryId = 42;
        await Sut.StartAsync(plexLibraryId, PlexMediaType.Movie, TestContext.Current.CancellationToken);

        await Sut.UpdateItemAsync(
            plexLibraryId,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Movie,
                Received = 10,
                Total = 100,
                TimeRemaining = TimeSpan.Zero,
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        capturedDto.ShouldNotBeNull();
        capturedDto.PlexLibraryId.ShouldBe(plexLibraryId);

        Mock.Mock<IProgressHubService>()
            .Verify(
                x =>
                    x.SendLibraryProgressUpdateAsync(It.IsAny<LibrarySyncProgressDTO>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );
    }
}
