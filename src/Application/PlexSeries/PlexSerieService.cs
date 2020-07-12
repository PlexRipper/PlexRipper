using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;

namespace PlexRipper.Application.PlexSeries
{
    public class PlexSerieService : IPlexSerieService
    {
        private readonly IPlexApiService _plexServiceApi;


        public PlexSerieService(
            IPlexApiService plexServiceApi)
        {
            _plexServiceApi = plexServiceApi;
        }

    }
}
