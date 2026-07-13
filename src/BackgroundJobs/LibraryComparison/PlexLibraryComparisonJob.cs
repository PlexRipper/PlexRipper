namespace Reaparr.BackgroundJobs;

/// <summary>
/// Quartz job that compares a remote Plex library against an owned Plex library
/// for a specific media type and stores comparison hit rows.
/// Triggered after library sync completes or ownership/access changes.
/// </summary>
public class PlexLibraryComparisonJob : IJob
{
    public const string RemoteLibraryIdParameter = nameof(RemoteLibraryIdParameter);
    public const string OwnedLibraryIdParameter = nameof(OwnedLibraryIdParameter);
    public const string MediaTypeParameter = nameof(MediaTypeParameter);

    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    private int _remoteLibraryId;
    private int _ownedLibraryId;
    private PlexMediaType _mediaType;

    public PlexLibraryComparisonJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<PlexLibraryComparisonJob>();
        _commandExecutor = commandExecutor;
    }

    public static JobKey GetJobKey(int remoteLibraryId, int ownedLibraryId, PlexMediaType mediaType) =>
        new(
            $"{nameof(JobTypes.LibraryComparisonJob)}_{remoteLibraryId}_{ownedLibraryId}_{mediaType}",
            nameof(JobTypes.LibraryComparisonJob)
        );

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var cancellationToken = context.CancellationToken;

        if (
            !dataMap.ContainsKey(RemoteLibraryIdParameter)
            || !dataMap.ContainsKey(OwnedLibraryIdParameter)
            || !dataMap.ContainsKey(MediaTypeParameter)
        )
        {
            _log.Here()
                .Error(
                    "Missing required parameters in job data map. RemoteId: {RemoteId}, OwnedId: {OwnedId}, MediaType: {MediaType}",
                    dataMap.ContainsKey(RemoteLibraryIdParameter),
                    dataMap.ContainsKey(OwnedLibraryIdParameter),
                    dataMap.ContainsKey(MediaTypeParameter)
                );
            return;
        }

        _remoteLibraryId = dataMap.GetInt(RemoteLibraryIdParameter);
        _ownedLibraryId = dataMap.GetInt(OwnedLibraryIdParameter);
        _mediaType = (PlexMediaType)dataMap.GetInt(MediaTypeParameter);

        _log.Here()
            .Debug(
                "Executing comparison job: remote library {RemoteLibId} vs owned library {OwnedLibId} for {MediaType}",
                _remoteLibraryId,
                _ownedLibraryId,
                _mediaType
            );

        // Jobs should swallow exceptions; Quartz will otherwise keep re-executing
        var result = await Result.Try(() =>
            _commandExecutor.Send(
                new CompareMoviePlexLibraryCommand(_remoteLibraryId, _ownedLibraryId, _mediaType),
                cancellationToken
            )
        );

        if (result.IsFailed)
        {
            result.LogError();
            _log.Here()
                .Warning(
                    "Comparison job failed for remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                    _remoteLibraryId,
                    _ownedLibraryId,
                    _mediaType
                );
        }
        else
        {
            _log.Here()
                .Information(
                    "Comparison job completed for remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                    _remoteLibraryId,
                    _ownedLibraryId,
                    _mediaType
                );
        }
    }
}
