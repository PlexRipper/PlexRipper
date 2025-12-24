using FastEndpoints;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class SendNotificationResultHandler : IEventHandler<SendNotificationResult>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ISignalRService _signalRService;

    public SendNotificationResultHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ISignalRService signalRService
    )
    {
        _log = log.ForContext<SendNotificationResultHandler>();
        _dbContextFactory = dbContextFactory;
        _signalRService = signalRService;
    }

    public async Task HandleAsync(SendNotificationResult notification, CancellationToken cancellationToken)
    {
        if (notification.Result.HasError<Error>())
        {
            // Create a new DbContext for this operation to avoid threading issues
            using var dbContext = await _dbContextFactory.CreateAsync();
            foreach (var error in notification.Result.Errors)
            {
                var createdNotification = new Notification(error);
                await dbContext.Notifications.AddAsync(createdNotification, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await _signalRService.SendNotificationAsync(createdNotification);
            }
        }
        else
            _log.Here().Warning("No errors to send as notifications from Result: {ResultObject}", notification.Result);
    }
}
