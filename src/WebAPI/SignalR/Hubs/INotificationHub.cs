using Application.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
///  The NotificationHub class is a SignalR hub that sends notifications to the front-end.
/// </summary>
public interface INotificationHub
{
    /// <summary>
    ///  Sends a notification to the front-end.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Notification(NotificationDTO notification, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Refreshes the notification.
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RefreshNotification(DataType dataType, CancellationToken cancellationToken = default);
}