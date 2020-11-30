using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class AddOrUpdatePlexServersCommand : IRequest<Result<bool>>
    {
        public PlexAccount PlexAccount { get; }

        public List<PlexServer> PlexServers { get; }

        public AddOrUpdatePlexServersCommand(PlexAccount plexAccount, List<PlexServer> plexServers)
        {
            PlexAccount = plexAccount;
            PlexServers = plexServers;
        }
    }
}