namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and TV show/season/episode hit rows onto remote-library
/// <see cref="PlexMediaSlimDTO"/> items, setting <see cref="PlexMediaSlimDTO.ComparisonId"/> per item in-place.
/// </summary>
/// <param name="Items">The overview page items from a remote library. Modified in-place.</param>
/// <param name="RemoteLibraryId">The remote TV show Plex library being browsed.</param>
public record ApplyRemoteTvShowComparisonStateCommand(
    List<PlexMediaSlimDTO> Items,
    int RemoteLibraryId
) : ICommand<Result>;

public class ApplyRemoteTvShowComparisonStateCommandValidator
    : AbstractValidator<ApplyRemoteTvShowComparisonStateCommand>
{
    public ApplyRemoteTvShowComparisonStateCommandValidator()
    {
        RuleFor(x => x.Items).NotNull().WithMessage("Items must not be null.");
        RuleFor(x => x.RemoteLibraryId).GreaterThan(0).WithMessage("RemoteLibraryId must be greater than 0.");
    }
}

public class ApplyRemoteTvShowComparisonStateCommandHandler
    : ICommandHandler<ApplyRemoteTvShowComparisonStateCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public ApplyRemoteTvShowComparisonStateCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<ApplyRemoteTvShowComparisonStateCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(
        ApplyRemoteTvShowComparisonStateCommand command,
        CancellationToken ct)
    {
        var items = command.Items;
        if (items.Count == 0)
            return Result.Ok();

        var remoteUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == command.RemoteLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);

        if (remoteUpdatedAt is null)
        {
            _log.Here()
                .Warning("Remote library {LibraryId} not found for TV comparison projection", command.RemoteLibraryId);
            return Result.Ok();
        }

        var ownedLibraries = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .Where(x => x.Type == PlexMediaType.TvShow)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (ownedLibraries.Count == 0)
            return Result.Ok();

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.RemotePlexLibraryId == command.RemoteLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && ownedLibraries.Keys.Contains(x.OwnedPlexLibraryId))
            .ToListAsync(ct);

        var currentOwnedLibraryIds = scopeRows
            .Where(x =>
                x.RemoteLibraryUpdatedAt == remoteUpdatedAt
                && ownedLibraries.TryGetValue(x.OwnedPlexLibraryId, out var ownedUpdatedAt)
                && x.OwnedLibraryUpdatedAt == ownedUpdatedAt)
            .Select(x => x.OwnedPlexLibraryId)
            .ToHashSet();

        if (currentOwnedLibraryIds.Count == 0)
        {
            if (await HasPendingComparisonAsync(command.RemoteLibraryId, ownedLibraries.Keys.ToHashSet(), ct))
            {
                foreach (var item in items)
                    item.SetComparisonState(PlexMediaComparisonState.Pending);
            }

            return Result.Ok();
        }

        var itemIds = items.Select(x => x.Id).ToHashSet();

        var showHits = await _dbContext.PlexTvShowComparisons
            .Where(x =>
                x.RemotePlexLibraryId == command.RemoteLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && itemIds.Contains(x.RemotePlexMediaId))
            .Select(x => new { x.RemotePlexMediaId, x.OwnedPlexLibraryId, x.HitState })
            .ToListAsync(ct);

        var showHitLookup = showHits
            .GroupBy(x => x.RemotePlexMediaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var remoteEpisodes = await _dbContext.PlexTvShowEpisodes
            .Where(x => itemIds.Contains(x.TvShowId))
            .ToListAsync(ct);

        var remoteEpisodeCountLookup = remoteEpisodes
            .GroupBy(x => x.TvShowId)
            .ToDictionary(x => x.Key, x => x.Count());
        var episodeHits = await (
            from comparison in _dbContext.PlexEpisodeComparisons
            join episode in _dbContext.PlexTvShowEpisodes on comparison.RemotePlexMediaId equals episode.Id
            where comparison.RemotePlexLibraryId == command.RemoteLibraryId
                  && currentOwnedLibraryIds.Contains(comparison.OwnedPlexLibraryId)
                  && itemIds.Contains(episode.TvShowId)
            select new { episode.TvShowId, comparison.RemotePlexMediaId, comparison.HitState })
            .ToListAsync(ct);

        var episodeHitLookup = episodeHits
            .GroupBy(x => x.TvShowId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    MatchedCount = g.Select(x => x.RemotePlexMediaId).Distinct().Count(),
                    HigherQualityCount = g.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality),
                });

        for (var i = 0; i < items.Count; i++)
        {
            var showId = items[i].Id;
            var hasShowHit = showHitLookup.TryGetValue(showId, out var showHitsForItem);

            if (!hasShowHit)
            {
                items[i].SetComparisonState(PlexMediaComparisonState.Missing);
                continue;
            }

            var showHigherQualityCount = showHitsForItem!.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);

            remoteEpisodeCountLookup.TryGetValue(showId, out var remoteEpisodeCount);
            episodeHitLookup.TryGetValue(showId, out var episodeHitSummary);
            var matchedEpisodeCount = episodeHitSummary?.MatchedCount ?? 0;
            var hasPartialMissingChildren = remoteEpisodeCount > 0 && matchedEpisodeCount < remoteEpisodeCount;
            var totalHigherQuality = showHigherQualityCount + (episodeHitSummary?.HigherQualityCount ?? 0);

            items[i]
                .SetComparisonState((hasPartialMissingChildren, totalHigherQuality > 0) switch
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
        int remoteLibraryId,
        HashSet<int> ownedLibraryIds,
        CancellationToken ct) =>
        await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x =>
                x.RemotePlexLibraryId == remoteLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && ownedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing), ct);
}
