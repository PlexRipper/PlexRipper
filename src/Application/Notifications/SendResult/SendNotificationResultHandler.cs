using Application.Contracts;
using Data.Contracts;
using Logging.Interface;

namespace PlexRipper.Application;

public class SendNotificationResultHandler : INotificationHandler<SendNotificationResult>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public SendNotificationResultHandler(ILog log, IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _log = log;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(SendNotificationResult notification, CancellationToken cancellationToken)
    {
        if (notification.Result.HasError<Error>())
        {
            foreach (var error in notification.Result.Errors)
            {
                var createdNotification = new Notification(error);
                await _dbContext.Notifications.AddAsync(createdNotification, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _signalRService.SendNotificationAsync(createdNotification);
            }
        }
        else
            _log.Warning("No errors to send as notifications from Result: {ResultObject}", notification.Result);
    }
}