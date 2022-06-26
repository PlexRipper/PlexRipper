namespace PlexRipper.Application
{
    public class GetPlexTvShowsByPlexLibraryIdQuery : IRequest<Result<List<PlexTvShow>>>
    {
        public GetPlexTvShowsByPlexLibraryIdQuery(int plexLibraryId)
        {
            PlexLibraryId = plexLibraryId;
        }

        public int PlexLibraryId { get; }
    }
}