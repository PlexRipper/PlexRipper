using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public record PlexLibraryComparisonJobPayload
{
    public required int OwnedPlexLibraryId { get; init; }

    public required int RemotePlexLibraryId { get; init; }
}

/// <summary>
/// Compares one remote Plex library with one owned Plex library.
/// </summary>
public class PlexLibraryComparisonJob
    : BaseBackgroundJob<PlexLibraryComparisonJobPayload, LibraryComparisonCompletedDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public PlexLibraryComparisonJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IProgressHubService progressHubService
    )
        : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<PlexLibraryComparisonJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    protected override JobTypes JobType => JobTypes.LibraryComparisonJob;

    protected override List<RefreshDataType> RefreshDataTypes => [];

    public static JobKey GetJobKey(int ownedPlexLibraryId, int remotePlexLibraryId) =>
        new(
            $"{nameof(JobTypes.LibraryComparisonJob)}_{ownedPlexLibraryId}_{remotePlexLibraryId}",
            JobTypes.LibraryComparisonJob
        );

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<PlexLibraryComparisonJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        var payload = context.Request;
        var libraries = await _dbContext
            .PlexLibraries.Where(x => x.Id == payload.RemotePlexLibraryId || x.Id == payload.OwnedPlexLibraryId)
            .SelectOwnership()
            .ToListAsync(cancellationToken);

        var remoteLibrary =
            libraries.SingleOrDefault(x => x.Id == payload.RemotePlexLibraryId)
            ?? throw new InvalidOperationException($"Remote Plex library {payload.RemotePlexLibraryId} was not found");
        var ownedLibrary =
            libraries.SingleOrDefault(x => x.Id == payload.OwnedPlexLibraryId)
            ?? throw new InvalidOperationException($"Owned Plex library {payload.OwnedPlexLibraryId} was not found");

        if (remoteLibrary.Id == ownedLibrary.Id)
            throw new InvalidOperationException("A Plex library cannot be compared with itself");

        if (remoteLibrary.IsOwned)
            throw new InvalidOperationException($"Remote Plex library {remoteLibrary.Id} is currently marked as owned");

        if (!ownedLibrary.IsOwned)
            throw new InvalidOperationException($"Owned Plex library {ownedLibrary.Id} is currently marked as remote");

        if (remoteLibrary.Type != ownedLibrary.Type)
            throw new InvalidOperationException(
                $"Cannot compare {remoteLibrary.Type} library {remoteLibrary.Id} with {ownedLibrary.Type} library {ownedLibrary.Id}"
            );

        _log.Here()
            .Information(
                "Comparing remote library {RemoteLibraryId} with owned library {OwnedLibraryId} for {MediaType}",
                remoteLibrary.Id,
                ownedLibrary.Id,
                remoteLibrary.Type
            );

        Result result;
        switch (remoteLibrary.Type)
        {
            case PlexMediaType.Movie:
                result = await _commandExecutor.Send(
                    new CompareMoviePlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id),
                    cancellationToken
                );
                break;
            case PlexMediaType.TvShow:
                result = await _commandExecutor.Send(
                    new CompareTvShowPlexLibraryCommand(ownedLibrary.Id, remoteLibrary.Id),
                    cancellationToken
                );
                break;
            default:
                result = Result.Fail($"Library comparisons are not supported for media type {remoteLibrary.Type}");
                break;
        }

        if (result.IsCancelled)
            throw new OperationCanceledException(cancellationToken);

        if (result.IsFailed)
        {
            result.LogError();
            throw new InvalidOperationException(
                $"Comparison of remote library {remoteLibrary.Id} with owned library {ownedLibrary.Id} failed: "
                    + string.Join("; ", result.Errors.Select(x => x.Message).ToArray())
            );
        }

        _log.Here()
            .Information(
                "Compared remote library {RemoteLibraryId} with owned library {OwnedLibraryId}",
                remoteLibrary.Id,
                ownedLibrary.Id
            );
    }

    protected override async Task<LibraryComparisonCompletedDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<PlexLibraryComparisonJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        var payload = context.Request;
        var mediaType = await _dbContext
            .PlexLibraries.Where(x => x.Id == payload.RemotePlexLibraryId)
            .Select(x => (PlexMediaType?)x.Type)
            .SingleOrDefaultAsync(cancellationToken);

        return mediaType is null
            ? null
            : new LibraryComparisonCompletedDTO
            {
                AffectedLibraryIds = [payload.OwnedPlexLibraryId, payload.RemotePlexLibraryId],
                MediaType = mediaType.Value,
                CompletedAt = DateTime.UtcNow,
            };
    }
}
