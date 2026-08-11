using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

/// <summary>
/// Schedules one native TickerQ comparison for an owned/remote Plex library pair.
/// </summary>
public record ScheduleLibraryComparisonJobCommand(
    int OwnedPlexLibraryId,
    int RemotePlexLibraryId
) : ICommand<Result>;

public class ScheduleLibraryComparisonJobCommandValidator
    : AbstractValidator<ScheduleLibraryComparisonJobCommand>
{
    public ScheduleLibraryComparisonJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
        RuleFor(x => x.OwnedPlexLibraryId)
            .NotEqual(x => x.RemotePlexLibraryId)
            .WithMessage("The remote and owned Plex libraries must be different.");
    }
}

public class ScheduleLibraryComparisonJobCommandHandler
    : ICommandHandler<ScheduleLibraryComparisonJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public ScheduleLibraryComparisonJobCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler
    )
    {
        _log = log.ForContext<ScheduleLibraryComparisonJobCommandHandler>();
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task<Result> ExecuteAsync(
        ScheduleLibraryComparisonJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var libraries = await _dbContext.PlexLibraries
            .Where(x => x.Id == command.RemotePlexLibraryId || x.Id == command.OwnedPlexLibraryId)
            .SelectOwnership()
            .ToListAsync(cancellationToken);

        var remoteLibrary = libraries.SingleOrDefault(x => x.Id == command.RemotePlexLibraryId);
        var ownedLibrary = libraries.SingleOrDefault(x => x.Id == command.OwnedPlexLibraryId);

        if (remoteLibrary is null)
            return Result.Fail($"Remote Plex library {command.RemotePlexLibraryId} was not found");
        if (ownedLibrary is null)
            return Result.Fail($"Owned Plex library {command.OwnedPlexLibraryId} was not found");
        if (remoteLibrary.IsOwned)
            return Result.Fail($"Remote Plex library {remoteLibrary.Id} is currently marked as owned");
        if (!ownedLibrary.IsOwned)
            return Result.Fail($"Owned Plex library {ownedLibrary.Id} is currently marked as remote");
        if (remoteLibrary.Type != ownedLibrary.Type)
            return Result.Fail(
                $"Cannot compare {remoteLibrary.Type} library {remoteLibrary.Id} with {ownedLibrary.Type} library {ownedLibrary.Id}"
            );
        if (remoteLibrary.Type is not PlexMediaType.Movie and not PlexMediaType.TvShow)
            return Result.Fail($"Library comparisons are not supported for media type {remoteLibrary.Type}");

        var jobKey = PlexLibraryComparisonJob.GetJobKey(ownedLibrary.Id, remoteLibrary.Id);
        var hasActiveTicker = await _dbContext.TimeTickers.AnyAsync(
            x => x.JobKey == jobKey.Name
                 && x.JobType == jobKey.Type
                 && (x.Status == TickerStatus.Idle
                     || x.Status == TickerStatus.Queued
                     || x.Status == TickerStatus.InProgress),
            cancellationToken
        );

        if (hasActiveTicker)
        {
            _log.Here()
                .Debug(
                    "Library comparison job is already active for remote library {RemoteLibraryId} and owned library {OwnedLibraryId}",
                    remoteLibrary.Id,
                    ownedLibrary.Id
                );
            return Result.Ok();
        }

        var tickerResult = await _backgroundJobScheduler.ExecuteJob<
            PlexLibraryComparisonJob,
            PlexLibraryComparisonJobPayload
        >(
            jobKey,
            new PlexLibraryComparisonJobPayload
            {
                RemotePlexLibraryId = remoteLibrary.Id,
                OwnedPlexLibraryId = ownedLibrary.Id,
            },
            cancellationToken
        );

        if (!tickerResult.IsSucceeded)
            return tickerResult.Exception is null
                ? Result.Fail("Failed to schedule library comparison job")
                : Result.Fail(new ExceptionalError(tickerResult.Exception));

        _log.Here()
            .Debug(
                "Scheduled library comparison job for remote library {RemoteLibraryId} and owned library {OwnedLibraryId}",
                remoteLibrary.Id,
                ownedLibrary.Id
            );

        return Result.Ok();
    }
}
