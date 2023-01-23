using Application.Contracts;

namespace PlexRipper.Application;

public class PlexMovieService : PlexMediaService, IPlexMovieService
{
    public PlexMovieService(IMediator mediator, IPlexApiService plexServiceApi) : base(mediator, plexServiceApi) { }
}