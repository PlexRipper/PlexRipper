using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.Notifications
{
    public class CreateNotificationCommand : IRequest<Result<bool>>
    {
        public Notification Notification { get; }

        public CreateNotificationCommand(Notification notification)
        {
            Notification = notification;
        }
    }
}