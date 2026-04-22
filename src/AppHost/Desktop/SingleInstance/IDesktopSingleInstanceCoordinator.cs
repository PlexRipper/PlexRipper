namespace Reaparr.AppHost;

/// <summary>
/// Coordinates ownership and relaunch signals for a single desktop instance.
/// </summary>
public interface IDesktopSingleInstanceCoordinator : IDisposable
{
    /// <summary>
    /// Attempts to acquire primary desktop ownership for this process.
    /// </summary>
    bool TryAcquirePrimaryOwnership();

    /// <summary>
    /// Sends a show-window signal to the primary desktop process.
    /// </summary>
    Task<Result> SignalPrimaryInstanceAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Starts listening for show-window signals in the primary desktop process.
    /// </summary>
    Result StartListener(Func<CancellationToken, Task<Result>> onSignal, CancellationToken cancellationToken);
}
