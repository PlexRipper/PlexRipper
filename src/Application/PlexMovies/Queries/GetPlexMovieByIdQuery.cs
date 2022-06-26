namespace PlexRipper.Application;

public class GetPlexMovieByIdQuery : IRequest<Result<PlexMovie>>
{
    public GetPlexMovieByIdQuery(int id, bool includeLibrary = false, bool includeServer = false)
    {
        Id = id;
        IncludeLibrary = includeLibrary;
        IncludeServer = includeServer;
    }

    public int Id { get; }

    public bool IncludeLibrary { get; }

    public bool IncludeServer { get; }
}