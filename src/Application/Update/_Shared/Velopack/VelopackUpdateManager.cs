namespace Reaparr.Application;

public class VelopackUpdateManager : IVelopackUpdateManager
{
    private readonly UpdateManager _updateManager;

    private const string GithubRepoUrl = "https://github.com/Reaparr/Reaparr";

    public VelopackUpdateManager()
    {
        var source = new Velopack.Sources.GithubSource(
            GithubRepoUrl,
            string.Empty,
            EnvironmentExtensions.IsDevRelease()
        );
        _updateManager = new UpdateManager(source);
    }

    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync()
    {
        // Velopack's CheckForUpdatesAsync does not accept a CancellationToken.
        var updateInfo = await _updateManager.CheckForUpdatesAsync();

        return updateInfo is null
            ? AppUpdateCheckResult.NoUpdate()
            : AppUpdateCheckResult.UpdateAvailable(updateInfo.TargetFullRelease.Version.ToString());
    }

    public AppUpdatePendingRestart? GetPendingRestartAsset()
    {
        var pendingRestart = _updateManager.UpdatePendingRestart;
        return pendingRestart is null ? null : new AppUpdatePendingRestart(pendingRestart.Version.ToString());
    }

    public async Task<string?> DownloadUpdateAsync(CancellationToken cancellationToken)
    {
        // Single network call: fetch latest update info, then download it.
        // Velopack's CheckForUpdatesAsync does not accept a CancellationToken.
        var updateInfo = await _updateManager.CheckForUpdatesAsync();
        if (updateInfo is null)
            return null;

        await _updateManager.DownloadUpdatesAsync(updateInfo, null, cancellationToken);
        return updateInfo.TargetFullRelease.Version.ToString();
    }

    public void ApplyUpdateAndRestart(AppUpdatePendingRestart pendingRestart)
    {
        var asset = _updateManager.UpdatePendingRestart;
        if (asset is null || asset.Version.ToString() != pendingRestart.Version)
            throw new InvalidOperationException("No matching pending update is available to apply");

        _updateManager.ApplyUpdatesAndRestart(asset, []);
    }
}
