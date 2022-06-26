using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexTvShowService : PlexMediaService, IPlexTvShowService
    {
        public PlexTvShowService(IMediator mediator, IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi) : base(mediator, plexAuthenticationService, plexServiceApi) { }

        public async Task<Result<PlexTvShow>> GetTvShow(int id)
        {
            return await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(id));
        }
    }
}