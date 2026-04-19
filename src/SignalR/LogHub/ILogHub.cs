namespace Reaparr.SignalR;

/// <summary>
/// SignalR client contract for live log streaming.
/// </summary>
public interface ILogHub
{
    /// <summary>
    /// Sends a log event to connected clients.
    /// </summary>
    Task LogEvent(LiveLogEventDTO logEvent, CancellationToken cancellationToken = default);
}
