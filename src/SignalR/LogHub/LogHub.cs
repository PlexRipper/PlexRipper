using Microsoft.AspNetCore.Authorization;

namespace Reaparr.SignalR;

/// <summary>
/// SignalR hub for live log streaming.
/// </summary>
[Authorize(Policy = "AuthenticatedUsers")]
public class LogHub : Hub<ILogHub>
{
    private readonly ILogger _log;

    public LogHub(ILogger log)
    {
        _log = log.ForContext<LogHub>();
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _log.Here().Debug("Client connected to {HubName}: {ConnectionId}", nameof(LogHub), Context.ConnectionId);
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _log.Here()
                .Error(
                    exception,
                    "Client disconnected with error from {HubName}: {ConnectionId}",
                    nameof(LogHub),
                    Context.ConnectionId
                );
        }
        else
        {
            _log.Here()
                .Debug("Client disconnected from {HubName}: {ConnectionId}", nameof(LogHub), Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
