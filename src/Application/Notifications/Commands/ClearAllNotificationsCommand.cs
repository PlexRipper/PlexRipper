using FluentResults;
using MediatR;

namespace PlexRipper.Application.Notifications
{
    public class ClearAllNotificationsCommand : IRequest<Result>
    {
        public ClearAllNotificationsCommand() { }
    }
}