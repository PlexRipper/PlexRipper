using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetNotificationsQuery : IRequest<Result<List<Notification>>> { }