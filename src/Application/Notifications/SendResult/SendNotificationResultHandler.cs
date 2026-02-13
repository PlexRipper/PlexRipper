using FastEndpoints;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

public class SendNotificationResultHandler : IEventHandler<SendNotificationResult>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly INotificationHubService _notificationHubService;

    public SendNotificationResultHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<SendNotificationResultHandler>();
        _dbContextFactory = dbContextFactory;
        _notificationHubService = notificationHubService;
    }

    public async Task HandleAsync(SendNotificationResult notification, CancellationToken cancellationToken)
    {
        if (notification.Result.HasError<Error>())
        {
            // Create a new DbContext for this operation to avoid threading issues
            using var dbContext = await _dbContextFactory.CreateAsync();
            var createdNotifications = new List<Notification>();
            foreach (var error in notification.Result.Errors)
            {
                var createdNotification = new Notification(error);
                await dbContext.Notifications.AddAsync(createdNotification, cancellationToken);
                createdNotifications.Add(createdNotification);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var createdNotification in createdNotifications)
            {
                await _notificationHubService.SendNotificationAsync(createdNotification);
            }
        }
        else
            _log.Here().Warning("No errors to send as notifications from Result: {ResultObject}", notification.Result);
    }
}
