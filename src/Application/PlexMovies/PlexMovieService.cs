using PlexRipper.Application.Common;

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
