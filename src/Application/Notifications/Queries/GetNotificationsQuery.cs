using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.Notifications.Queries
{
    public class GetNotificationsQuery : IRequest<Result<List<Notification>>> { }
}