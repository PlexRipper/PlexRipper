using PlexRipper.Application.Common;

namespace PlexRipper.Application.PlexTvShows
{
    public class PlexTvShowService : IPlexTvShowService
    {
        private readonly IPlexApiService _plexServiceApi;

        public PlexTvShowService(IPlexApiService plexServiceApi)
        {
            _plexServiceApi = plexServiceApi;
        }
    }
}