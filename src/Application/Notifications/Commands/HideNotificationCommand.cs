using FluentResults;
using MediatR;

namespace PlexRipper.Application.Notifications
{
    public class HideNotificationCommand : IRequest<Result>
    {
        public int Id { get; }

        public HideNotificationCommand(int id)
        {
            Id = id;
        }
    }
}