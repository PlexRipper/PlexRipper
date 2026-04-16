namespace Reaparr.Application;

/// <summary>
/// Wraps Velopack update checks behind a testable interface.
/// </summary>
public interface IVelopackUpdateManager
{
    /// <summary>
    /// Checks whether an application update is available.
    /// </summary>
    Task<AppUpdateCheckResult> CheckForUpdatesAsync();

    /// <summary>
    /// Gets the downloaded update that is pending restart, if present.
    /// </summary>
    AppUpdatePendingRestart? GetPendingRestartAsset();

    /// <summary>
    /// Downloads the latest available update package.
    /// </summary>
    /// <returns>The downloaded update version, or <c>null</c> when no update is available.</returns>
    Task<string?> DownloadUpdateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Applies the downloaded update and restarts the application.
    /// </summary>
    void ApplyUpdateAndRestart(AppUpdatePendingRestart pendingRestart);
}
