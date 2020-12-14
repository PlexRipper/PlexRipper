using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class UpdatePlexServersCommand : IRequest<Result>
    {
        public UpdatePlexServersCommand(List<PlexServer> plexServers)
        {
            PlexServers = plexServers;
        }

        public List<PlexServer> PlexServers { get; }
    }
}