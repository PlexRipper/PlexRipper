using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMedia;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
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