using Application.Contracts;

namespace PlexRipper.Application;

// TODO Delete this
public class NotificationsService : INotificationsService
{
    private readonly ISignalRService _signalRService;

    public NotificationsService(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public Task<Result> SendResult<T>(Result<T> result) => SendResult(result.ToResult());

    public async Task<Result> SendResult(Result result)
    {
        if (result.HasError<Error>())
        {
            foreach (var error in result.Errors)
            {
                var notification = new Notification(error);
                // var notificationId = await CreateNotification(notification);
                // if (notificationId.IsFailed)
                //     return notificationId.ToResult();

               // notification.Id = notificationId.Value;
                await _signalRService.SendNotificationAsync(notification);
            }
        }

        return Result.Ok();
    }
}