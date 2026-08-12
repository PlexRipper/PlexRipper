namespace Reaparr.Application;

/// <summary>
/// Schedules comparisons for every compatible remote-to-owned library pair affected by one library.
/// </summary>
/// <param name="PlexLibraryId">
/// The library whose sync, ownership, or access change should refresh comparison cache rows.
/// </param>
public record ScheduleAffectedLibraryComparisonJobsCommand(int PlexLibraryId) : ICommand<Result>;

/// <summary>
/// Validates requests to discover comparison pairs affected by one library.
/// </summary>
public class ScheduleAffectedLibraryComparisonJobsCommandValidator
    : AbstractValidator<ScheduleAffectedLibraryComparisonJobsCommand>
{
    public ScheduleAffectedLibraryComparisonJobsCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

/// <summary>
/// Discovers compatible remote-to-owned library pairs for one changed library and schedules each affected comparison.
/// </summary>
public class ScheduleAffectedLibraryComparisonJobsCommandHandler
    : ICommandHandler<ScheduleAffectedLibraryComparisonJobsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IMediaQueryCache _mediaQueryCache;

    public ScheduleAffectedLibraryComparisonJobsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IMediaQueryCache mediaQueryCache
    )
    {
        _log = log.ForContext<ScheduleAffectedLibraryComparisonJobsCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _mediaQueryCache = mediaQueryCache;
    }

    public async Task<Result> ExecuteAsync(
        ScheduleAffectedLibraryComparisonJobsCommand command,
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

        var sourceLibraryDetails = await _dbContext.PlexLibraries
            .Where(x => x.Id == sourceLibrary.Id)
            .Select(x => new { x.PlexServerId, LibraryName = x.Title })
            .SingleAsync(cancellationToken);
        var serverName = await _dbContext.GetPlexServerNameById(sourceLibraryDetails.PlexServerId);
        var scheduledCount = 0;
        var failedResults = new List<ResultBase>();
        var affectedLibraryIds = new HashSet<int>();
        foreach (var pair in pairs)
        {
            var result = await _commandExecutor.Send(
                new ScheduleLibraryComparisonJobCommand(pair.OwnedLibraryId, pair.RemoteLibraryId),
                cancellationToken
            );

            if (result.IsCancelled)
            {
                _log.Here()
                    .Debug(
                        "Stopped scheduling affected library comparisons because shutdown was requested for server {ServerName} ({ServerId}), library {LibraryName} ({LibraryId})",
                        serverName,
                        sourceLibraryDetails.PlexServerId,
                        sourceLibraryDetails.LibraryName,
                        sourceLibrary.Id
                    );
                return result;
            }

            if (result.IsFailed)
            {
                result.LogWarning();
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

        return failedResults.Count > 0 ? Result.Merge(failedResults.ToArray()) : Result.Ok();
    }
}