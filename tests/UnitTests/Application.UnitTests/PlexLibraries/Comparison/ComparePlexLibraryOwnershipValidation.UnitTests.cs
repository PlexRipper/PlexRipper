namespace Reaparr.Application.UnitTests;

public class CompareMoviePlexLibraryCommandOwnershipUnitTests : BaseCommandUnitTest<CompareMoviePlexLibraryCommand>
{
    [Test]
    public async Task ShouldTreatOwnedOverrideFalseAsRemote_WhenAccountLibraryStillOwned()
    {
        // Arrange
        await SetupDatabase(
            32,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        scope.RemotePlexLibraryId.ShouldBe(remoteLibrary.Id);
        scope.OwnedPlexLibraryId.ShouldBe(ownedLibrary.Id);
    }

    [Test]
    public async Task ShouldCreateHitRowsForEveryOwnedMovieWithSameTmdbGuid_WhenMultipleCandidatesExist()
    {
        // Arrange
        await SetupDatabase(
            36,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovies = await GetLibraryMoviesAsync(remoteLibrary.Id);
        var remoteMovie = remoteMovies[0];
        var ownedMovies = await GetLibraryMoviesAsync(ownedLibrary.Id);
        await UpdateMovieMatchFieldsAsync(
            remoteMovies[1].Id,
            "remote no match",
            1984,
            99,
            VideoQuality.FullHD,
            tmdbGuid: 9999
        );
        await UpdateMovieMatchFieldsAsync(
            remoteMovie.Id,
            "tmdb remote",
            2026,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 4242
        );
        await UpdateMovieMatchFieldsAsync(
            ownedMovies[0].Id,
            "tmdb owned one",
            2020,
            90,
            VideoQuality.HD,
            tmdbGuid: 4242
        );
        await UpdateMovieMatchFieldsAsync(
            ownedMovies[1].Id,
            "tmdb owned two",
            2021,
            100,
            VideoQuality.FullHD,
            tmdbGuid: 4242
        );

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hits = await IDbContext
            .PlexMovieComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .OrderBy(x => x.OwnedPlexMediaId)
            .ToListAsync(CancellationToken);
        hits.Count.ShouldBe(2);
        hits.Select(x => x.OwnedPlexMediaId).ToHashSet().SetEquals(ownedMovies.Select(x => x.Id)).ShouldBeTrue();
        hits.ShouldAllBe(x => x.RemotePlexMediaId == remoteMovie.Id);
        hits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.TmdbGuid);
        hits.ShouldAllBe(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);
    }

    [Test]
    public async Task ShouldPreferTmdbGuidOverImdbAndTitleFallback_WhenMultipleMatchLayersExist()
    {
        // Arrange
        await SetupDatabase(
            37,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovie = (await GetLibraryMoviesAsync(remoteLibrary.Id))[0];
        var ownedMovies = await GetLibraryMoviesAsync(ownedLibrary.Id);
        await UpdateMovieMatchFieldsAsync(
            remoteMovie.Id,
            "same title",
            2001,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 111,
            imdbGuid: "tt-priority"
        );
        await UpdateMovieMatchFieldsAsync(
            ownedMovies[0].Id,
            "different title",
            1999,
            80,
            VideoQuality.HD,
            tmdbGuid: 111
        );
        await UpdateMovieMatchFieldsAsync(
            ownedMovies[1].Id,
            "same title",
            2001,
            120,
            VideoQuality.HD,
            tmdbGuid: 222,
            imdbGuid: "tt-priority"
        );

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hit = await IDbContext.PlexMovieComparisons.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        hit.RemotePlexMediaId.ShouldBe(remoteMovie.Id);
        hit.OwnedPlexMediaId.ShouldBe(ownedMovies[0].Id);
        hit.MatchType.ShouldBe(PlexMediaComparisonMatchType.TmdbGuid);
    }

    [Test]
    public async Task ShouldPreferTitleYearDurationOverTitleYear_WhenDurationMatchExists()
    {
        // Arrange
        await SetupDatabase(
            38,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovie = (await GetLibraryMoviesAsync(remoteLibrary.Id))[0];
        var ownedMovies = await GetLibraryMoviesAsync(ownedLibrary.Id);
        await UpdateMovieMatchFieldsAsync(remoteMovie.Id, "duration title", 1995, 123, VideoQuality.FullHD);
        await UpdateMovieMatchFieldsAsync(ownedMovies[0].Id, "duration title", 1995, 123, VideoQuality.FullHD);
        await UpdateMovieMatchFieldsAsync(ownedMovies[1].Id, "duration title", 1995, 456, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hit = await IDbContext.PlexMovieComparisons.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        hit.OwnedPlexMediaId.ShouldBe(ownedMovies[0].Id);
        hit.MatchType.ShouldBe(PlexMediaComparisonMatchType.NormalizedTitleYearAndDuration);
        hit.HitState.ShouldBe(PlexMediaComparisonHitState.Matched);
    }

    [Test]
    public async Task ShouldFallbackToTitleYear_WhenRemoteDurationIsZero()
    {
        // Arrange
        await SetupDatabase(
            39,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovie = (await GetLibraryMoviesAsync(remoteLibrary.Id))[0];
        var ownedMovie = (await GetLibraryMoviesAsync(ownedLibrary.Id))[0];
        await UpdateMovieMatchFieldsAsync(remoteMovie.Id, "fallback title", 2003, 0, VideoQuality.FullHD);
        await UpdateMovieMatchFieldsAsync(ownedMovie.Id, "fallback title", 2003, 321, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hit = await IDbContext.PlexMovieComparisons.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        hit.RemotePlexMediaId.ShouldBe(remoteMovie.Id);
        hit.OwnedPlexMediaId.ShouldBe(ownedMovie.Id);
        hit.MatchType.ShouldBe(PlexMediaComparisonMatchType.NormalizedTitleAndYear);
    }

    [Test]
    public async Task ShouldCreateHitRowsForEveryOwnedMovieWithSameTitleYearDuration_WhenMultipleCandidatesExist()
    {
        // Arrange
        await SetupDatabase(
            88,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovies = await GetLibraryMoviesAsync(remoteLibrary.Id);
        var remoteMovie = remoteMovies[0];
        var ownedMovies = await GetLibraryMoviesAsync(ownedLibrary.Id);
        await UpdateMovieMatchFieldsAsync(
            remoteMovies[1].Id,
            "remote no match",
            1984,
            99,
            VideoQuality.FullHD,
            tmdbGuid: 9999
        );
        await UpdateMovieMatchFieldsAsync(remoteMovie.Id, "same movie duration", 2026, 120, VideoQuality.UHD_4K);
        await UpdateMovieMatchFieldsAsync(ownedMovies[0].Id, "same movie duration", 2026, 120, VideoQuality.HD);
        await UpdateMovieMatchFieldsAsync(ownedMovies[1].Id, "same movie duration", 2026, 120, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hits = await IDbContext
            .PlexMovieComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .OrderBy(x => x.OwnedPlexMediaId)
            .ToListAsync(CancellationToken);
        hits.Count.ShouldBe(2);
        hits.Select(x => x.OwnedPlexMediaId).ToHashSet().SetEquals(ownedMovies.Select(x => x.Id)).ShouldBeTrue();
        hits.ShouldAllBe(x => x.RemotePlexMediaId == remoteMovie.Id);
        hits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.NormalizedTitleYearAndDuration);
        hits.ShouldAllBe(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);
    }

    [Test]
    public async Task ShouldDeleteOldMovieHits_WhenRerunFindsNoMatches()
    {
        // Arrange
        await SetupDatabase(
            40,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.MovieCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteMovie = (await GetLibraryMoviesAsync(remoteLibrary.Id))[0];
        var ownedMovie = (await GetLibraryMoviesAsync(ownedLibrary.Id))[0];
        dbContext.PlexMovieComparisons.Add(
            CreateMovieComparison(
                remoteLibrary.Id,
                ownedLibrary.Id,
                remoteMovie.Id,
                ownedMovie.Id,
                PlexMediaComparisonHitState.Matched
            )
        );
        await dbContext.SaveChangesAsync(CancellationToken);
        await UpdateMovieMatchFieldsAsync(remoteMovie.Id, "remote only", 2001, 120, VideoQuality.FullHD);
        await UpdateMovieMatchFieldsAsync(ownedMovie.Id, "owned only", 2002, 121, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hitCount = await IDbContext.PlexMovieComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        hitCount.ShouldBe(0);
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
        scope.CompletedAt.ShouldBeGreaterThan(default(DateTime));
    }

    [Test]
    public async Task ShouldUpdateExistingComparisonScopeSnapshots_WhenScopeAlreadyExists()
    {
        // Arrange
        await SetupDatabase(
            33,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        const long remoteContentChangedAt = 1721582655L;
        const long ownedContentChangedAt = 1721570913L;
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryContentChangedAtAsync(remoteLibrary.Id, remoteContentChangedAt);
        await SetLibraryContentChangedAtAsync(ownedLibrary.Id, ownedContentChangedAt);
        dbContext.PlexComparisonScopes.Add(
            new PlexComparisonState
            {
                Id = 0,
                RemotePlexLibraryId = remoteLibrary.Id,
                OwnedPlexLibraryId = ownedLibrary.Id,
                MediaType = PlexMediaType.Movie,
                CompletedAt = new DateTime(2026, 7, 20, 22, 19, 16, DateTimeKind.Utc),
            }
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.Movie,
            CancellationToken
        );
    }

    private async Task<List<PlexMovie>> GetLibraryMoviesAsync(int plexLibraryId) =>
        await IDbContext
            .PlexMovies.Where(x => x.PlexLibraryId == plexLibraryId)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);

    private async Task UpdateMovieMatchFieldsAsync(
        int plexMovieId,
        string searchTitle,
        int year,
        int duration,
        VideoQuality quality,
        int? tmdbGuid = null,
        string? imdbGuid = null,
        int? tvdbGuid = null
    )
    {
        await IDbContext
            .PlexMovies.Where(x => x.Id == plexMovieId)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(y => y.SearchTitle, searchTitle)
                        .SetProperty(y => y.Year, year)
                        .SetProperty(y => y.Duration, duration)
                        .SetProperty(y => y.Quality, quality)
                        .SetProperty(y => y.Guid_TMDB, tmdbGuid)
                        .SetProperty(y => y.Guid_IMDB, imdbGuid)
                        .SetProperty(y => y.Guid_TVDB, tvdbGuid),
                CancellationToken
            );
    }
}

public class CompareTvShowPlexLibraryCommandOwnershipUnitTests : BaseCommandUnitTest<CompareTvShowPlexLibraryCommand>
{
    [Test]
    public async Task ShouldTreatOwnedOverrideFalseAsRemote_WhenAccountLibraryStillOwned()
    {
        // Arrange
        await SetupDatabase(
            35,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
        scope.RemotePlexLibraryId.ShouldBe(remoteLibrary.Id);
        scope.OwnedPlexLibraryId.ShouldBe(ownedLibrary.Id);
    }

    [Test]
    public async Task ShouldCreateHitRowsForEveryOwnedTvShowWithSameTmdbGuid_WhenMultipleCandidatesExist()
    {
        // Arrange
        await SetupDatabase(
            86,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShows = await GetLibraryTvShowsAsync(remoteLibrary.Id);
        var remoteShow = remoteShows[0];
        var ownedShows = await GetLibraryTvShowsAsync(ownedLibrary.Id);
        var remoteSeason = (await GetLibrarySeasonsAsync(remoteLibrary.Id)).Single(x => x.TvShowId == remoteShow.Id);
        var ownedSeasons = await GetLibrarySeasonsAsync(ownedLibrary.Id);
        var remoteEpisode = (await GetLibraryEpisodesAsync(remoteLibrary.Id)).Single(x => x.TvShowId == remoteShow.Id);
        var ownedEpisodes = await GetLibraryEpisodesAsync(ownedLibrary.Id);
        await UpdateTvShowMatchFieldsAsync(
            remoteShows[1].Id,
            "remote no match",
            1984,
            99,
            VideoQuality.FullHD,
            tmdbGuid: 9999
        );
        await UpdateTvShowMatchFieldsAsync(
            remoteShow.Id,
            "tmdb remote show",
            2026,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 5151
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShows[0].Id,
            "tmdb owned show one",
            2020,
            90,
            VideoQuality.HD,
            tmdbGuid: 5151
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShows[1].Id,
            "tmdb owned show two",
            2021,
            100,
            VideoQuality.FullHD,
            tmdbGuid: 5151
        );

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hits = await IDbContext
            .PlexTvShowComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .OrderBy(x => x.OwnedPlexMediaId)
            .ToListAsync(CancellationToken);
        hits.Count.ShouldBe(2);
        hits.Select(x => x.OwnedPlexMediaId).ToHashSet().SetEquals(ownedShows.Select(x => x.Id)).ShouldBeTrue();
        hits.ShouldAllBe(x => x.RemotePlexMediaId == remoteShow.Id);
        hits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.TmdbGuid);
        hits.ShouldAllBe(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);
        var seasonHits = await IDbContext
            .PlexSeasonComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .ToListAsync(CancellationToken);
        var episodeHits = await IDbContext
            .PlexEpisodeComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .ToListAsync(CancellationToken);
        seasonHits.Count.ShouldBe(ownedSeasons.Count);
        seasonHits.ShouldAllBe(x =>
            x.RemotePlexMediaId == remoteSeason.Id
            && x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers
            && x.HitState == PlexMediaComparisonHitState.Matched
        );
        seasonHits.Select(x => x.OwnedPlexMediaId).ToHashSet().SetEquals(ownedSeasons.Select(x => x.Id)).ShouldBeTrue();
        episodeHits.Count.ShouldBe(ownedEpisodes.Count);
        episodeHits.ShouldAllBe(x =>
            x.RemotePlexMediaId == remoteEpisode.Id
            && x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers
            && x.HitState == PlexMediaComparisonHitState.Matched
        );
        episodeHits
            .Select(x => x.OwnedPlexMediaId)
            .ToHashSet()
            .SetEquals(ownedEpisodes.Select(x => x.Id))
            .ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPreferTmdbGuidOverTitleFallback_WhenMultipleTvShowMatchLayersExist()
    {
        // Arrange
        await SetupDatabase(
            41,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShows = await GetLibraryTvShowsAsync(remoteLibrary.Id);
        var ownedShows = await GetLibraryTvShowsAsync(ownedLibrary.Id);
        await UpdateTvShowMatchFieldsAsync(
            remoteShows[0].Id,
            "same show",
            2001,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 111,
            imdbGuid: "tt-tv-priority"
        );
        await UpdateTvShowMatchFieldsAsync(
            remoteShows[1].Id,
            "remote no match",
            1984,
            99,
            VideoQuality.FullHD,
            tmdbGuid: 9999
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShows[0].Id,
            "different show",
            1999,
            80,
            VideoQuality.HD,
            tmdbGuid: 111
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShows[1].Id,
            "same show",
            2001,
            120,
            VideoQuality.HD,
            tmdbGuid: 222,
            imdbGuid: "tt-tv-priority"
        );

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hit = await IDbContext.PlexTvShowComparisons.SingleAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        hit.RemotePlexMediaId.ShouldBe(remoteShows[0].Id);
        hit.OwnedPlexMediaId.ShouldBe(ownedShows[0].Id);
        hit.MatchType.ShouldBe(PlexMediaComparisonMatchType.TmdbGuid);
    }

    [Test]
    public async Task ShouldCreateSeasonAndEpisodeRows_WhenShowSeasonAndEpisodeNumbersMatch()
    {
        // Arrange
        await SetupDatabase(
            42,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 2;
                config.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShow = (await GetLibraryTvShowsAsync(remoteLibrary.Id))[0];
        var ownedShow = (await GetLibraryTvShowsAsync(ownedLibrary.Id))[0];
        await UpdateTvShowMatchFieldsAsync(
            remoteShow.Id,
            "numbered show",
            2011,
            500,
            VideoQuality.UHD_4K,
            tmdbGuid: 2222
        );
        await UpdateTvShowMatchFieldsAsync(ownedShow.Id, "numbered show", 2011, 500, VideoQuality.HD, tmdbGuid: 2222);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var showHits = await IDbContext
            .PlexTvShowComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .ToListAsync(CancellationToken);
        var seasonHits = await IDbContext
            .PlexSeasonComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .ToListAsync(CancellationToken);
        var episodeHits = await IDbContext
            .PlexEpisodeComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .ToListAsync(CancellationToken);
        showHits.Count.ShouldBe(1);
        seasonHits.Count.ShouldBe(2);
        episodeHits.Count.ShouldBe(4);
        showHits[0].HitState.ShouldBe(PlexMediaComparisonHitState.HigherQuality);
        seasonHits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers);
        episodeHits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers);
    }

    [Test]
    public async Task ShouldCreateHitRowsForEveryOwnedTvShowWithSameTitleYearDuration_WhenMultipleCandidatesExist()
    {
        // Arrange
        await SetupDatabase(
            87,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 2;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShows = await GetLibraryTvShowsAsync(remoteLibrary.Id);
        var remoteShow = remoteShows[0];
        var ownedShows = await GetLibraryTvShowsAsync(ownedLibrary.Id);
        await UpdateTvShowMatchFieldsAsync(
            remoteShows[1].Id,
            "remote no match",
            1984,
            99,
            VideoQuality.FullHD,
            tmdbGuid: 9999
        );
        await UpdateTvShowMatchFieldsAsync(remoteShow.Id, "same title duration", 2026, 120, VideoQuality.UHD_4K);
        await UpdateTvShowMatchFieldsAsync(ownedShows[0].Id, "same title duration", 2026, 120, VideoQuality.HD);
        await UpdateTvShowMatchFieldsAsync(ownedShows[1].Id, "same title duration", 2026, 120, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var hits = await IDbContext
            .PlexTvShowComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id
            )
            .OrderBy(x => x.OwnedPlexMediaId)
            .ToListAsync(CancellationToken);
        hits.Count.ShouldBe(2);
        hits.Select(x => x.OwnedPlexMediaId).ToHashSet().SetEquals(ownedShows.Select(x => x.Id)).ShouldBeTrue();
        hits.ShouldAllBe(x => x.RemotePlexMediaId == remoteShow.Id);
        hits.ShouldAllBe(x => x.MatchType == PlexMediaComparisonMatchType.NormalizedTitleYearAndDuration);
        hits.ShouldAllBe(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);
    }

    [Test]
    public async Task ShouldNotCreateSeasonOrEpisodeRows_WhenShowMatchesButSeasonNumbersDoNot()
    {
        // Arrange
        await SetupDatabase(
            43,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShow = (await GetLibraryTvShowsAsync(remoteLibrary.Id))[0];
        var ownedShow = (await GetLibraryTvShowsAsync(ownedLibrary.Id))[0];
        var ownedSeason = (await GetLibrarySeasonsAsync(ownedLibrary.Id))[0];
        await UpdateTvShowMatchFieldsAsync(
            remoteShow.Id,
            "season mismatch",
            2012,
            500,
            VideoQuality.FullHD,
            tmdbGuid: 3333
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShow.Id,
            "season mismatch",
            2012,
            500,
            VideoQuality.FullHD,
            tmdbGuid: 3333
        );
        await UpdateSeasonNumberAsync(ownedSeason.Id, 99);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var showHitCount = await IDbContext.PlexTvShowComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        var seasonHitCount = await IDbContext.PlexSeasonComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        var episodeHitCount = await IDbContext.PlexEpisodeComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        showHitCount.ShouldBe(1);
        seasonHitCount.ShouldBe(0);
        episodeHitCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldDeleteOldTvHits_WhenRerunFindsNoShowMatches()
    {
        // Arrange
        await SetupDatabase(
            44,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 1;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShow = (await GetLibraryTvShowsAsync(remoteLibrary.Id))[0];
        var ownedShow = (await GetLibraryTvShowsAsync(ownedLibrary.Id))[0];
        var remoteSeason = (await GetLibrarySeasonsAsync(remoteLibrary.Id))[0];
        var ownedSeason = (await GetLibrarySeasonsAsync(ownedLibrary.Id))[0];
        var remoteEpisode = (await GetLibraryEpisodesAsync(remoteLibrary.Id))[0];
        var ownedEpisode = (await GetLibraryEpisodesAsync(ownedLibrary.Id))[0];
        dbContext.PlexTvShowComparisons.Add(
            CreateTvShowComparison(remoteLibrary.Id, ownedLibrary.Id, remoteShow.Id, ownedShow.Id)
        );
        dbContext.PlexSeasonComparisons.Add(
            CreateSeasonComparison(remoteLibrary.Id, ownedLibrary.Id, remoteSeason.Id, ownedSeason.Id)
        );
        dbContext.PlexEpisodeComparisons.Add(
            CreateEpisodeComparison(remoteLibrary.Id, ownedLibrary.Id, remoteEpisode.Id, ownedEpisode.Id)
        );
        await dbContext.SaveChangesAsync(CancellationToken);
        await UpdateTvShowMatchFieldsAsync(remoteShow.Id, "remote only", 2001, 120, VideoQuality.FullHD);
        await UpdateTvShowMatchFieldsAsync(ownedShow.Id, "owned only", 2002, 121, VideoQuality.FullHD);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        var showHitCount = await IDbContext.PlexTvShowComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        var seasonHitCount = await IDbContext.PlexSeasonComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        var episodeHitCount = await IDbContext.PlexEpisodeComparisons.CountAsync(
            x => x.RemotePlexLibraryId == remoteLibrary.Id && x.OwnedPlexLibraryId == ownedLibrary.Id,
            CancellationToken
        );
        showHitCount.ShouldBe(0);
        seasonHitCount.ShouldBe(0);
        episodeHitCount.ShouldBe(0);
        var scope = await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
        scope.CompletedAt.ShouldBeGreaterThan(default(DateTime));
    }

    [Test]
    public async Task ShouldUpdateExistingComparisonScopeSnapshots_WhenScopeAlreadyExists()
    {
        // Arrange
        await SetupDatabase(
            34,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        const long remoteContentChangedAt = 1721582655L;
        const long ownedContentChangedAt = 1721570913L;
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryContentChangedAtAsync(remoteLibrary.Id, remoteContentChangedAt);
        await SetLibraryContentChangedAtAsync(ownedLibrary.Id, ownedContentChangedAt);
        dbContext.PlexComparisonScopes.Add(
            new PlexComparisonState
            {
                Id = 0,
                RemotePlexLibraryId = remoteLibrary.Id,
                OwnedPlexLibraryId = ownedLibrary.Id,
                MediaType = PlexMediaType.TvShow,
                CompletedAt = new DateTime(2026, 7, 20, 22, 19, 16, DateTimeKind.Utc),
            }
        );
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        await IDbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
    }

    [Test]
    public async Task ShouldPersistComparisonRowsAcrossBatchBoundary()
    {
        // Arrange
        await SetupDatabase(
            62701,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexTvShowLibraryCount = 1;
                config.PlexAccountCount = 1;
                config.TvShowCount = 101;
                config.TvShowSeasonCount = 1;
                config.TvShowEpisodeCount = 1;
            }
        );

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteShows = await GetLibraryTvShowsAsync(remoteLibrary.Id);
        var firstRemoteShow = remoteShows[0];
        var remoteShow = remoteShows[^1];
        var ownedShow = (await GetLibraryTvShowsAsync(ownedLibrary.Id))[^1];
        await UpdateTvShowMatchFieldsAsync(
            firstRemoteShow.Id,
            "batch boundary show",
            2026,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 62701
        );
        await UpdateTvShowMatchFieldsAsync(
            remoteShow.Id,
            "batch boundary show",
            2026,
            120,
            VideoQuality.UHD_4K,
            tmdbGuid: 62701
        );
        await UpdateTvShowMatchFieldsAsync(
            ownedShow.Id,
            "batch boundary show",
            2026,
            120,
            VideoQuality.HD,
            tmdbGuid: 62701
        );

        var remoteSeasons = await GetLibrarySeasonsAsync(remoteLibrary.Id);
        var firstRemoteSeason = remoteSeasons.Single(x => x.TvShowId == firstRemoteShow.Id);
        var remoteSeason = remoteSeasons.Single(x => x.TvShowId == remoteShow.Id);
        var ownedSeason = (await GetLibrarySeasonsAsync(ownedLibrary.Id)).Single(x => x.TvShowId == ownedShow.Id);
        var remoteEpisodes = await GetLibraryEpisodesAsync(remoteLibrary.Id);
        var firstRemoteEpisode = remoteEpisodes.Single(x => x.TvShowId == firstRemoteShow.Id);
        var remoteEpisode = remoteEpisodes.Single(x => x.TvShowId == remoteShow.Id);
        var ownedEpisode = (await GetLibraryEpisodesAsync(ownedLibrary.Id)).Single(x => x.TvShowId == ownedShow.Id);

        // Act
        var result = await TestHandlerExecuteAsync(
            new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        remoteShows.Count.ShouldBe(101);
        remoteShows[99].Id.ShouldBeLessThan(remoteShow.Id);
        var showHits = await dbContext
            .PlexTvShowComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && (x.RemotePlexMediaId == firstRemoteShow.Id || x.RemotePlexMediaId == remoteShow.Id)
                && x.OwnedPlexMediaId == ownedShow.Id
            )
            .ToListAsync(CancellationToken);
        showHits.Count.ShouldBe(2);
        showHits.ShouldAllBe(x =>
            x.OwnedPlexMediaId == ownedShow.Id
            && x.MatchType == PlexMediaComparisonMatchType.TmdbGuid
            && x.HitState == PlexMediaComparisonHitState.HigherQuality
            && x.RemoteQuality == VideoQuality.UHD_4K
            && x.OwnedQuality == VideoQuality.HD
        );
        showHits
            .Select(x => x.RemotePlexMediaId)
            .ToHashSet()
            .SetEquals([firstRemoteShow.Id, remoteShow.Id])
            .ShouldBeTrue();

        var seasonHits = await dbContext
            .PlexSeasonComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && (x.RemotePlexMediaId == firstRemoteSeason.Id || x.RemotePlexMediaId == remoteSeason.Id)
                && x.OwnedPlexMediaId == ownedSeason.Id
            )
            .ToListAsync(CancellationToken);
        seasonHits.Count.ShouldBe(2);
        seasonHits.ShouldAllBe(x =>
            x.OwnedPlexMediaId == ownedSeason.Id
            && x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers
            && x.HitState == PlexMediaComparisonHitState.Matched
        );
        seasonHits
            .Select(x => x.RemotePlexMediaId)
            .ToHashSet()
            .SetEquals([firstRemoteSeason.Id, remoteSeason.Id])
            .ShouldBeTrue();

        var episodeHits = await dbContext
            .PlexEpisodeComparisons.Where(x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && (x.RemotePlexMediaId == firstRemoteEpisode.Id || x.RemotePlexMediaId == remoteEpisode.Id)
                && x.OwnedPlexMediaId == ownedEpisode.Id
            )
            .ToListAsync(CancellationToken);
        episodeHits.Count.ShouldBe(2);
        episodeHits.ShouldAllBe(x =>
            x.OwnedPlexMediaId == ownedEpisode.Id
            && x.MatchType == PlexMediaComparisonMatchType.ParentAndChildNumbers
            && x.HitState == PlexMediaComparisonHitState.Matched
        );
        episodeHits
            .Select(x => x.RemotePlexMediaId)
            .ToHashSet()
            .SetEquals([firstRemoteEpisode.Id, remoteEpisode.Id])
            .ShouldBeTrue();

        var scope = await dbContext.PlexComparisonScopes.SingleAsync(
            x =>
                x.RemotePlexLibraryId == remoteLibrary.Id
                && x.OwnedPlexLibraryId == ownedLibrary.Id
                && x.MediaType == PlexMediaType.TvShow,
            CancellationToken
        );
        scope.CompletedAt.ShouldBeGreaterThan(default);
    }

    private async Task<List<PlexTvShow>> GetLibraryTvShowsAsync(int plexLibraryId) =>
        await IDbContext
            .PlexTvShows.Where(x => x.PlexLibraryId == plexLibraryId)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);

    private async Task<List<PlexTvShowSeason>> GetLibrarySeasonsAsync(int plexLibraryId) =>
        await IDbContext
            .PlexTvShowSeason.Where(x => x.PlexLibraryId == plexLibraryId)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);

    private async Task UpdateTvShowMatchFieldsAsync(
        int plexTvShowId,
        string searchTitle,
        int year,
        int duration,
        VideoQuality quality,
        int? tmdbGuid = null,
        string? imdbGuid = null,
        int? tvdbGuid = null
    )
    {
        await IDbContext
            .PlexTvShows.Where(x => x.Id == plexTvShowId)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(y => y.SearchTitle, searchTitle)
                        .SetProperty(y => y.Year, year)
                        .SetProperty(y => y.Duration, duration)
                        .SetProperty(y => y.Quality, quality)
                        .SetProperty(y => y.Guid_TMDB, tmdbGuid)
                        .SetProperty(y => y.Guid_IMDB, imdbGuid)
                        .SetProperty(y => y.Guid_TVDB, tvdbGuid),
                CancellationToken
            );
    }

    private async Task UpdateSeasonNumberAsync(int plexTvShowSeasonId, int seasonNumber)
    {
        await IDbContext
            .PlexTvShowSeason.Where(x => x.Id == plexTvShowSeasonId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.SeasonNumber, seasonNumber), CancellationToken);
    }
}
