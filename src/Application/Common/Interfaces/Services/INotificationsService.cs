namespace PlexRipper.Application;

public interface INotificationsService
{
    Task<Result<int>> CreateNotification(Notification notification);

    Task<Result<List<Notification>>> GetNotifications();

    Task<Result> HideNotification(int id);

    Task<Result> SendResult(Result result);
    Task<Result> SendResult<T>(Result<T> result);

    Task<Result> ClearAllNotifications();
}