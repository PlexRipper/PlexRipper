namespace PlexRipper.Application;

public class PlexMovieService : PlexMediaService, IPlexMovieService
{
    public PlexMovieService(
        IMediator mediator,
        IPlexAuthenticationService plexAuthenticationService,
        IPlexApiService plexServiceApi) : base(mediator, plexAuthenticationService, plexServiceApi) { }
}