namespace Reaparr.SignalR.Contracts;

public interface INotificationHubService
{
    /// <summary>
    /// Sends a notification to the front-end.
    /// </summary>
    Task SendNotificationAsync(Notification notification);

    /// <summary>
    /// Sends a refresh data notification to the front-end.
    /// </summary>
    Task SendRefreshNotificationAsync(RefreshDataType dataType);

    /// <summary>
    /// Sends multiple refresh data notifications to the front-end.
    /// </summary>
    Task SendRefreshNotificationAsync(List<RefreshDataType> dataTypes);
}
