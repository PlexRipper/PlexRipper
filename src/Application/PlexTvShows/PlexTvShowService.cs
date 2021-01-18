using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexTvShows
{
    public class PlexTvShowService : IPlexTvShowService
    {
        private readonly IMediator _mediator;


        public PlexTvShowService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<PlexTvShow>> GetTvShow(int id)
        {
            return await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(id));
        }

    }
}