using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class RefreshPlexTvShowLibraryCommandUnitTests : BaseUnitTest<RefreshPlexTvShowLibraryCommandHandler>
{
    public RefreshPlexTvShowLibraryCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSuccessfullyRefreshLibraryAndUpdateSyncedAt_WhenTvShowsExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            9423,
            config =>
            {
                config.TvShowCount = 3;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 5;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.TvShows).First();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>()))
            .Returns(Task.CompletedTask);

        var seasonsList = FakeData.GetPlexTvShowSeason(seed).Generate(6);
        var episodesList = new List<PlexTvShowEpisode>();

        foreach (var season in seasonsList)
        {
            var episodes = FakeData.GetPlexTvShowEpisode(seed).Generate(30);
            foreach (var episode in episodes)
            {
                episode.ParentGuid = season.Guid;
                episode.MediaSize = 150000000;
                episode.Duration = 1800;
            }

            episodesList.AddRange(episodes);
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(seasonsList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaEpisodesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(episodesList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexTvShowsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new BulkInsertTvShowsRapport()));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ShouldSendProgressWithSeasonAndEpisodeItems_WhenTvShowsExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            9423,
            config =>
            {
                config.TvShowCount = 3;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 5;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.TvShows).First();
        var capturedItems = new List<LibraryProgressItem>();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<LibraryProgressItem>()))
            .Callback<int, LibraryProgressItem>((_, item) => capturedItems.Add(item))
            .Returns(Task.CompletedTask);

        var seasonsList = FakeData.GetPlexTvShowSeason(seed).Generate(6);
        var episodesList = new List<PlexTvShowEpisode>();

        foreach (var season in seasonsList)
        {
            var episodes = FakeData.GetPlexTvShowEpisode(seed).Generate(30);
            foreach (var episode in episodes)
            {
                episode.ParentGuid = season.Guid;
                episode.MediaSize = 150000000;
                episode.Duration = 1800;
            }

            episodesList.AddRange(episodes);
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(seasonsList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaEpisodesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(episodesList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexTvShowsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new BulkInsertTvShowsRapport()));

        // Act
        await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        capturedItems.ShouldNotBeEmpty();

        // At least one captured item must be a TvShow
        capturedItems.ShouldContain(i => i.MediaType == PlexMediaType.TvShow);

        // At least one captured item must be a Season (from phase 2+)
        capturedItems.ShouldContain(i => i.MediaType == PlexMediaType.Season);

        // At least one captured item must be an Episode (from phase 3+)
        capturedItems.ShouldContain(i => i.MediaType == PlexMediaType.Episode);
    }

    [Fact]
    public async Task ShouldReturnFailure_WhenLibraryIsNotTvShowType()
    {
        // Arrange
        await SetupDatabase(
            9424,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 1;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.First();

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("PlexLibrary is not of type TvShow");
    }

    [Fact]
    public async Task ShouldStillUpdateLibraryAndLogWarning_WhenNoTvShowsExist()
    {
        // Arrange
        await SetupDatabase(
            9425,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 0;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.First();

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturnFailure_WhenGetAllMediaSeasonsCommandFails()
    {
        // Arrange
        await SetupDatabase(
            9426,
            config =>
            {
                config.TvShowCount = 3;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.TvShows).First();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()))
            .Returns(Task.CompletedTask);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to get seasons"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to get seasons");
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()), Times.Once());
    }

    [Fact]
    public async Task ShouldReturnFailure_WhenGetAllMediaEpisodesCommandFails()
    {
        // Arrange
        var seed = await SetupDatabase(
            9427,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 3;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.TvShows).First();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()))
            .Returns(Task.CompletedTask);

        var seasonsList = FakeData.GetPlexTvShowSeason(seed).Generate(6);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(seasonsList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaEpisodesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to get episodes"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to get episodes");
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()), Times.Once());
    }

    [Fact]
    public async Task ShouldReturnFailure_WhenSyncPlexTvShowsCommandFails()
    {
        // Arrange
        var seed = await SetupDatabase(
            9428,
            config =>
            {
                config.TvShowCount = 3;
            }
        );
        var testLibrary = IDbContext.PlexLibraries.Include(x => x.TvShows).First();

        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()))
            .Returns(Task.CompletedTask);

        var seasonsList = FakeData.GetPlexTvShowSeason(seed).Generate(6);
        var episodesList = new List<PlexTvShowEpisode>();

        foreach (var season in seasonsList)
        {
            var episodes = FakeData.GetPlexTvShowEpisode(seed).Generate(30);
            foreach (var episode in episodes)
            {
                episode.ParentGuid = season.Guid;
                episode.MediaSize = 150000000;
                episode.Duration = 1800;
            }

            episodesList.AddRange(episodes);
        }

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(seasonsList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaEpisodesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(episodesList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<SyncPlexTvShowsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to sync TV shows"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary)),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to sync TV shows");
        Mock.Mock<ILibrarySyncProgressStore>()
            .Verify(x => x.UpdateErrorAsync(It.IsAny<int>(), It.IsAny<Result>()), Times.Once());
    }
}
