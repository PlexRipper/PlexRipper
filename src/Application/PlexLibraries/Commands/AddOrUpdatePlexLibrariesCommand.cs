namespace PlexRipper.Application;

public class AddOrUpdatePlexLibrariesCommand : IRequest<Result>
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