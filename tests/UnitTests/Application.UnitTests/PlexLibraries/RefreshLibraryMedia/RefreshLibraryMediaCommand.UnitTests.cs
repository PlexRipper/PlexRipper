using Application.Contracts;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application.UnitTests;

public class RefreshLibraryMediaCommand_UnitTests : BaseUnitTest<RefreshLibraryMediaCommandHandler>
{
    public RefreshLibraryMediaCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Theory]
    [InlineData(PlexMediaType.Movie)]
    [InlineData(PlexMediaType.TvShow)]
    public async Task ShouldMarkTheLibraryAsSynced_WhenThereIsNoMediaReturned(PlexMediaType libraryType)
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
        });

        var updatedPlexLibrary = await GetUpdatedLibrary(libraryType);

        mock.Mock<IPlexApiService>()
            .Setup(x =>
                x.GetLibraryMediaAsync(
                    It.IsAny<PlexLibrary>(),
                    It.IsAny<Action<MediaSyncProgress>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(updatedPlexLibrary));
        mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Returns(Task.CompletedTask);
        mock.SetupMediator(It.IsAny<SyncPlexMoviesCommand>).ReturnsAsync(Result.Ok(new CrudMoviesReport()));

        // Act
        var request = new RefreshLibraryMediaCommand(updatedPlexLibrary.Id);
        var handler = mock.Create<RefreshLibraryMediaCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var libraryDb = await IDbContext
            .PlexLibraries.Where(x => x.Key == updatedPlexLibrary.Key)
            .FirstOrDefaultAsync();
        libraryDb.ShouldNotBeNull();
        libraryDb.SyncedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldSyncTvShowLibrarySuccessfully_WhenMediaDataIsReturnedFromTheAPI()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
        });

        var updatedPlexLibrary = await GetUpdatedLibrary(PlexMediaType.TvShow);

        var rawTvShowData = FakeData.GetPlexTvShows().Generate(10);
        var rawSeasonData = FakeData.GetPlexTvShowSeason().Generate(100);
        var rawEpisodesData = FakeData.GetPlexTvShowEpisode().Generate(1000);

        updatedPlexLibrary.TvShows = rawTvShowData;

        // Set keys
        var seasonIndex = 0;
        var episodeIndex = 0;

        foreach (var tvShow in rawTvShowData)
            // Assign 10 seasons to each TV show
            for (var i = 0; i < 10 && seasonIndex < rawSeasonData.Count; i++)
            {
                var season = rawSeasonData[seasonIndex];
                season.ParentKey = tvShow.Key;
                seasonIndex++;

                // Assign 10 episodes to each season
                for (var j = 0; j < 10 && episodeIndex < rawEpisodesData.Count; j++)
                {
                    var episode = rawEpisodesData[episodeIndex];
                    episode.ParentKey = season.Key;
                    episodeIndex++;
                }
            }

        mock.Mock<IPlexApiService>()
            .Setup(x =>
                x.GetLibraryMediaAsync(
                    It.IsAny<PlexLibrary>(),
                    It.IsAny<Action<MediaSyncProgress>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(updatedPlexLibrary));

        mock.Mock<IPlexApiService>()
            .Setup(x =>
                x.GetAllSeasonsAsync(
                    It.IsAny<PlexLibrary>(),
                    It.IsAny<Action<MediaSyncProgress>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(rawSeasonData));

        mock.Mock<IPlexApiService>()
            .Setup(x =>
                x.GetAllEpisodesAsync(
                    It.IsAny<PlexLibrary>(),
                    It.IsAny<Action<MediaSyncProgress>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(rawEpisodesData));

        mock.Mock<ISignalRService>()
            .Setup(x => x.SendLibraryProgressUpdateAsync(It.IsAny<LibraryProgress>()))
            .Returns(Task.CompletedTask);

        mock.SetupMediator(It.IsAny<SyncPlexTvShowsCommand>).ReturnsAsync(Result.Ok(new CrudTvShowsReport()));

        // Act
        var request = new RefreshLibraryMediaCommand(updatedPlexLibrary.Id);
        var handler = mock.Create<RefreshLibraryMediaCommandHandler>();
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        var dbContext = IDbContext;
        var libraryDb = await dbContext.PlexLibraries.Where(x => x.Key == updatedPlexLibrary.Key).FirstOrDefaultAsync();
        libraryDb.ShouldNotBeNull();
        libraryDb.SyncedAt.ShouldNotBeNull();

        // Verify that the merge was successful
        Func<List<PlexTvShow>, bool> isValid = tvShows =>
        {
            tvShows.ShouldNotBeNull();
            tvShows.Count.ShouldBe(10);
            tvShows.SelectMany(x => x.Seasons).Count().ShouldBe(100);
            tvShows.SelectMany(x => x.Seasons).SelectMany(x => x.Episodes).Count().ShouldBe(1000);
            return true;
        };

        mock.Mock<IMediator>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<SyncPlexTvShowsCommand>(command => isValid(command.PlexTvShows)),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    private async Task<PlexLibrary> GetUpdatedLibrary(PlexMediaType type)
    {
        var plexLibrary = await IDbContext.PlexLibraries.Where(x => x.Type == type).FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        var mockPlexLibrary = FakeData.GetPlexLibrary(libraryType: type).Generate();
        return new PlexLibrary
        {
            Id = plexLibrary.Id,
            Type = type,
            Title = mockPlexLibrary.Title,
            Key = plexLibrary.Key,
            CreatedAt = mockPlexLibrary.CreatedAt,
            UpdatedAt = mockPlexLibrary.UpdatedAt,
            ScannedAt = mockPlexLibrary.ScannedAt,
            SyncedAt = null,
            Uuid = mockPlexLibrary.Uuid,
            MetaData = mockPlexLibrary.MetaData,
            PlexServer = mockPlexLibrary.PlexServer,
            PlexServerId = plexLibrary.PlexServerId,
            DefaultDestination = mockPlexLibrary.DefaultDestination,
            DefaultDestinationId = mockPlexLibrary.DefaultDestinationId,
            Movies = mockPlexLibrary.Movies,
            TvShows = mockPlexLibrary.TvShows,
            PlexAccountLibraries = mockPlexLibrary.PlexAccountLibraries,
        };
    }
}
