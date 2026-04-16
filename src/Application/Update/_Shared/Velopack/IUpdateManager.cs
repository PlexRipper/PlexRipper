namespace Reaparr.Application;

/// <summary>
/// Checks, downloads, and applies application updates for both desktop and Docker deployments.
/// </summary>
public interface IUpdateManager
{
    /// <summary>
    /// Checks whether an application update is available.
    /// Returns all release notes since the current version, newest first.
    /// </summary>
    Task<AppUpdateCheckResult> CheckForUpdatesAsync();

    /// <summary>
    /// Downloads the latest available update package (desktop only).
    /// </summary>
    /// <returns>The downloaded update version, or <c>null</c> when no update is available or not in desktop mode.</returns>
    Task<string?> DownloadUpdateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Applies the downloaded update and restarts the application (desktop only).
    /// </summary>
    void ApplyUpdateAndRestart();
}
