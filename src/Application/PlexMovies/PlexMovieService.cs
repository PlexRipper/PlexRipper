using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;

namespace PlexRipper.Application.PlexMovies
{
    public class PlexMovieService : IPlexMovieService
    {
        private readonly IPlexApiService _plexServiceApi;


        public PlexMovieService(
            IPlexApiService plexServiceApi)
        {
            _plexServiceApi = plexServiceApi;
        }

    }
}
