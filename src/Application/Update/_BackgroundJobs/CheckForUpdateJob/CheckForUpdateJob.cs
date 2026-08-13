using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public record CheckForUpdateJobPayload;

/// <summary>
/// Dispatches <see cref="CheckForUpdatesCommand"/> on a recurring schedule.
/// The command handles update checking and notifies the front-end when an update is found.
/// </summary>
public class CheckForUpdateJob : BaseBackgroundJob<CheckForUpdateJobPayload, AppUpdateCheckResult>
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;
    private AppUpdateCheckResult? _updateCheckResult;

    public CheckForUpdateJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IProgressHubService progressHubService
    )
        : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<CheckForUpdateJob>();
        _commandExecutor = commandExecutor;
    }

    protected override JobTypes JobType => JobTypes.CheckForUpdateJob;

    public static JobKey GetJobKey() => new(nameof(JobTypes.CheckForUpdateJob), JobTypes.CheckForUpdateJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<CheckForUpdateJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        _log.Here().Debug("Executing job: {JobName}", nameof(CheckForUpdateJob));

        var result = await _commandExecutor.Send(new CheckForUpdatesCommand(), cancellationToken);
        if (result.IsCancelled)
            throw new OperationCanceledException(cancellationToken);

        if (result.IsFailed)
        {
            result.LogError();
            throw new InvalidOperationException(
                $"Failed to check for application updates: {string.Join("; ", result.Errors.Select(x => x.Message))}"
            );
        }

        _updateCheckResult = result.Value;
    }

    protected override Task<AppUpdateCheckResult?> GetStatusUpdateDataAsync(
        TickerFunctionContext<CheckForUpdateJobPayload> context,
        CancellationToken cancellationToken
    ) => Task.FromResult(_updateCheckResult);
}