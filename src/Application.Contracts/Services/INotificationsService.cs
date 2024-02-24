using FluentResults;

namespace Application.Contracts;

public interface INotificationsService
{
    Task<Result> HideNotification(int id);

    Task<Result> SendResult(Result result);
    Task<Result> SendResult<T>(Result<T> result);

    Task<Result<int>> ClearAllNotifications();
}