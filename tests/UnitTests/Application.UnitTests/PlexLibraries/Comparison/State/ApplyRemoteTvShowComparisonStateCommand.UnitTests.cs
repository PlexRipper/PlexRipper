namespace Reaparr.Application.UnitTests;

public class ApplyRemoteTvShowComparisonStateCommandUnitTests
    : BaseCommandUnitTest<ApplyRemoteTvShowComparisonStateCommand>
{
    [Test]
    public async Task ShouldMarkPartial_WhenRemoteTvShowHasTopLevelMatchAndMissingEpisodes()
    {
        // Arrange
        await SetupDatabase(61, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 1;
            config.TvShowSeasonCount = 1;
            config.TvShowEpisodeCount = 3;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 8, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var ownedTvShow = await GetLibraryTvShowAsync(ownedLibrary.Id);
        var remoteEpisodes = await GetLibraryEpisodesAsync(remoteLibrary.Id);
        var ownedEpisodes = await GetLibraryEpisodesAsync(ownedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexTvShowComparisons.Add(CreateTvShowComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteTvShow.Id,
            ownedTvShow.Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[0].Id,
            ownedEpisodes[0].Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[1].Id,
            ownedEpisodes[1].Id,
            PlexMediaComparisonHitState.Matched
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateTvShowItem(remoteTvShow) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyRemoteTvShowComparisonStateCommand(items, remoteLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Partial.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkPartialAndHigherQuality_WhenRemoteTvShowHasMissingEpisodesAndHigherQualityEpisode()
    {
        // Arrange
        await SetupDatabase(62, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 1;
            config.TvShowSeasonCount = 1;
            config.TvShowEpisodeCount = 3;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 8, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var ownedTvShow = await GetLibraryTvShowAsync(ownedLibrary.Id);
        var remoteEpisodes = await GetLibraryEpisodesAsync(remoteLibrary.Id);
        var ownedEpisodes = await GetLibraryEpisodesAsync(ownedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexTvShowComparisons.Add(CreateTvShowComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteTvShow.Id,
            ownedTvShow.Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[0].Id,
            ownedEpisodes[0].Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[1].Id,
            ownedEpisodes[1].Id,
            PlexMediaComparisonHitState.Matched
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateTvShowItem(remoteTvShow) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyRemoteTvShowComparisonStateCommand(items, remoteLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.PartialAndHigherQuality.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkMissing_WhenRemoteTvShowHasManyEpisodesAndNoTopLevelHit()
    {
        // Arrange
        await SetupDatabase(64, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 1;
            config.TvShowSeasonCount = 1;
            config.TvShowEpisodeCount = 1200;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 8, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);

        var items = new List<PlexMediaSlimDTO> { CreateTvShowItem(remoteTvShow) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyRemoteTvShowComparisonStateCommand(items, remoteLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Missing.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkHigherQuality_WhenShowHitIsHigherQualityAndAllEpisodesAreMatched()
    {
        // Arrange
        await SetupDatabase(65, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 1;
            config.TvShowSeasonCount = 1;
            config.TvShowEpisodeCount = 2;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);
        await SetLibraryUpdatedAtAsync(remoteLibrary.Id, new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc));
        await SetLibraryUpdatedAtAsync(ownedLibrary.Id, new DateTime(2026, 7, 22, 8, 5, 0, DateTimeKind.Utc));
        remoteLibrary = await GetLibraryAsync(remoteLibrary.Id);
        ownedLibrary = await GetLibraryAsync(ownedLibrary.Id);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        var ownedTvShow = await GetLibraryTvShowAsync(ownedLibrary.Id);
        var remoteEpisodes = await GetLibraryEpisodesAsync(remoteLibrary.Id);
        var ownedEpisodes = await GetLibraryEpisodesAsync(ownedLibrary.Id);
        await AddCurrentScopeAsync(remoteLibrary, ownedLibrary);
        dbContext.PlexTvShowComparisons.Add(CreateTvShowComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteTvShow.Id,
            ownedTvShow.Id,
            PlexMediaComparisonHitState.HigherQuality
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[0].Id,
            ownedEpisodes[0].Id,
            PlexMediaComparisonHitState.Matched
        ));
        dbContext.PlexEpisodeComparisons.Add(CreateEpisodeComparison(
            remoteLibrary.Id,
            ownedLibrary.Id,
            remoteEpisodes[1].Id,
            ownedEpisodes[1].Id,
            PlexMediaComparisonHitState.Matched
        ));
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateTvShowItem(remoteTvShow) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyRemoteTvShowComparisonStateCommand(items, remoteLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.HigherQuality.ToComparisonId());
    }

    [Test]
    public async Task ShouldMarkPending_WhenNoCurrentTvScopeAndComparisonIsQueued()
    {
        // Arrange
        await SetupDatabase(66, config =>
        {
            config.PlexServerCount = 2;
            config.PlexTvShowLibraryCount = 1;
            config.PlexAccountCount = 1;
            config.TvShowCount = 1;
        });

        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];
        await SetOwnedOverrideAsync(remoteLibrary.PlexServerId, false);
        await SetOwnedOverrideAsync(ownedLibrary.PlexServerId, true);

        var remoteTvShow = await GetLibraryTvShowAsync(remoteLibrary.Id);
        dbContext.TimeTickers.Add(new JobTimeTicker
        {
            Function = nameof(PlexLibraryComparisonJob),
            Request = [],
            JobKey = PlexLibraryComparisonJob.GetJobKey(ownedLibrary.Id, remoteLibrary.Id).Name,
            JobType = JobTypes.LibraryComparisonJob,
        });
        await dbContext.SaveChangesAsync(CancellationToken);

        var items = new List<PlexMediaSlimDTO> { CreateTvShowItem(remoteTvShow) };

        // Act
        var result = await TestHandlerExecuteAsync(new ApplyRemoteTvShowComparisonStateCommand(items, remoteLibrary.Id));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        items[0].ComparisonId.ShouldBe(PlexMediaComparisonState.Pending.ToComparisonId());
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

    private async Task<PlexTvShow> GetLibraryTvShowAsync(int plexLibraryId) =>
        await IDbContext.PlexTvShows
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .SingleAsync(CancellationToken);

    private async Task<List<PlexTvShowEpisode>> GetLibraryEpisodesAsync(int plexLibraryId) =>
        await IDbContext.PlexTvShowEpisodes
            .Where(x => x.PlexLibraryId == plexLibraryId)
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);

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
            MediaType = PlexMediaType.TvShow,
            CompletedAt = DateTime.UtcNow,
            RemoteLibraryUpdatedAt = remoteUpdatedAt,
            OwnedLibraryUpdatedAt = ownedUpdatedAt,
        });
        await dbContext.SaveChangesAsync(CancellationToken);
    }

    private static PlexTvShowComparison CreateTvShowComparison(
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

    private static PlexEpisodeComparison CreateEpisodeComparison(
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
            MatchType = PlexMediaComparisonMatchType.ParentAndChildNumbers,
            ComparedAt = DateTime.UtcNow,
        };

    private static PlexMediaSlimDTO CreateTvShowItem(PlexTvShow tvShow) =>
        new()
        {
            Id = tvShow.Id,
            PlexApiRatingKey = tvShow.PlexApiRatingKey,
            PlexApiMetaDataKey = tvShow.PlexApiMetaDataKey,
            Title = tvShow.Title,
            SearchTitle = tvShow.SearchTitle,
            SortIndex = tvShow.SortIndex,
            Year = tvShow.Year,
            Duration = tvShow.Duration,
            MediaSize = tvShow.MediaSize,
            ChildCount = tvShow.ChildCount,
            GrandChildCount = tvShow.GrandChildCount,
            AddedAt = tvShow.AddedAt,
            UpdatedAt = tvShow.UpdatedAt,
            PlexLibraryId = tvShow.PlexLibraryId,
            PlexServerId = tvShow.PlexServerId,
            Type = PlexMediaType.TvShow,
            HasThumb = tvShow.HasThumb,
            Qualities = [],
        };
}
