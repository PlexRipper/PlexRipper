namespace PlexRipper.Application;

public class GetPlexLibraryByIdQuery : IRequest<Result<PlexLibrary>>
{
    /// <summary>
    /// Retrieves the <see cref="PlexLibrary"/> by its id from the database.
    /// </summary>
    /// <param name="id">The id to match</param>
    /// <param name="includePlexServer"></param>
    /// <param name="includeMedia">Include all media, this must be true to use topLevelMediaOnly </param>
    /// <param name="topLevelMediaOnly">Will only retrieve the top level media, movies without movieData, tvShows without seasons and episodes.</param>
    public GetPlexLibraryByIdQuery(int id, bool includePlexServer = false, bool includeMedia = false,bool topLevelMediaOnly = false)
    {
        Id = id;
        IncludePlexServer = includePlexServer;
        IncludeMedia = includeMedia;
        TopLevelMediaOnly = topLevelMediaOnly;
    }

    public int Id { get; }

    public bool IncludePlexServer { get; }

    public bool IncludeMedia { get; }

    public bool TopLevelMediaOnly { get; }
}