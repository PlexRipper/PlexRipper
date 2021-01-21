using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMedia;

namespace PlexRipper.Application.PlexMovies
{
    public class PlexMovieService : PlexMediaService, IPlexMovieService
    {
        private readonly IPlexApiService _plexServiceApi;

        public PlexMovieService(IMediator mediator, IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi) : base(mediator, plexAuthenticationService, plexServiceApi) { }
    }
}