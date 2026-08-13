namespace Reaparr.Application;

/// <summary>
/// Projects stored comparison scopes and movie hit rows onto owned-library <see cref="PlexMediaSlimDTO"/> items,
/// marking them <see cref="PlexMediaComparisonState.HigherQuality"/> when any current remote scope has an upgrade hit.
/// </summary>
/// <param name="Items">The overview page items from an owned library. Modified in-place.</param>
/// <param name="OwnedLibraryId">The owned movie Plex library being browsed.</param>
public record ApplyOwnedMovieComparisonStateCommand(
    List<PlexMediaSlimDTO> Items,
    int OwnedLibraryId
) : ICommand<Result>;

public class ApplyOwnedMovieComparisonStateCommandValidator
    : AbstractValidator<ApplyOwnedMovieComparisonStateCommand>
{
    public ApplyOwnedMovieComparisonStateCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Items).NotNull().WithMessage("Items must not be null.");
        RuleFor(x => x.OwnedLibraryId).GreaterThan(0).WithMessage("OwnedLibraryId must be greater than 0.");
    }
}

public class ApplyOwnedMovieComparisonStateCommandHandler
    : ICommandHandler<ApplyOwnedMovieComparisonStateCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public ApplyOwnedMovieComparisonStateCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<ApplyOwnedMovieComparisonStateCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(
        ApplyOwnedMovieComparisonStateCommand command,
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
            _log.Here()
                .Warning("Owned library {LibraryId} not found for comparison projection", command.OwnedLibraryId);
            return Result.Ok();
        }

        var remoteLibraries = await _dbContext.PlexLibraries
            .WhereIsNotOwned()
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (remoteLibraries.Count == 0)
            return Result.Ok();

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.OwnedPlexLibraryId == command.OwnedLibraryId
                && x.MediaType == PlexMediaType.Movie
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
                foreach (var item in items)
                    item.SetComparisonState(PlexMediaComparisonState.Pending);
            }

            return Result.Ok();
        }

        var itemIds = items.Select(x => x.Id).ToHashSet();

        var upgradeIds = await _dbContext.PlexMovieComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == command.OwnedLibraryId
                && itemIds.Contains(x.OwnedPlexMediaId)
                && x.HitState == PlexMediaComparisonHitState.HigherQuality)
            .Select(x => x.OwnedPlexMediaId)
            .Distinct()
            .ToListAsync(ct);

        var upgradeIdSet = upgradeIds.ToHashSet();

        foreach (var item in items)
        {
            item.SetComparisonState(upgradeIdSet.Contains(item.Id)
                ? PlexMediaComparisonState.HigherQuality
                : PlexMediaComparisonState.Owned);
        }

        return Result.Ok();
    }

    private async Task<bool> HasPendingComparisonAsync(
        int ownedLibraryId,
        HashSet<int> remoteLibraryIds,
        CancellationToken ct) => await _dbContext.HasActiveLibraryComparisonAsync(
        remoteLibraryIds
            .Select(x => PlexLibraryComparisonJob.GetJobKey(ownedLibraryId, x)),
        ct
    );
}