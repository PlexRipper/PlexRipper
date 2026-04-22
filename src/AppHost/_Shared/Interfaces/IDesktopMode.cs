namespace Reaparr.AppHost;

/// <summary>
/// Defines the contract for controlling desktop mode lifecycle.
/// </summary>
public interface IDesktopMode
{
    /// <summary>
    /// Starts desktop mode and prepares the main window.
    /// </summary>
    Task<Result> StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Shows or recreates the main desktop window.
    /// </summary>
    Task<Result> ShowMainWindowAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the visible main window to the background without stopping the host.
    /// </summary>
    Task<Result> CloseMainWindowAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Requests explicit desktop shutdown.
    /// </summary>
    Task<Result> ExitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits until explicit desktop shutdown is requested.
    /// </summary>
    Task WaitForExitAsync(CancellationToken cancellationToken);
}
