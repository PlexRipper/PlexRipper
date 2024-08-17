using Application.Contracts;

namespace PlexRipper.WebAPI;

public interface INotificationHub
{
    Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default);

    Task RefreshNotification(DataType dataType, CancellationToken cancellationToken = default);
}
