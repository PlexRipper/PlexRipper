namespace Reaparr.Application;

/// <summary>
/// Handles application update checks, downloads, and apply for both desktop (Velopack) and Docker deployments.
/// </summary>
public class UpdateManager : IUpdateManager
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;
    private readonly Velopack.UpdateManager _velopackManager;

    public UpdateManager(ILogger log, ICommandExecutor commandExecutor, Velopack.UpdateManager velopackManager)
    {
        _log = log.ForContext<UpdateManager>();
        _commandExecutor = commandExecutor;
        _velopackManager = velopackManager;
    }

    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync()
    {
        var releasesResult = await _commandExecutor.Send(new GetGitHubReleasesCommand());
        if (releasesResult.IsFailed)
            return AppUpdateCheckResult.NoUpdate();

        var releases = releasesResult.Value;

        // Desktop mode
        if (EnvironmentExtensions.IsDesktopMode())
        {
            var updateInfo = await _velopackManager.CheckForUpdatesAsync();

            if (updateInfo is null)
            {
                _log.Here().Information("No update available");
                return AppUpdateCheckResult.NoUpdate();
            }

            var targetVersion = updateInfo.TargetFullRelease.Version.ToString();
            _log.Here().Information("Update available: {Version}", targetVersion);

            return AppUpdateCheckResult.UpdateAvailable(targetVersion, releases);
        }

        // Docker Mode
        var isDevRelease = EnvironmentExtensions.IsDevRelease();
        var latest = releases.FirstOrDefault(r => r.IsDevRelease == isDevRelease);
        if (latest is null)
            return AppUpdateCheckResult.NoUpdate();

        var latestVersion = latest.Version.TrimStart('v');

        if (!releases.Any())
        {
            _log.Here().Information("No update available");
            return AppUpdateCheckResult.NoUpdate();
        }

        _log.Here().Information("Update available: {Version}", latestVersion);

        return AppUpdateCheckResult.UpdateAvailable(latestVersion, releases);
    }

    public async Task<string?> DownloadUpdateAsync(CancellationToken cancellationToken)
    {
        if (!EnvironmentExtensions.IsDesktopMode())
        {
            _log.Here().Debug("Skipping update download — not running in desktop mode");
            return null;
        }

        // Single network call: fetch latest update info, then download it.
        // Velopack's CheckForUpdatesAsync does not accept a CancellationToken.
        var updateInfo = await _velopackManager.CheckForUpdatesAsync();
        if (updateInfo is null)
            return null;

        await _velopackManager.DownloadUpdatesAsync(updateInfo, null, cancellationToken);
        return updateInfo.TargetFullRelease.Version.ToString();
    }

    public void ApplyUpdateAndRestart()
    {
        if (!EnvironmentExtensions.IsDesktopMode())
        {
            _log.Here().Debug("Skipping update apply — not running in desktop mode");
            return;
        }

        var asset = _velopackManager.UpdatePendingRestart;
        _velopackManager.ApplyUpdatesAndRestart(asset, []);
    }
}
