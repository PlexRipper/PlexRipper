namespace PlexRipper.Application;

public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
{
    public GetPlexServerByIdQuery(int id, bool includeLibraries = false)
    {
        Id = id;
        IncludeLibraries = includeLibraries;
    }

    public int Id { get; }

    public bool IncludeLibraries { get; }
}