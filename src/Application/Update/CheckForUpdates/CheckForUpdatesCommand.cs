namespace Reaparr.Application;

/// <summary>
/// Checks whether a desktop application update is available via Velopack and notifies the front-end when one is found.
/// </summary>
public record CheckForUpdatesCommand : ICommand<Result<AppUpdateCheckResult>>;

public class CheckForUpdatesCommandHandler : ICommandHandler<CheckForUpdatesCommand, Result<AppUpdateCheckResult>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly UpdateManager _velopackManager;
    private readonly INotificationHubService _notificationHubService;

    public CheckForUpdatesCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        UpdateManager velopackManager,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<CheckForUpdatesCommandHandler>();
        _commandExecutor = commandExecutor;
        _velopackManager = velopackManager;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result<AppUpdateCheckResult>> ExecuteAsync(
        CheckForUpdatesCommand command,
        CancellationToken cancellationToken
    )
    {
        var releasesResult = await _commandExecutor.Send(new GetGitHubReleasesCommand(), cancellationToken);
        var releases = releasesResult.IsSuccess ? releasesResult.Value : [];

        // Desktop mode
        if (EnvironmentExtensions.IsDesktopMode())
        {
            if (!_velopackManager.IsInstalled)
            {
                _log.Here().Information("Skipping update check because the application is not installed");
                return Result.Ok(AppUpdateCheckResult.NoUpdate());
            }

            _log.Here()
                .Information(
                    "Checking for Velopack updates for {AppId} {CurrentVersion}",
                    _velopackManager.AppId,
                    _velopackManager.CurrentVersion
                );

            var updateResult = await Result.Try(() => _velopackManager.CheckForUpdatesAsync());
            if (updateResult.IsFailed)
                return updateResult.LogError();

            var updateInfo = updateResult.Value;
            if (updateInfo is null)
            {
                _log.Here().Information("No update available");
                return Result.Ok(AppUpdateCheckResult.NoUpdate());
            }

            var targetVersion = updateInfo.TargetFullRelease.Version.ToString();
            _log.Here().Information("Update available: {Version}", targetVersion);

            await _notificationHubService.SendRefreshNotificationAsync(
                RefreshDataType.UpdateAvailable,
                cancellationToken
            );
            return Result.Ok(AppUpdateCheckResult.UpdateAvailable(targetVersion, releases));
        }

        if (releasesResult.IsFailed)
            return AppUpdateCheckResult.NoUpdate();

        // Docker Mode
        var isDevRelease = EnvironmentExtensions.IsDevRelease();
        var latest = releases.FirstOrDefault(r => r.IsDevRelease == isDevRelease);
        if (latest is null)
        {
            _log.Here().Information("No update available");
            return Result.Ok(AppUpdateCheckResult.NoUpdate());
        }

        var latestVersion = latest.Version.TrimStart('v');

        _log.Here().Information("Update available: {Version}", latestVersion);

        await _notificationHubService.SendRefreshNotificationAsync(RefreshDataType.UpdateAvailable, cancellationToken);
        return Result.Ok(AppUpdateCheckResult.UpdateAvailable(latestVersion, releases));
    }
}
