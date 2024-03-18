using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.WebAPI.SignalR.Common;
using SignalRSwaggerGen.Attributes;

namespace PlexRipper.WebAPI.SignalR.Hubs;

public class NotificationHub : Hub<INotificationHub>
{
    private readonly ILog<NotificationHub> _log;

    public NotificationHub(ILog<NotificationHub> log)
    {
        _log = log;
    }

    public async Task Notification(
        [SignalRParam(paramType: typeof(NotificationDTO))]
        NotificationDTO notification,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending notification: {MessageTypesNotification} => {@NotificationDto}",
            MessageTypes.Notification.ToString(), notification);
        await Clients.All.Notification(notification, cancellationToken);
    }
}