namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and TV show hit rows onto owned-library <see cref="PlexMediaSlimDTO"/> items,
/// marking them <see cref="PlexMediaComparisonState.HigherQuality"/> when any current remote scope has an upgrade hit.
/// </summary>
/// <param name="Items">The overview page items from an owned library. Modified in-place.</param>
/// <param name="OwnedLibraryId">The owned TV show Plex library being browsed.</param>
public record ApplyOwnedTvShowComparisonStateCommand(
    List<PlexMediaSlimDTO> Items,
    int OwnedLibraryId
) : ICommand<Result>;

public class ApplyOwnedTvShowComparisonStateCommandValidator
    : AbstractValidator<ApplyOwnedTvShowComparisonStateCommand>
{
    public ApplyOwnedTvShowComparisonStateCommandValidator()
    {
        RuleFor(x => x.Items).NotNull().WithMessage("Items must not be null.");
        RuleFor(x => x.OwnedLibraryId).GreaterThan(0).WithMessage("OwnedLibraryId must be greater than 0.");
    }
}

public class ApplyOwnedTvShowComparisonStateCommandHandler
    : ICommandHandler<ApplyOwnedTvShowComparisonStateCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public ApplyOwnedTvShowComparisonStateCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<ApplyOwnedTvShowComparisonStateCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(
        ApplyOwnedTvShowComparisonStateCommand command,
        CancellationToken ct)
    {
        var items = command.Items;
        if (items.Count == 0)
            return Result.Ok();

        var ownedUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == command.OwnedLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);

        if (ownedUpdatedAt is null)
        {
            _log.Here().Warning("Owned TV library {LibraryId} not found for comparison projection", command.OwnedLibraryId);
            return Result.Ok();
        }

        var remoteLibraries = await _dbContext.PlexLibraries
            .WhereIsNotOwned()
            .Where(x => x.Type == PlexMediaType.TvShow)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (remoteLibraries.Count == 0)
            return Result.Ok();

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.OwnedPlexLibraryId == command.OwnedLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && remoteLibraries.Keys.Contains(x.RemotePlexLibraryId))
            .ToListAsync(ct);

        var currentRemoteLibraryIds = scopeRows
            .Where(x =>
                x.OwnedLibraryUpdatedAt == ownedUpdatedAt
                && remoteLibraries.TryGetValue(x.RemotePlexLibraryId, out var remoteUpdatedAt)
                && x.RemoteLibraryUpdatedAt == remoteUpdatedAt)
            .Select(x => x.RemotePlexLibraryId)
            .ToHashSet();

        if (currentRemoteLibraryIds.Count == 0)
        {
            if (await HasPendingComparisonAsync(command.OwnedLibraryId, remoteLibraries.Keys.ToHashSet(), ct))
            {
                foreach (var t in items)
                    t.SetComparisonState(PlexMediaComparisonState.Pending);
            }

            return Result.Ok();
        }

        var itemIds = items.Select(x => x.Id).ToHashSet();

        var showUpgradeIds = await _dbContext.PlexTvShowComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == command.OwnedLibraryId
                && itemIds.Contains(x.OwnedPlexMediaId)
                && x.HitState == PlexMediaComparisonHitState.HigherQuality)
            .Select(x => x.OwnedPlexMediaId)
            .Distinct()
            .ToListAsync(ct);

        var showUpgradeIdSet = showUpgradeIds.ToHashSet();

        var remoteEpisodeCounts = await _dbContext.PlexTvShowEpisodes
            .Where(x => currentRemoteLibraryIds.Contains(x.PlexLibraryId))
            .GroupBy(x => x.TvShowId)
            .Select(g => new { RemotePlexMediaId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var remoteEpisodeCountLookup = remoteEpisodeCounts.ToDictionary(x => x.RemotePlexMediaId, x => x.Count);

        var showHits = await _dbContext.PlexTvShowComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == command.OwnedLibraryId
                && itemIds.Contains(x.OwnedPlexMediaId))
            .Select(x => new { x.RemotePlexMediaId, x.OwnedPlexMediaId })
            .ToListAsync(ct);

        var remoteToOwnedShowLookup = showHits
            .GroupBy(x => x.RemotePlexMediaId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.OwnedPlexMediaId).Distinct().ToList());

        var episodeHits = await (
            from comparison in _dbContext.PlexEpisodeComparisons
            join episode in _dbContext.PlexTvShowEpisodes on comparison.OwnedPlexMediaId equals episode.Id
            where currentRemoteLibraryIds.Contains(comparison.RemotePlexLibraryId)
                  && comparison.OwnedPlexLibraryId == command.OwnedLibraryId
                  && itemIds.Contains(episode.TvShowId)
            select new { episode.TvShowId, comparison.RemotePlexMediaId, comparison.OwnedPlexMediaId, comparison.HitState })
            .ToListAsync(ct);

        var episodeHitLookup = episodeHits
            .GroupBy(x => x.TvShowId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    MatchedCount = g.Select(x => x.OwnedPlexMediaId).Distinct().Count(),
                    HigherQualityCount = g.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality),
                });

        var remoteEpisodeCountByOwnedShow = remoteToOwnedShowLookup
            .SelectMany(x => x.Value.Select(ownedShowId => new
            {
                OwnedShowId = ownedShowId,
                RemoteEpisodeCount = remoteEpisodeCountLookup.GetValueOrDefault(x.Key),
            }))
            .GroupBy(x => x.OwnedShowId)
            .ToDictionary(x => x.Key, x => x.Max(y => y.RemoteEpisodeCount));

        foreach (var item in items)
        {
            var showId = item.Id;
            remoteEpisodeCountByOwnedShow.TryGetValue(showId, out var remoteEpisodeCount);
            episodeHitLookup.TryGetValue(showId, out var episodeHitSummary);
            var matchedEpisodeCount = episodeHitSummary?.MatchedCount ?? 0;
            var hasPartialMissingChildren = remoteEpisodeCount > 0 && matchedEpisodeCount < remoteEpisodeCount;
            var hasHigherQuality = showUpgradeIdSet.Contains(showId) || episodeHitSummary?.HigherQualityCount > 0;

            item.SetComparisonState((hasPartialMissingChildren, hasHigherQuality) switch
            {
                (true, true) => PlexMediaComparisonState.PartialAndHigherQuality,
                (true, false) => PlexMediaComparisonState.Partial,
                (false, true) => PlexMediaComparisonState.HigherQuality,
                _ => PlexMediaComparisonState.Owned,
            });
        }

        return Result.Ok();
    }

    private async Task<bool> HasPendingComparisonAsync(
        int ownedLibraryId,
        HashSet<int> remoteLibraryIds,
        CancellationToken ct) =>
        await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x =>
                x.OwnedPlexLibraryId == ownedLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && remoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing), ct);
}
