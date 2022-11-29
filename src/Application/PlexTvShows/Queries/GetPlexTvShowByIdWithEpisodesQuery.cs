namespace PlexRipper.Application;

public class GetPlexTvShowByIdWithEpisodesQuery : IRequest<Result<PlexTvShow>>
{
    public GetPlexTvShowByIdWithEpisodesQuery(int id, bool includePlexServer = false, bool includeLibrary = false)
    {
        Id = id;
        IncludeLibrary = includeLibrary;
        IncludePlexServer = includePlexServer;
    }

    public int Id { get; }

    public bool IncludeLibrary { get; }

    public bool IncludePlexServer { get; }
}