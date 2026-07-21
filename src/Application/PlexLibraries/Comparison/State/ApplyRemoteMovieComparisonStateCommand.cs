namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and movie hit rows onto remote-library <see cref="PlexMediaSlimDTO"/> items,
/// setting <see cref="PlexMediaSlimDTO.ComparisonState"/> per item in-place.
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
            _log.Here().Warning("Remote library {LibraryId} not found for comparison projection", command.RemoteLibraryId);
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
            return Result.Ok();

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

        var totalOwnedTargets = currentOwnedLibraryIds.Count;

        for (var i = 0; i < items.Count; i++)
        {
            if (!hitLookup.TryGetValue(items[i].Id, out var itemHits))
            {
                items[i] = items[i] with { ComparisonState = PlexMediaComparisonState.Missing };
                continue;
            }

            var ownedWithHit = itemHits.Select(x => x.OwnedPlexLibraryId).ToHashSet();
            var missingCount = totalOwnedTargets - ownedWithHit.Count;
            var higherQualityCount = itemHits.Count(x => x.HitState == PlexMediaComparisonHitState.HigherQuality);

            items[i] = items[i] with
            {
                ComparisonState = (missingCount, higherQualityCount) switch
                {
                    (> 0, _) => PlexMediaComparisonState.Missing,
                    (_, > 0) => PlexMediaComparisonState.HigherQuality,
                    _ => PlexMediaComparisonState.Owned,
                },
            };
        }

        return Result.Ok();
    }
}
