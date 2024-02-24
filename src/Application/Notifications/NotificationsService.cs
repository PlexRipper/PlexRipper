using Application.Contracts;
using Data.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public class NotificationsService : INotificationsService
{
    private readonly IMediator _mediator;

    private readonly ISignalRService _signalRService;

    public NotificationsService(IMediator mediator, ISignalRService signalRService)
    {
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public async Task<Result> HideNotification(int id) => await _mediator.Send(new HideNotificationCommand(id));

    public Task<Result> SendResult<T>(Result<T> result) => SendResult(result.ToResult());

    public async Task<Result<int>> ClearAllNotifications() => await _mediator.Send(new ClearAllNotificationsCommand());

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
                await _signalRService.SendNotificationAsync(notification);
            }
        }

        return Result.Ok();
    }
}