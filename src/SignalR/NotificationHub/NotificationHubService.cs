namespace Reaparr.SignalR;

/// <summary>
/// Sends notification-related SignalR messages to the front-end via <see cref="NotificationHub"/>.
/// </summary>
public class NotificationHubService : INotificationHubService
{
    private readonly ILogger _log;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHubService"/> class.
    /// </summary>
    public NotificationHubService(ILogger log, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _log = log.ForContext<NotificationHubService>();
        _hub = hub;
    }

    /// <inheritdoc/>
    public async Task SendNotificationAsync(Notification notification)
    {
        var result = await Result.Try(async Task () =>
            await _hub.Clients.All.Notification(notification.ToDTO(), CancellationToken.None));

        if (result.IsFailed)
        {
            result.LogError();
            _log.Here().Error("Failed to send notification");
        }
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(RefreshDataType dataType)
    {
        var result = await Result.Try(async Task () =>
            await _hub.Clients.All.RefreshNotification(dataType, CancellationToken.None));
        if (result.IsFailed)
        {
            result.LogError();
            _log.Here().Error("Failed to send refresh notification");
        }
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(List<RefreshDataType> dataTypes)
    {
        await Task.WhenAll(dataTypes.Select(SendRefreshNotificationAsync));
    }
}