namespace PlexRipper.Application;

public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
{
    public GetPlexServerByIdQuery(int id, bool includeConnections = false, bool includeLibraries = false)
    {
        Id = id;
        IncludeConnections = includeConnections;
        IncludeLibraries = includeLibraries;
    }

    public int Id { get; }
    public bool IncludeConnections { get; }

    public bool IncludeLibraries { get; }
}