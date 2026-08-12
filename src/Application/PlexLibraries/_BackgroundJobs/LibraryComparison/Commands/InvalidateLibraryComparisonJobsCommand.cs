namespace Reaparr.Application;

/// <summary>
/// Invalidates every active comparison job in which one of the supplied libraries participates.
/// Queued jobs are deleted; running jobs receive cooperative cancellation and retain their history.
/// </summary>
public record InvalidateLibraryComparisonJobsCommand(IReadOnlyCollection<int> PlexLibraryIds)
    : ICommand<Result<BackgroundJobInvalidationResult>>;

public class InvalidateLibraryComparisonJobsCommandValidator
    : AbstractValidator<InvalidateLibraryComparisonJobsCommand>
{
    public InvalidateLibraryComparisonJobsCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibraryIds).NotEmpty();
        RuleForEach(x => x.PlexLibraryIds).GreaterThan(0);
    }
}

public class InvalidateLibraryComparisonJobsCommandHandler
    : ICommandHandler<InvalidateLibraryComparisonJobsCommand, Result<BackgroundJobInvalidationResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public InvalidateLibraryComparisonJobsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler
    )
    {
        _log = log.ForContext<InvalidateLibraryComparisonJobsCommandHandler>();
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task<Result<BackgroundJobInvalidationResult>> ExecuteAsync(
        InvalidateLibraryComparisonJobsCommand command,
        CancellationToken cancellationToken
    )
    {
        var libraryIds = command.PlexLibraryIds.Distinct().ToList();
        var matchingJobKeys = await _dbContext.TimeTickers
            .Where(x =>
                x.JobType == JobTypes.LibraryComparisonJob
                && x.RequestJson != null
                && (libraryIds.Contains(x.RequestJson.OwnedPlexLibraryId)
                    || libraryIds.Contains(x.RequestJson.RemotePlexLibraryId))
            )
            .Select(x => x.JobKey)
            .ToListAsync(cancellationToken);

        var jobKeys = matchingJobKeys
            .Select(x => new JobKey(x, JobTypes.LibraryComparisonJob))
            .ToList();

        var result = await _backgroundJobScheduler.DeleteBatchJobs(
            jobKeys,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            _log.Here()
                .Debug(
                    "Invalidated comparison jobs for libraries {LibraryIds}: deleted {DeletedCount}, requested cancellation for {CancellationCount}",
                    command.PlexLibraryIds,
                    result.Value.DeletedCount,
                    result.Value.CancellationRequestedCount
                );
        }

        return result;
    }
}