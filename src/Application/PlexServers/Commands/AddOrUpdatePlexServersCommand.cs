namespace PlexRipper.Application;

public class AddOrUpdatePlexServersCommand : IRequest<Result>
{
    public List<PlexServer> PlexServers { get; }

    public AddOrUpdatePlexServersCommand(List<PlexServer> plexServers)
    {
        PlexServers = plexServers;
    }
}