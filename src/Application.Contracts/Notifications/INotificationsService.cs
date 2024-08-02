using FluentResults;

namespace Application.Contracts;

public interface INotificationsService
{
    Task<Result> SendResult(Result result);
    Task<Result> SendResult<T>(Result<T> result);
}
