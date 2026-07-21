namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and TV show/season/episode hit rows onto remote-library
/// <see cref="PlexMediaSlimDTO"/> items, setting <see cref="PlexMediaSlimDTO.ComparisonState"/> per item in-place.
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
            return Result.Ok();

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

        var seasonHigherQuality = await _dbContext.PlexSeasonComparisons
            .Where(x =>
                x.RemotePlexLibraryId == command.RemoteLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && x.HitState == PlexMediaComparisonHitState.HigherQuality)
            .GroupBy(x => x.RemotePlexMediaId)
            .Select(g => new { RemotePlexMediaId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var seasonHqLookup = seasonHigherQuality.ToDictionary(x => x.RemotePlexMediaId, x => x.Count);

        for (var i = 0; i < items.Count; i++)
        {
            var showId = items[i].Id;
            var hasShowHit = showHitLookup.TryGetValue(showId, out var showHitsForItem);

            if (!hasShowHit)
            {
                items[i] = items[i] with { ComparisonState = PlexMediaComparisonState.Missing };
                continue;
            }

            var showHigherQualityCount = showHitsForItem!.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);

            seasonHqLookup.TryGetValue(showId, out var seasonHq);
            var totalHigherQuality = showHigherQualityCount + seasonHq;

            items[i] = items[i] with
            {
                ComparisonState = totalHigherQuality > 0
                    ? PlexMediaComparisonState.HigherQuality
                    : PlexMediaComparisonState.Owned,
            };
        }

        return Result.Ok();
    }
}