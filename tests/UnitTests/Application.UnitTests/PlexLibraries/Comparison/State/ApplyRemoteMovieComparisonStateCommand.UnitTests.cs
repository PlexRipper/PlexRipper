namespace Reaparr.Application.UnitTests;

public class ApplyRemoteMovieComparisonStateCommandUnitTests
    : BaseUnitTest<ApplyRemoteMovieComparisonStateCommandHandler>
{
    [Test]
    public async Task ShouldMarkOwned_WhenItemMatchesAnyCurrentOwnedLibrary()
    {
        // Arrange
        await SetupDatabase(41, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var matchedOwnedLibrary = libraries[1];
        var missingOwnedLibrary = libraries[2];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(matchedOwnedLibrary.PlexServerId, true);
        await SetOwnedOverrideAsync(missingOwnedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(matchedOwnedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(missingOwnedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        matchedOwnedLibrary = await GetLibraryAsync(matchedOwnedLibrary.Id);
        missingOwnedLibrary = await GetLibraryAsync(missingOwnedLibrary.Id);

        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);
        var ownedMovie = await GetLibraryMovieAsync(matchedOwnedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, matchedOwnedLibrary);
        await AddCurrentScopeAsync(remoteLibrary, missingOwnedLibrary);
        dbContext.PlexMovieComparisons.Add(CreateMovieComparison(
            remoteLibrary.Id,
            matchedOwnedLibrary.Id,
            remoteMovie.Id,
            ownedMovie.Id,
            PlexMediaComparisonHitState.Matched
        ));
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(remoteMovie) };

        // Act
        var result = await Sut.ExecuteAsync(new ApplyRemoteMovieComparisonStateCommand(items, remoteLibrary.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonState.ShouldBe(PlexMediaComparisonState.Owned);
    }

    [Test]
    public async Task ShouldMarkHigherQuality_WhenAnyCurrentOwnedLibraryHitIsHigherQuality()
    {
        // Arrange
        await SetupDatabase(42, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var matchedOwnedLibrary = libraries[1];
        var higherQualityOwnedLibrary = libraries[2];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(matchedOwnedLibrary.PlexServerId, true);
        await SetOwnedOverrideAsync(higherQualityOwnedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(matchedOwnedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(higherQualityOwnedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        matchedOwnedLibrary = await GetLibraryAsync(matchedOwnedLibrary.Id);
        higherQualityOwnedLibrary = await GetLibraryAsync(higherQualityOwnedLibrary.Id);

        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);
        var matchedOwnedMovie = await GetLibraryMovieAsync(matchedOwnedLibrary.Id);
        var higherQualityOwnedMovie = await GetLibraryMovieAsync(higherQualityOwnedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, matchedOwnedLibrary);
        await AddCurrentScopeAsync(remoteLibrary, higherQualityOwnedLibrary);
        dbContext.PlexMovieComparisons.Add(CreateMovieComparison(
            remoteLibrary.Id,
            matchedOwnedLibrary.Id,
            remoteMovie.Id,
            matchedOwnedMovie.Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexMovieComparisons.Add(CreateMovieComparison(
            remoteLibrary.Id,
            higherQualityOwnedLibrary.Id,
            remoteMovie.Id,
            higherQualityOwnedMovie.Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(remoteMovie) };

        // Act
        var result = await Sut.ExecuteAsync(new ApplyRemoteMovieComparisonStateCommand(items, remoteLibrary.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonState.ShouldBe(PlexMediaComparisonState.HigherQuality);
    }

    [Test]
    public async Task ShouldMarkMissing_WhenItemHasNoHitsInAnyCurrentOwnedLibrary()
    {
        // Arrange
        await SetupDatabase(43, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.MovieCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 21, 17, 24, 15, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 21, 14, 8, 33, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(remoteMovie) };

        // Act
        var result = await Sut.ExecuteAsync(new ApplyRemoteMovieComparisonStateCommand(items, remoteLibrary.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonState.ShouldBe(PlexMediaComparisonState.Missing);
    }

    private async Task SetOwnedOverrideAsync(int plexServerId, bool ownedOverride)
    {
        await IDbContext.PlexServers
            .Where(x => x.Id == plexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, ownedOverride), CancellationToken);
    }

    private async Task SetLibraryUpdatedAtAsync(int plexLibraryId, DateTime updatedAt)
    {
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.UpdatedAt, updatedAt), CancellationToken);
    }

    private async Task<PlexLibrary> GetLibraryAsync(int plexLibraryId) =>
        await IDbContext.PlexLibraries
            .Where(x => x.Id == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task<PlexMovie> GetLibraryMovieAsync(int plexLibraryId) =>
        await IDbContext.PlexMovies
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task AddCurrentScopeAsync(PlexLibrary remoteLibrary, PlexLibrary ownedLibrary)
    {
        var remoteUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == remoteLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);
        var ownedUpdatedAt = await IDbContext.PlexLibraries
            .Where(x => x.Id == ownedLibrary.Id)
            .Select(x => x.UpdatedAt)
            .SingleAsync(CancellationToken);

        var dbContext = IDbContext;
        dbContext.PlexComparisonScopes.Add(new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibrary.Id,
            OwnedPlexLibraryId = ownedLibrary.Id,
            MediaType = PlexMediaType.Movie,
            CompletedAt = DateTime.UtcNow,
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesNewAsync(CancellationToken);
    }

    private static PlexMovieComparison CreateMovieComparison(
        int remotePlexLibraryId,
        int ownedPlexLibraryId,
        int remotePlexMediaId,
        int ownedPlexMediaId,
        PlexMediaComparisonHitState hitState) =>
        new()
        {
            Id = 0,
            RemotePlexLibraryId = remotePlexLibraryId,
            OwnedPlexLibraryId = ownedPlexLibraryId,
            RemotePlexMediaId = remotePlexMediaId,
            OwnedPlexMediaId = ownedPlexMediaId,
            HitState = hitState,
            RemoteQuality = VideoQuality.FullHD,
            OwnedQuality = hitState == PlexMediaComparisonHitState.HigherQuality ? VideoQuality.HD : VideoQuality.FullHD,
            MatchType = PlexMediaComparisonMatchType.TmdbGuid,
            ComparedAt = DateTime.UtcNow,
        };

    private static PlexMediaSlimDTO CreateMovieItem(PlexMovie movie) =>
        new()
        {
            Id = movie.Id,
            PlexApiRatingKey = movie.PlexApiRatingKey,
            PlexApiMetaDataKey = movie.PlexApiMetaDataKey,
            Title = movie.Title,
            SearchTitle = movie.SearchTitle,
            SortIndex = movie.SortIndex,
            Year = movie.Year,
            Duration = movie.Duration,
            MediaSize = movie.MediaSize,
            ChildCount = movie.ChildCount,
            GrandChildCount = 0,
            AddedAt = movie.AddedAt,
            UpdatedAt = movie.UpdatedAt,
            PlexLibraryId = movie.PlexLibraryId,
            PlexServerId = movie.PlexServerId,
            Type = PlexMediaType.Movie,
            HasThumb = movie.HasThumb,
            Qualities = [],
        };
}
