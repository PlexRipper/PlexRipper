using Data.Contracts;

namespace PlexRipper.Application;

public class PlexTvShowService : PlexMediaService, IPlexTvShowService
{
    public PlexTvShowService(IMediator mediator, IPlexApiService plexServiceApi) : base(mediator, plexServiceApi) { }

    public async Task<Result<PlexTvShow>> GetTvShow(int id)
    {
        return await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(id));
    }
}