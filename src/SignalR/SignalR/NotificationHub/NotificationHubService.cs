using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

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
    public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hub.Clients.All.Notification(notification.ToDTO(), cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send notification");
        }
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(
        RefreshDataType dataType,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _hub.Clients.All.RefreshNotification(dataType, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send refresh notification");
        }
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
