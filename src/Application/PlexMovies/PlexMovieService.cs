using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMedia;

namespace PlexRipper.Application.PlexMovies
{
    public class PlexMovieService : PlexMediaService, IPlexMovieService
    {
        public PlexMovieService(IMediator mediator, IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi) : base(mediator, plexAuthenticationService, plexServiceApi) { }
    }
}