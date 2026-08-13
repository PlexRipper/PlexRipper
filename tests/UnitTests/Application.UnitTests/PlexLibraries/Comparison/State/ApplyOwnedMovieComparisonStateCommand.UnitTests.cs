namespace Reaparr.Application.UnitTests;

public class ApplyOwnedMovieComparisonStateCommandUnitTests
    : BaseCommandUnitTest<ApplyOwnedMovieComparisonStateCommand>
{
    [Test]
    public async Task ShouldMarkOwned_WhenCurrentRemoteScopeHasNoUpgradeHit()
    {
        // Arrange
        await SetupDatabase(64, config =>
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
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 10, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);
        var ownedMovie = await GetLibraryMovieAsync(ownedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexMovieComparisons.Add(CreateMovieComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteMovie.Id,
            ownedMovie.Id,
            PlexMediaComparisonHitState.Matched
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(ownedMovie) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyOwnedMovieComparisonStateCommand(items, ownedLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Owned.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkPending_WhenNoCurrentRemoteScopeAndComparisonIsQueued()
    {
        // Arrange
        await SetupDatabase(65, config =>
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
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 10, 5, 0, DateTimeKind.Utc));

        var ownedMovie = await GetLibraryMovieAsync(ownedLibrary.Id);
        dbContext.TimeTickers.Add(new JobTimeTicker
        {
            Function = nameof(PlexLibraryComparisonJob),
            Request = [],
            JobKey = PlexLibraryComparisonJob.GetJobKey(ownedLibrary.Id, remoteLibrary.Id).Name,
            JobType = JobTypes.LibraryComparisonJob,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(ownedMovie) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyOwnedMovieComparisonStateCommand(items, ownedLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Pending.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkHigherQuality_WhenCurrentRemoteScopeHasUpgradeHit()
    {
        // Arrange
        await SetupDatabase(66, config =>
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
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 10, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteMovie = await GetLibraryMovieAsync(remoteLibrary.Id);
        var ownedMovie = await GetLibraryMovieAsync(ownedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexMovieComparisons.Add(CreateMovieComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteMovie.Id,
            ownedMovie.Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(ownedMovie) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyOwnedMovieComparisonStateCommand(items, ownedLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.HigherQuality.ToComparisonId());
    }

    [Test]
    public async Task ShouldLeaveNotCompared_WhenOnlyCompletedQueueExistsWithoutCurrentScope()
    {
        // Arrange
        await SetupDatabase(67, config =>
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

        var ownedMovie = await GetLibraryMovieAsync(ownedLibrary.Id);
        var items = new List<PlexMediaSlimDTO> { CreateMovieItem(ownedMovie) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyOwnedMovieComparisonStateCommand(items, ownedLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.NotCompared.ToComparisonId());
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
        await dbContext.SaveChangesAsync(CancellationToken);
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
