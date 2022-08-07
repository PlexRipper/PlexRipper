using PlexRipper.Application.Notifications.Queries;

namespace PlexRipper.Application.Notifications;

public class NotificationsService : INotificationsService
{
    private readonly IMediator _mediator;

    private readonly ISignalRService _signalRService;

    public NotificationsService(IMediator mediator, ISignalRService signalRService)
    {
        _mediator = mediator;
        _signalRService = signalRService;
    }

    /// <summary>
    /// Creates a <see cref="Notification"/> in the database.
    /// </summary>
    /// <param name="notification">The Notification to create.</param>
    /// <returns>The Id of the created <see cref="Notification"/>.</returns>
    public async Task<Result<int>> CreateNotification(Notification notification)
    {
        return await _mediator.Send(new CreateNotificationCommand(notification));
    }

    public async Task<Result<List<Notification>>> GetNotifications()
    {
        return await _mediator.Send(new GetNotificationsQuery());
    }

    public async Task<Result> HideNotification(int id)
    {
        return await _mediator.Send(new HideNotificationCommand(id));
    }

    public Task<Result> SendResult<T>(Result<T> result)
    {
        return SendResult(result.ToResult());
    }

    public async Task<Result> ClearAllNotifications()
    {
        return await _mediator.Send(new ClearAllNotificationsCommand());
    }

    public async Task<Result> SendResult(Result result)
    {
        if (result.HasError<Error>())
        {
            foreach (var error in result.Errors)
            {
                var notification = new Notification(error);
                var notificationId = await CreateNotification(notification);
                if (notificationId.IsFailed)
                    return notificationId.ToResult();

                notification.Id = notificationId.Value;
                await _signalRService.SendNotification(notification);
            }
        }

        return Result.Ok();
    }
}