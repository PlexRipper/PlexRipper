namespace Reaparr.Application;

/// <summary>
/// Invalidates every active comparison job in which one of the supplied libraries participates.
/// Queued jobs are deleted; running jobs receive cooperative cancellation and retain their history.
/// </summary>
public record InvalidateLibraryComparisonJobsCommand(IReadOnlyCollection<int> PlexLibraryIds) : ICommand<Result>;

public class InvalidateLibraryComparisonJobsCommandValidator : AbstractValidator<InvalidateLibraryComparisonJobsCommand>
{
    public InvalidateLibraryComparisonJobsCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibraryIds).NotEmpty();
        RuleForEach(x => x.PlexLibraryIds).GreaterThan(0);
    }
}

public class InvalidateLibraryComparisonJobsCommandHandler
    : ICommandHandler<InvalidateLibraryComparisonJobsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public InvalidateLibraryComparisonJobsCommandHandler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<InvalidateLibraryComparisonJobsCommandHandler>();
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        InvalidateLibraryComparisonJobsCommand command,
        CancellationToken cancellationToken
    )
    {
        var affectedLibraryIds = command.PlexLibraryIds.ToHashSet();
        var comparisonJobKeys = await _scheduler.GetJobKeys(JobTypes.LibraryComparisonJob, cancellationToken);
        var comparisonJobs = await Task.WhenAll(
            comparisonJobKeys.Select(async jobKey =>
                (JobKey: jobKey, Detail: await _scheduler.GetJobDetail(jobKey, cancellationToken))
            )
        );

        var jobKeysToDelete = comparisonJobs
            .Where(x =>
            {
                var payload = x.Detail?.JobDataMap.GetPayload<PlexLibraryComparisonJobPayload>();
                return payload is not null
                    && (
                        affectedLibraryIds.Contains(payload.OwnedPlexLibraryId)
                        || affectedLibraryIds.Contains(payload.RemotePlexLibraryId)
                    );
            })
            .Select(x => x.JobKey)
            .ToList();

        var result = await _scheduler.DeleteBatchJobs(jobKeysToDelete, cancellationToken);

        if (result.IsSuccess)
            _log.Here().Debug("Invalidated comparison jobs for libraries {LibraryIds}", command.PlexLibraryIds);

        return result;
    }
}
