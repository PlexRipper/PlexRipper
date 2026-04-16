namespace Reaparr.Application;

/// <summary>
/// Checks whether a desktop application update is available via Velopack and notifies the front-end when one is found.
/// </summary>
public record CheckForUpdatesCommand : ICommand<Result<AppUpdateCheckResult>>;

public class CheckForUpdatesCommandHandler : ICommandHandler<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>
{
    private readonly ILogger _log;
    private readonly IVelopackUpdateManager _updateManager;
    private readonly INotificationHubService _notificationHubService;

    public CheckForUpdatesCommandHandler(
        ILogger log,
        IVelopackUpdateManager updateManager,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<CheckForUpdatesCommandHandler>();
        _updateManager = updateManager;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result<AppUpdateCheckResult>> ExecuteAsync(
        CheckForUpdatesCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await _updateManager.CheckForUpdatesAsync();

            if (result.IsUpdateAvailable)
            {
                _log.Here().Information("Update available: {Version}", result.AvailableVersion);
                await _notificationHubService.SendRefreshNotificationAsync(
                    RefreshDataType.UpdateAvailable,
                    CancellationToken.None
                );
            }

            return Result.Ok(result);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
