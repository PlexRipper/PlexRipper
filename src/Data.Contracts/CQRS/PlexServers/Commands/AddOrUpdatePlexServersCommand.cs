using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddOrUpdatePlexServersCommand : IRequest<Result>
{
    public List<PlexServer> PlexServers { get; }

    public AddOrUpdatePlexServersCommand(List<PlexServer> plexServers)
    {
        PlexServers = plexServers;
    }
}