using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Notifications.Queries;
using PlexRipper.Domain;

namespace PlexRipper.Application.Notifications
{
    public class NotificationsService : INotificationsService
    {
        private readonly IMediator _mediator;

        public NotificationsService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<bool>> CreateNotification(Notification notification)
        {
            return await _mediator.Send(new CreateNotificationCommand(notification));
        }

        public async Task<Result<List<Notification>>> GetNotifications()
        {
            return await _mediator.Send(new GetNotificationsQuery());
        }
    }
}