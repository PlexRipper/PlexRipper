using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;

namespace PlexRipper.WebAPI;

public class NotificationHub : Hub<INotificationHub>, INotificationHub
{
    private readonly ILog<NotificationHub> _log;

    public NotificationHub(ILog<NotificationHub> log)
    {
        _log = log;
    }

    public async Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default)
    {
        _log.Debug(
            "Sending notification: {MessageTypesNotification} => {@NotificationDto}",
            MessageTypes.Notification.ToString(),
            notification
        );
        await Clients.All.Notification(notification, cancellationToken);
    }

    public async Task RefreshNotification(DataType dataType, CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending refresh notification: {@DataType}", dataType);
        await Clients.All.RefreshNotification(dataType, cancellationToken);
    }
}
