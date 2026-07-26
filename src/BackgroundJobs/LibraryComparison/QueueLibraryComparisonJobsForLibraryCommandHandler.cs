using Microsoft.EntityFrameworkCore;

namespace Reaparr.BackgroundJobs;

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
/// Discovers compatible remote-to-owned library pairs for one changed library and queues each affected comparison.
/// </summary>
public class QueueLibraryComparisonJobsForLibraryCommandHandler
    : ICommandHandler<QueueLibraryComparisonJobsForLibraryCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public QueueLibraryComparisonJobsForLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<QueueLibraryComparisonJobsForLibraryCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
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
                .Select(x => (RemoteLibraryId: x.Id, OwnedLibraryId: sourceLibrary.Id))
            : targetLibraries
                .Where(x => x.IsOwned)
                .Select(x => (RemoteLibraryId: sourceLibrary.Id, OwnedLibraryId: x.Id));

        var queuedCount = 0;
        var failedResults = new List<ResultBase>();
        foreach (var pair in pairs)
        {
            var result = await _commandExecutor.Send(
                new QueueLibraryMediaCompareJobCommand(pair.RemoteLibraryId, pair.OwnedLibraryId, sourceLibrary.Type),
                cancellationToken
            );

            if (result.IsFailed)
            {
                failedResults.AddRange(result);
                continue;
            }

            queuedCount++;
        }

        _log.Here()
            .Debug(
                "Queued {QueuedCount} library comparison jobs affected by library {PlexLibraryId}",
                queuedCount,
                sourceLibrary.Id
            );

        return failedResults.Count > 0 ? Result.Merge(failedResults.ToArray()).LogError() : Result.Ok();
    }
}