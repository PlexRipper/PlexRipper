namespace PlexRipper.Application
{
    public class GetPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetPlexServersByPlexAccountIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}