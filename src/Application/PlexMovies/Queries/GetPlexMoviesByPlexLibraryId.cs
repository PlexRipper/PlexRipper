namespace PlexRipper.Application;

public class GetPlexMoviesByPlexLibraryId : IRequest<Result<List<PlexMovie>>>
{
    public GetPlexMoviesByPlexLibraryId(int plexLibraryId)
    {
        PlexLibraryId = plexLibraryId;
    }

    public int PlexLibraryId { get; }
}