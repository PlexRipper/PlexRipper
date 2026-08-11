namespace Reaparr.Application;

/// <summary>
/// Schedules comparisons for every compatible remote-to-owned library pair affected by one library.
/// </summary>
/// <param name="PlexLibraryId">
/// The library whose sync, ownership, or access change should refresh comparison cache rows.
/// </param>
public record QueueLibraryComparisonJobsForLibraryCommand(int PlexLibraryId) : ICommand<Result>;

/// <summary>
/// Validates requests to discover comparison pairs affected by one library.
/// </summary>
public class QueueLibraryComparisonJobsForLibraryCommandValidator
    : AbstractValidator<QueueLibraryComparisonJobsForLibraryCommand>
{
    public QueueLibraryComparisonJobsForLibraryCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

/// <summary>
/// Discovers compatible remote-to-owned library pairs for one changed library and schedules each affected comparison.
/// </summary>
public class QueueLibraryComparisonJobsForLibraryCommandHandler
    : ICommandHandler<QueueLibraryComparisonJobsForLibraryCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IMediaQueryCache _mediaQueryCache;

    public QueueLibraryComparisonJobsForLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IMediaQueryCache mediaQueryCache
    )
    {
        _log = log.ForContext<QueueLibraryComparisonJobsForLibraryCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _mediaQueryCache = mediaQueryCache;
    }

    public async Task<Result> ExecuteAsync(
        QueueLibraryComparisonJobsForLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        var sourceLibrary = await _dbContext.PlexLibraries
            .Where(x => x.Id == command.PlexLibraryId)
            .SelectOwnership()
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceLibrary is null)
            return Result.Fail($"Library {command.PlexLibraryId} was not found or was disabled");

        if (sourceLibrary.Type is not PlexMediaType.Movie and not PlexMediaType.TvShow)
            return Result.Ok();

        var targetLibraries = await _dbContext.PlexLibraries
            .Where(x => x.Id != sourceLibrary.Id && x.Type == sourceLibrary.Type)
            .SelectOwnership()
            .ToListAsync(cancellationToken);

        // Comparison always flows remote-to-owned, regardless of which side changed.
        var pairs = sourceLibrary.IsOwned
            ? targetLibraries
                .Where(x => !x.IsOwned)
                .Select(x => (OwnedLibraryId: sourceLibrary.Id, RemoteLibraryId: x.Id))
            : targetLibraries
                .Where(x => x.IsOwned)
                .Select(x => (OwnedLibraryId: x.Id, RemoteLibraryId: sourceLibrary.Id));

        var scheduledCount = 0;
        var failedResults = new List<ResultBase>();
        var affectedLibraryIds = new HashSet<int>();
        foreach (var pair in pairs)
        {
            var result = await _commandExecutor.Send(
                new ScheduleLibraryComparisonJobCommand(pair.OwnedLibraryId, pair.RemoteLibraryId),
                cancellationToken
            );

            if (result.IsFailed)
            {
                failedResults.Add(result);
                continue;
            }

            affectedLibraryIds.Add(pair.RemoteLibraryId);
            affectedLibraryIds.Add(pair.OwnedLibraryId);
            scheduledCount++;
        }

        if (affectedLibraryIds.Count > 0)
        {
            _mediaQueryCache.InvalidateLibraries(
                affectedLibraryIds,
                $"Library comparisons scheduled for {sourceLibrary.Type}"
            );
        }

        _log.Here()
            .Debug(
                "Scheduled {ScheduledCount} library comparison jobs affected by library {PlexLibraryId}",
                scheduledCount,
                sourceLibrary.Id
            );

        return failedResults.Count > 0 ? Result.Merge(failedResults.ToArray()).LogError() : Result.Ok();
    }
}