namespace PlexRipper.Application
{
    public class GetPlexServerByPlexMediaIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexMediaIdQuery(int mediaId, PlexMediaType plexMediaType)
        {
            MediaId = mediaId;
            PlexMediaType = plexMediaType;
        }

        public int MediaId { get; }

        public PlexMediaType PlexMediaType { get; }
    }
}