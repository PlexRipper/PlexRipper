using Application.Contracts;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class PlexMovieService : IPlexMovieService
{
    public PlexMovieService(IMediator mediator, IPlexApiService plexServiceApi) { }
}