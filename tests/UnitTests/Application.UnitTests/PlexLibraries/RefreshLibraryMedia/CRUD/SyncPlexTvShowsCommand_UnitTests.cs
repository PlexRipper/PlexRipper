using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class SyncPlexTvShowsCommand_UnitTests : BaseUnitTest<SyncPlexTvShowsCommandHandler>
{
    private SyncPlexTvShowsCommandValidator _validator =
        new(LogManager.CreateLogInstance<SyncPlexTvShowsCommandValidator>());

    public SyncPlexTvShowsCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateAllTvShows_WhenNoneExists()
    {
        // Arrange
        var seed = await SetupDatabase(
            11865,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();

        var newTvShows = FakeData
            .GetPlexTvShows(
                seed,
                config =>
                {
                    config.TvShowSeasonCount = 2;
                    config.TvShowEpisodeCount = 5;
                }
            )
            .Generate(10);

        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var dbPlexTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        var dbSeasons = dbPlexTvShows.SelectMany(x => x.Seasons).ToList();
        var dbEpisodes = dbSeasons.SelectMany(x => x.Episodes).ToList();

        dbPlexTvShows.Count.ShouldBe(10);
        dbSeasons.Count.ShouldBe(20);
        dbEpisodes.Count.ShouldBe(100);

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldUpdateAllTvShows_WhenAllExists()
    {
        // Arrange
        await SetupDatabase(
            48674,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 5;
            }
        );

        var library = IDbContext
            .PlexLibraries.Include(x => x.TvShows)
            .ThenInclude(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .First();
        library.ShouldNotBeNull();

        foreach (var x in library.TvShows)
            x.FullTitle = "TEST";

        var newTvShows = library.TvShows;
        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexTvShows = IDbContext.PlexTvShows.ToList();
        plexTvShows.Count.ShouldBe(10);
        foreach (var plexTvShow in plexTvShows)
            plexTvShow.FullTitle.ShouldBe("TEST");

        VerifyKeys(library.TvShows);
    }

    [Fact]
    public async Task ShouldDeleteTwentyTvShows_WhenSomeExist()
    {
        // Arrange
        await SetupDatabase(
            4503253,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 50;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var tvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();
        tvShows.ShouldNotBeNull();

        var newTvShows = tvShows.GetRange(0, 30);

        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldInsertSeasonsWithExistingTvShows_WhenSomeSeasonsAlreadyExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            34284,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 5;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var newTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        foreach (var tvShow in newTvShows)
            tvShow.Seasons.AddRange(FakeData.GetPlexTvShowSeason(seed).Generate(3));

        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // var dbTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldNotCreateDuplicateKeys_WhenTheSameMediaIsCreated()
    {
        // Arrange
        await SetupDatabase(
            16344,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 5;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var newTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);

        var result = await _sut.Handle(request, CancellationToken.None);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result2 = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldCreateUpdateAndDeleteMovies_WhenSomeExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            85746,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 50;
            }
        );

        var library = IDbContext.PlexLibraries.First();
        var tvShowsDb = IDbContext.PlexTvShows.ToList();
        library.ShouldNotBeNull();

        var newTvShows = new List<PlexTvShow>();
        newTvShows.AddRange(tvShowsDb.GetRange(0, 10));
        newTvShows.AddRange(FakeData.GetPlexTvShows(seed).Generate(30));

        for (var i = 0; i < newTvShows.Count; i++)
        {
            // Create updated TvShows based on the Key
            if (i is >= 10 and < 30)
            {
                newTvShows[i].Key = tvShowsDb[i].Key;
                newTvShows[i].UpdatedAt = DateTime.Now;
            }

            newTvShows[i].Id = 0;
            newTvShows[i].Title = $"TEST - {newTvShows[i].Title}";
        }

        SetIds(library, newTvShows);

        // Act
        var request = new SyncPlexTvShowsCommand(newTvShows);
        (await _validator.ValidateAsync(request)).IsValid.ShouldBeTrue();
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var plexTvShows = IDbContext.PlexTvShows.ToList();
        plexTvShows.Count.ShouldBe(40);
        foreach (var plexTvShow in newTvShows)
        {
            var findResult = plexTvShows.Find(x => x.Key == plexTvShow.Key);
            findResult.ShouldNotBeNull();
            findResult.Title.Contains("TEST").ShouldBeTrue();
        }

        VerifyKeys(newTvShows);
    }

    private void VerifyKeys(List<PlexTvShow> newTvShows)
    {
        var plexLibraryId = newTvShows.First().PlexLibraryId;
        var dbPlexTvShows = IDbContext
            .PlexTvShows.Include(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .ToList();
        var dbSeasons = dbPlexTvShows.SelectMany(x => x.Seasons).ToList();
        var dbEpisodes = dbSeasons.SelectMany(x => x.Episodes).ToList();

        dbPlexTvShows.All(x => newTvShows.Any(y => y.Key == x.Key)).ShouldBeTrue();
        dbSeasons.All(x => newTvShows.SelectMany(y => y.Seasons).Any(y => y.Key == x.Key)).ShouldBeTrue();
        dbEpisodes
            .All(x => newTvShows.SelectMany(y => y.Seasons.SelectMany(z => z.Episodes)).Any(y => y.Key == x.Key))
            .ShouldBeTrue();
    }

    private void SetIds(PlexLibrary library, List<PlexTvShow> newTvShows)
    {
        foreach (var plexTvShow in newTvShows)
        {
            plexTvShow.PlexLibraryId = library.Id;
            plexTvShow.PlexServerId = library.PlexServerId;
            foreach (var season in plexTvShow.Seasons)
            {
                season.PlexLibraryId = library.Id;
                season.PlexServerId = library.PlexServerId;

                foreach (var episode in season.Episodes)
                {
                    episode.PlexLibraryId = library.Id;
                    episode.PlexServerId = library.PlexServerId;
                }
            }
        }
    }
}
