namespace PlexRipper.Application
{
    public class GetPlexServerNameByIdQuery : IRequest<Result<string>>
    {
        public GetPlexServerNameByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}