using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
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
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updatedLibrary = await IDbContext.PlexLibraries.GetAsync(testLibrary.Id, CancellationToken);
        updatedLibrary.ShouldNotBeNull();
        updatedLibrary.SyncedAt.ShouldNotBeNull();
        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Verify(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()), Times.AtLeastOnce);
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
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
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
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to get seasons"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to get seasons");
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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()));

        var seasonsList = FakeData.GetPlexTvShowSeason(seed).Generate(6);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaSeasonsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(seasonsList));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetAllMediaEpisodesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to get episodes"));

        // Act
        var result = await Sut.ExecuteAsync(
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to get episodes");
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

        Mock.Mock<IRefreshLibraryProgressReporter>()
            .Setup(x => x.SendProgress(It.IsAny<RefreshLibraryProgressUpdate>()))
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
            new RefreshPlexTvShowLibraryCommand(new InsertMediaMetaDataCommandResponse(testLibrary), _ => { }),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to sync TV shows");
    }
}
