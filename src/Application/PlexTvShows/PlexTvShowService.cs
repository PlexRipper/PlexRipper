using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;

namespace PlexRipper.Application.PlexSeries
{
    public class PlexTvShowService : IPlexTvShowService
    {
        private readonly IPlexApiService _plexServiceApi;


        public PlexTvShowService(
            IPlexApiService plexServiceApi)
        {
            _plexServiceApi = plexServiceApi;
        }

    }
}
