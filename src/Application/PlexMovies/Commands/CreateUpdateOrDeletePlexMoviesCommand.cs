namespace PlexRipper.Application;

public class CreateUpdateOrDeletePlexMoviesCommand : IRequest<Result>
{
    public CreateUpdateOrDeletePlexMoviesCommand(PlexLibrary plexLibrary)
    {
        PlexLibrary = plexLibrary;
    }

    public PlexLibrary PlexLibrary { get; }
}