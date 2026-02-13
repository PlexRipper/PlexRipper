using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.SignalR;

/// <summary>
/// Sends notification-related SignalR messages to the front-end via <see cref="NotificationHub"/>.
/// </summary>
public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHubService"/> class.
    /// </summary>
    public NotificationHubService(IHubContext<NotificationHub, INotificationHub> hub)
    {
        _hub = hub;
    }

    /// <inheritdoc/>
    public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _hub.Clients.All.Notification(notification.ToDTO(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(
        RefreshDataType dataType,
        CancellationToken cancellationToken = default
    )
    {
        await _hub.Clients.All.RefreshNotification(dataType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(
        List<RefreshDataType> dataTypes,
        CancellationToken cancellationToken = default
    )
    {
        await Task.WhenAll(dataTypes.Select(dataType => SendRefreshNotificationAsync(dataType, cancellationToken)));
    }
}
