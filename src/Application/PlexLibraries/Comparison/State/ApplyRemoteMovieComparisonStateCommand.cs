namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and movie hit rows onto remote-library <see cref="PlexMediaSlimDTO"/> items,
/// setting <see cref="PlexMediaSlimDTO.ComparisonId"/> per item in-place.
/// </summary>
/// <param name="Items">The overview page items from a remote library. Modified in-place.</param>
/// <param name="RemoteLibraryId">The remote movie Plex library being browsed.</param>
public record ApplyRemoteMovieComparisonStateCommand(
    List<PlexMediaSlimDTO> Items,
    int RemoteLibraryId
) : ICommand<Result>;

public class ApplyRemoteMovieComparisonStateCommandValidator
    : AbstractValidator<ApplyRemoteMovieComparisonStateCommand>
{
    public ApplyRemoteMovieComparisonStateCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Items).NotNull().WithMessage("Items must not be null.");
        RuleFor(x => x.RemoteLibraryId).GreaterThan(0).WithMessage("RemoteLibraryId must be greater than 0.");
    }
}

public class ApplyRemoteMovieComparisonStateCommandHandler
    : ICommandHandler<ApplyRemoteMovieComparisonStateCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public ApplyRemoteMovieComparisonStateCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<ApplyRemoteMovieComparisonStateCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(
        ApplyRemoteMovieComparisonStateCommand command,
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
                .Warning("Remote library {LibraryId} not found for comparison projection", command.RemoteLibraryId);
            return Result.Ok();
        }

        var ownedLibraries = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (ownedLibraries.Count == 0)
            return Result.Ok();

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.RemotePlexLibraryId == command.RemoteLibraryId
                && x.MediaType == PlexMediaType.Movie
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
            if (await HasPendingComparisonAsync(ownedLibraries.Keys.ToHashSet(), command.RemoteLibraryId, ct))
            {
                foreach (var item in items)
                    item.SetComparisonState(PlexMediaComparisonState.Pending);
            }

            return Result.Ok();
        }

        var itemIds = items.Select(x => x.Id).ToHashSet();

        var hits = await _dbContext.PlexMovieComparisons
            .Where(x =>
                x.RemotePlexLibraryId == command.RemoteLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && itemIds.Contains(x.RemotePlexMediaId))
            .Select(x => new { x.RemotePlexMediaId, x.OwnedPlexLibraryId, x.HitState })
            .ToListAsync(ct);

        var hitLookup = hits
            .GroupBy(x => x.RemotePlexMediaId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in items)
        {
            if (!hitLookup.TryGetValue(item.Id, out var itemHits))
            {
                item.SetComparisonState(PlexMediaComparisonState.Missing);
                continue;
            }

            var higherQualityCount = itemHits.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);

            item.SetComparisonState(higherQualityCount > 0
                ? PlexMediaComparisonState.HigherQuality
                : PlexMediaComparisonState.Owned);
        }

        return Result.Ok();
    }

    private async Task<bool> HasPendingComparisonAsync(
        HashSet<int> ownedLibraryIds,
        int remoteLibraryId,
        CancellationToken ct) => await _dbContext.HasActiveLibraryComparisonAsync(
        ownedLibraryIds.Select(x => PlexLibraryComparisonJob.GetJobKey(x, remoteLibraryId)),
        ct
    );
}