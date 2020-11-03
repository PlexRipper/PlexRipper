using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class AddOrUpdatePlexLibrariesCommand : IRequest<Result<bool>>
    {
        public PlexAccount PlexAccount { get; }

        public List<PlexServer> PlexServers { get; }

        public AddOrUpdatePlexLibrariesCommand(PlexAccount plexAccount, List<PlexServer> plexServers)
        {
            PlexAccount = plexAccount;
            PlexServers = plexServers;
        }
    }
}