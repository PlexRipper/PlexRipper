namespace PlexRipper.Application;

public class GetPlexServerByPlexTvShowEpisodeIdQuery : IRequest<Result<PlexServer>>
{
    public GetPlexServerByPlexTvShowEpisodeIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }
}