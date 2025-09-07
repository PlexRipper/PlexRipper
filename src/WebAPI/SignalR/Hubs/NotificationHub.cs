using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;

namespace Reaparr.WebAPI;

/// <summary>
///  The NotificationHub class is a SignalR hub that sends notifications to the front-end.
/// </summary>
public class NotificationHub : Hub<INotificationHub>, INotificationHub
{
    private readonly Serilog.ILogger _log;

    /// <summary>
    ///  Initializes a new instance of the <see cref="NotificationHub"/> class.
    /// </summary>
    /// <param name="log">  The <see cref="Serilog.ILogger"/> instance to use for logging.</param>
    public NotificationHub(Serilog.ILogger log)
    {
        _log = log.ForContext<NotificationHub>();
    }

    /// <inheritdoc/>
    public override Task OnConnectedAsync()
    {
        _log.Here()
            .Debug("Client connected to {HubName}: {ConnectionId}", nameof(NotificationHub), Context.ConnectionId);
        return base.OnConnectedAsync();
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
                    nameof(NotificationHub),
                    Context.ConnectionId
                );
        }
        else
        {
            _log.Here()
                .Debug(
                    "Client disconnected from {HubName}: {ConnectionId}",
                    nameof(NotificationHub),
                    Context.ConnectionId
                );
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <inheritdoc/>
    public async Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending notification: {MessageTypesNotification} => {@NotificationDto}",
                nameof(MessageTypes.Notification),
                notification
            );
        await Clients.All.Notification(notification, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RefreshNotification(RefreshDataType dataType, CancellationToken cancellationToken = default)
    {
        _log.Here().Debug("Sending refresh notification: {@DataType}", dataType);
        await Clients.All.RefreshNotification(dataType, cancellationToken);
    }
}
