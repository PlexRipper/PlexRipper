using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries
{
    public class AddOrUpdatePlexLibrariesCommand : IRequest<Result<bool>>
    {
        public PlexAccount PlexAccount { get; }

        public PlexServer PlexServer { get; }

        public List<PlexLibrary> PlexLibraries { get; }

        public AddOrUpdatePlexLibrariesCommand(PlexAccount plexAccount, PlexServer plexServer, List<PlexLibrary> plexLibraries)
        {
            PlexAccount = plexAccount;
            PlexServer = plexServer;
            PlexLibraries = plexLibraries;
        }
    }
}