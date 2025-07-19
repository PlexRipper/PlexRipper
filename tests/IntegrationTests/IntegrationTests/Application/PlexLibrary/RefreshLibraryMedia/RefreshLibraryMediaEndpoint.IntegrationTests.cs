using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests;

public class RefreshLibraryMediaEndpointIntegrationTests : BaseIntegrationTests
{
    public RefreshLibraryMediaEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldFullyRefreshLibraryMedia_WhenPlexLibraryIsOfTypeMovieAndCommandIsSent()
    {
        // Arrange
        var seed = new Seed(8932);
        const int serverCount = 1;
        const int libraryCount = 3;
        const int movieCount = 500;
        const int roleCount = 5;
        const int genreCount = 2;
        const int countryCount = 2;
        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.HttpClientOptions = (x, _) =>
                {
                    x.SetupIdentityRequest(seed);
                };

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexMovieLibraryCount = 1;
                    x.MovieCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.GenerateFromDatabase = true;
                    x.PlexServerAccessCount = serverCount;
                    x.MovieLibraryCount = libraryCount;
                    x.MoviesPerLibraryCount = movieCount;
                    x.RolePerMediaItemCount = roleCount;
                    x.CountriesPerMediaItemCount = countryCount;
                    x.GenrePerMediaItemCount = genreCount;
                };
            }
        );

        var plexLibrary = await container.DbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.GETAsync<
            RefreshLibraryMediaEndpoint,
            RefreshLibraryMediaEndpointRequest,
            ResultDTO<PlexLibraryDTO>
        >(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id));

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue();

        // Verify the library was refreshed
        var dbContext = container.DbContext;
        var refreshedLibrary = await dbContext
            .PlexLibraries.Include(x => x.Movies)
            .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id);

        refreshedLibrary.ShouldNotBeNull();
        refreshedLibrary.Movies.Count.ShouldBe(movieCount);
        refreshedLibrary.SyncedAt.ShouldNotBeNull();
        refreshedLibrary.ActorsCount.ShouldBe(roleCount * movieCount);
        refreshedLibrary.GenresCount.ShouldBe(genreCount * movieCount);
        refreshedLibrary.CountriesCount.ShouldBe(countryCount * movieCount);

        var movieActorCount = await dbContext.PlexMovieActors.CountAsync();
        movieActorCount.ShouldBe(roleCount * movieCount);

        var genreMovieCount = await dbContext.PlexMovieGenres.CountAsync();
        genreMovieCount.ShouldBe(genreCount * movieCount);

        var countryMovieCount = await dbContext.PlexMovieCountries.CountAsync();
        countryMovieCount.ShouldBe(countryCount * movieCount);

        // Verify media data
        var movies = await dbContext.PlexMovies.IncludeMediaData().ToListAsync();
        movies.ShouldNotBeEmpty();
        movies.Count.ShouldBe(movieCount);

        var mediaList = movies.SelectMany(x => x.MediaDataList).ToList();
        mediaList.Count.ShouldBeGreaterThanOrEqualTo(movieCount);

        var parts = mediaList.SelectMany(x => x.Parts).ToList();
        parts.Count.ShouldBeGreaterThanOrEqualTo(movieCount);
    }

    [Fact]
    public async Task ShouldFullyRefreshLibraryMedia_WhenPlexLibraryIsOfTypeTvShowAndCommandIsSent()
    {
        // Arrange
        var seed = new Seed(8932);
        const int serverCount = 1;
        const int libraryCount = 3;
        const int tvShowCount = 100;
        const int seasonCount = 5;
        const int episodeCount = 10;
        const int roleCount = 5;
        const int genreCount = 2;
        const int countryCount = 2;
        const int totalEpisodeCount = tvShowCount * seasonCount * episodeCount;

        using var container = await CreateContainer(
            seed,
            config =>
            {
                config.HttpClientOptions = (x, _) => x.SetupIdentityRequest(seed);

                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexTvShowLibraryCount = 1;
                    x.TvShowCount = 0;
                };

                config.BaseMockHttpClientOptions = x =>
                {
                    x.GenerateFromDatabase = true;
                    x.PlexServerAccessCount = serverCount;
                    x.TvShowLibraryCount = libraryCount;
                    x.TvShowsPerLibraryCount = tvShowCount;
                    x.SeasonsPerTvShowCount = seasonCount;
                    x.EpisodesPerSeasonCount = episodeCount;
                    x.RolePerMediaItemCount = roleCount;
                    x.CountriesPerMediaItemCount = countryCount;
                    x.GenrePerMediaItemCount = genreCount;
                };
            }
        );

        var plexLibrary = await container.DbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var client = container.GetApiClient();
        await client.SignIn();

        var testResult = await client.GETAsync<
            RefreshLibraryMediaEndpoint,
            RefreshLibraryMediaEndpointRequest,
            ResultDTO<PlexLibraryDTO>
        >(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id));

        // Assert
        var result = testResult.Result;
        result.IsSuccess.ShouldBeTrue();
        testResult.Response.IsSuccessStatusCode.ShouldBeTrue();

        // Verify the library was refreshed
        var dbContext = container.DbContext;
        var refreshedLibrary = await dbContext
            .PlexLibraries.Include(x => x.TvShows)
            .ThenInclude(x => x.Seasons)
            .ThenInclude(x => x.Episodes)
            .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id);

        refreshedLibrary.ShouldNotBeNull();
        refreshedLibrary.TvShows.Count.ShouldBe(tvShowCount);
        refreshedLibrary.TvShows.Sum(x => x.Seasons.Count).ShouldBe(tvShowCount * seasonCount);
        refreshedLibrary.TvShows.Sum(x => x.Seasons.Sum(s => s.Episodes.Count)).ShouldBe(totalEpisodeCount);
        refreshedLibrary.SyncedAt.ShouldNotBeNull();
        refreshedLibrary.ActorsCount.ShouldBeGreaterThan(0);
        refreshedLibrary.GenresCount.ShouldBeGreaterThan(0);
        refreshedLibrary.CountriesCount.ShouldBeGreaterThan(0);

        var actors = await dbContext.PlexTvShowActors.ToListAsync();
        actors.ShouldNotBeEmpty();

        var genres = await dbContext.PlexTvShowGenres.ToListAsync();
        genres.ShouldNotBeEmpty();

        var countries = await dbContext.PlexTvShowCountries.ToListAsync();
        countries.ShouldNotBeEmpty();

        // Verify the library was refreshed
        var movieActorCount = await dbContext.PlexTvShowActors.CountAsync();
        movieActorCount.ShouldBe(roleCount * tvShowCount);

        var genreTvShowCount = await dbContext.PlexTvShowGenres.CountAsync();
        genreTvShowCount.ShouldBe(genreCount * tvShowCount);

        var countryTvShowCount = await dbContext.PlexTvShowCountries.CountAsync();
        countryTvShowCount.ShouldBe(countryCount * tvShowCount);

        // Verify media data
        var episodes = await dbContext.PlexTvShowEpisodes.IncludeMediaData().ToListAsync();
        episodes.ShouldNotBeEmpty();
        episodes.Count.ShouldBe(totalEpisodeCount);

        var mediaList = episodes.SelectMany(x => x.MediaDataList).ToList();
        mediaList.Count.ShouldBeGreaterThanOrEqualTo(totalEpisodeCount);

        var parts = mediaList.SelectMany(x => x.Parts).ToList();
        parts.Count.ShouldBeGreaterThanOrEqualTo(totalEpisodeCount);
    }
}
