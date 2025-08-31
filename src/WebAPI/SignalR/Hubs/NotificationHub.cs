using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;
using Reaparr.Logging;

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
    public async Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default)
    {
        _log.Debug(
            "Sending notification: {MessageTypesNotification} => {@NotificationDto}",
            MessageTypes.Notification.ToString(),
            notification
        );
        await Clients.All.Notification(notification, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RefreshNotification(RefreshDataType dataType, CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending refresh notification: {@DataType}", dataType);
        await Clients.All.RefreshNotification(dataType, cancellationToken);
    }
}
