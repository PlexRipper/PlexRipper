using FluentResults;
using MediatR;

namespace Application.Contracts;

public static class MediatrExtensions
{
    public static Task SendNotificationAsync(this IMediator mediator, Result result) =>
        mediator.Publish(new SendNotificationResult(result), CancellationToken.None);
}
