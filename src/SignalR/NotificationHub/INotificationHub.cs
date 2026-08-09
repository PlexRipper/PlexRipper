namespace Reaparr.SignalR;

/// <summary>
///  The NotificationHub class is a SignalR hub that sends notifications to the front-end.
/// </summary>
public interface INotificationHub
{
    /// <summary>
    ///  Sends a notification to the front-end.
    /// </summary>
    /// <param name="notification"></param>
    Task Notification(NotificationDTO notification);

    /// <summary>
    ///  Refreshes the notification.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    Task RefreshNotification(RefreshDataType dataType);
}
