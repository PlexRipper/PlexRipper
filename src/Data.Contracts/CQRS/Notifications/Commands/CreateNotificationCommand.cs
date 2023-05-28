using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class CreateNotificationCommand : IRequest<Result<int>>
{
    public Notification Notification { get; }

    public CreateNotificationCommand(Notification notification)
    {
        Notification = notification;
    }
}