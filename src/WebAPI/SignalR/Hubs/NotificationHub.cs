using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;

namespace PlexRipper.WebAPI;

/// <summary>
///  The NotificationHub class is a SignalR hub that sends notifications to the front-end.
/// </summary>
public class NotificationHub : Hub<INotificationHub>, INotificationHub
{
    private readonly ILog<NotificationHub> _log;

    /// <summary>
    ///  Initializes a new instance of the <see cref="NotificationHub"/> class.
    /// </summary>
    /// <param name="log">  The <see cref="ILog{NotificationHub}"/> instance to use for logging.</param>
    public NotificationHub(ILog<NotificationHub> log)
    {
        _log = log;
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
    public async Task RefreshNotification(DataType dataType, CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending refresh notification: {@DataType}", dataType);
        await Clients.All.RefreshNotification(dataType, cancellationToken);
    }
}