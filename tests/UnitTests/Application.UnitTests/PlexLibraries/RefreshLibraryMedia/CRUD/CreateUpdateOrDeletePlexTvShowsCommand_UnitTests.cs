using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class CreateUpdateOrDeletePlexTvShowsCommand_UnitTests : BaseUnitTest
{
    public CreateUpdateOrDeletePlexTvShowsCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldCreateAllTvShows_WhenNoneExists()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();

        var newTvShows = FakeData
            .GetPlexTvShows(
                45845,
                config =>
                {
                    config.TvShowSeasonCount = 2;
                    config.TvShowEpisodeCount = 5;
                }
            )
            .Generate(10);

        library.TvShows = newTvShows;

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

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
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
        });

        var library = IDbContext
            .PlexLibraries.Include(x => x.TvShows)
            .ThenInclude(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .First();
        library.ShouldNotBeNull();

        foreach (var x in library.TvShows)
            x.FullTitle = "TEST";

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

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
        Seed = 4503253;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 50;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var tvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();
        tvShows.ShouldNotBeNull();

        var newTvShows = tvShows.GetRange(0, 30);
        library.TvShows = newTvShows;

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldInsertSeasonsWithExistingTvShows_WhenSomeSeasonsAlreadyExist()
    {
        // Arrange
        Seed = 4503253;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var newTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        foreach (var tvShow in newTvShows)
            tvShow.Seasons.AddRange(FakeData.GetPlexTvShowSeason().Generate(3));

        library.TvShows = newTvShows;

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // var dbTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldNotCreateDuplicateKeys_WhenTheSameMediaIsCreated()
    {
        // Arrange
        Seed = 4503253;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
        });

        var library = IDbContext.PlexLibraries.First();
        library.ShouldNotBeNull();
        var newTvShows = IDbContext.PlexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes).ToList();

        library.TvShows = newTvShows;

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);

        var result = await handler.Handle(request, CancellationToken.None);
        var result2 = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        var tvShowsDb = IDbContext.PlexTvShows.Select(x => x.Key).ToHashSet();
        var seasonDb = IDbContext.PlexTvShowSeason.Select(x => x.Key).ToHashSet();
        var episodesDb = IDbContext.PlexTvShowEpisodes.Select(x => x.Key).ToHashSet();

        VerifyKeys(newTvShows);
    }

    [Fact]
    public async Task ShouldCreateUpdateAndDeleteMovies_WhenSomeExist()
    {
        // Arrange
        Seed = 4903259;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 50;
        });

        var library = IDbContext.PlexLibraries.First();
        var tvShowsDb = IDbContext.PlexTvShows.ToList();
        library.ShouldNotBeNull();

        var newTvShows = new List<PlexTvShow>();
        newTvShows.AddRange(tvShowsDb.GetRange(0, 10));
        newTvShows.AddRange(FakeData.GetPlexTvShows(45845).Generate(30));

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

        library.TvShows = newTvShows;

        // Act
        var request = new CreateUpdateOrDeletePlexTvShowsCommand(library);
        var handler = new CreateUpdateOrDeletePlexTvShowsCommandHandler(Log, IDbContext);
        var result = await handler.Handle(request, CancellationToken.None);

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
}
