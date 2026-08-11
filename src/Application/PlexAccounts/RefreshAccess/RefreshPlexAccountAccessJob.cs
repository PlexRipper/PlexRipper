using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public sealed record RefreshPlexAccountAccessJobPayload;

/// <summary>
/// Periodically validates enabled Plex accounts and refreshes their server and library access.
/// </summary>
public class RefreshPlexAccountAccessJob
    : BaseBackgroundJob<RefreshPlexAccountAccessJobPayload, RefreshPlexAccountAccessRapportDTO>
{
    private readonly ICommandExecutor _commandExecutor;

    public RefreshPlexAccountAccessJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    ) : base(log, progressHubService, notificationHubService)
    {
        _commandExecutor = commandExecutor;
    }

    protected override JobTypes JobType => JobTypes.RefreshPlexAccountAccessJob;

    protected override List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexAccount, RefreshDataType.PlexServer, RefreshDataType.PlexLibrary];

    public static JobKey GetJobKey() =>
        new(nameof(JobTypes.RefreshPlexAccountAccessJob), JobTypes.RefreshPlexAccountAccessJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<RefreshPlexAccountAccessJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        var result = await _commandExecutor.Send(new RefreshPlexAccountAccessCommand(), cancellationToken);
        if (result.IsFailed)
            throw new InvalidOperationException(string.Join(System.Environment.NewLine, result.Errors.Select(x => x.Message)));
    }
}
