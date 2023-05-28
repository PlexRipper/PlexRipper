using PlexRipper.WebAPI.SignalR.Common;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;

namespace PlexRipper.WebAPI.SignalR.Hubs;

[SignalRHub(autoDiscover: AutoDiscover.MethodsAndParams)]
public interface INotificationHub
{
    // This is meant to add MessageTypes to the Swagger schema
    [return: SignalRReturn(typeof(Task<MessageTypes>), 204)]
    [return: SignalRReturn(typeof(Task<NotificationDTO>), 200, "Success")]
    [SignalRMethod(operation: Operation.Get, autoDiscover: AutoDiscover.None)]
    Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default);
}