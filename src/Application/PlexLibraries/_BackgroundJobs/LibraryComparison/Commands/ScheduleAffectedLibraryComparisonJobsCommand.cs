using TickerQ.Utilities.Enums;

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
    private static readonly TimeSpan _comparisonJobDelay = TimeSpan.FromMinutes(2);

    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public ScheduleAffectedLibraryComparisonJobsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler
    )
    {
        _log = log.ForContext<ScheduleAffectedLibraryComparisonJobsCommandHandler>();
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
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

        // Resolve active comparison keys once before dispatching child commands, so repeated updates do not spam
        // schedule requests for pairs that are already queued or running.
        var activeComparisonJobKeys = await _dbContext.TimeTickers
            .Where(x =>
                x.JobType == JobTypes.LibraryComparisonJob
                && (x.Status == TickerStatus.Idle
                    || x.Status == TickerStatus.Queued
                    || x.Status == TickerStatus.InProgress)
            )
            .Select(x => x.JobKey)
            .ToHashSetAsync(cancellationToken);

        pairs = pairs.Where(pair =>
            !activeComparisonJobKeys.Contains(
                PlexLibraryComparisonJob.GetJobKey(pair.OwnedLibraryId, pair.RemoteLibraryId).Name
            )
        );

        var comparisonJobs = pairs
            .Select(pair =>
            {
                var jobKey = PlexLibraryComparisonJob.GetJobKey(pair.OwnedLibraryId, pair.RemoteLibraryId);
                var payload = new PlexLibraryComparisonJobPayload
                {
                    OwnedPlexLibraryId = pair.OwnedLibraryId,
                    RemotePlexLibraryId = pair.RemoteLibraryId,
                };
                return (jobKey, payload);
            })
            .ToList();

        if (comparisonJobs.Count == 0)
            return Result.Ok();

        var tickerResult = await _backgroundJobScheduler.ScheduleJobs<
            PlexLibraryComparisonJob,
            PlexLibraryComparisonJobPayload
        >(
            comparisonJobs,
            DateTime.UtcNow.Add(_comparisonJobDelay),
            cancellationToken
        );

        if (!tickerResult.IsSucceeded)
            return tickerResult.Exception is null
                ? Result.Fail("Failed to schedule affected library comparison jobs")
                : Result.Fail(new ExceptionalError(tickerResult.Exception));

        _log.Here()
            .Debug(
                "Scheduled {ScheduledCount} library comparison jobs affected by library {PlexLibraryId}",
                comparisonJobs.Count,
                sourceLibrary.Id
            );

        return Result.Ok();
    }
}