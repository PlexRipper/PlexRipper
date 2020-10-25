using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Commands
{
    public class CreatePlexServerStatusCommand : IRequest<Result<int>>
    {
        public PlexServerStatus PlexServerStatus { get; }

        public CreatePlexServerStatusCommand(PlexServerStatus plexServerStatus)
        {
            PlexServerStatus = plexServerStatus;
        }
    }
}