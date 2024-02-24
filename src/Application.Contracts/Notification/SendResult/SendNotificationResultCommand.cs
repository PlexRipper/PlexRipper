using FluentResults;
using MediatR;

namespace Application.Contracts;

public class SendNotificationResult : INotification
{
    public Result Result { get; }

    public SendNotificationResult(Result result)
    {
        Result = result;
    }
}