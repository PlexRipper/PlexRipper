namespace PlexRipper.Application;

public class UpdatePlexServersCommand : IRequest<Result>
{
    public UpdatePlexServersCommand(List<PlexServer> plexServers)
    {
        PlexServers = plexServers;
    }

    public List<PlexServer> PlexServers { get; }
}