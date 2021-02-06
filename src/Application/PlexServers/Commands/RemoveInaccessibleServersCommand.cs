using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexServers
{
    public class RemoveInaccessibleServersCommand : IRequest<Result> { }
}