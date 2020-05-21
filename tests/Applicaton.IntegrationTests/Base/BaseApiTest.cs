using PlexRipper.Application.Common.Models;
using PlexRipper.Infrastructure.API.Plex;
using PlexRipper.Infrastructure.Services;

namespace PlexRipper.Application.IntegrationTests.Base
{
    public static class BaseApiTest
    {

        public static Api GetApi()
        {
            return new Api(BaseDependanciesTest.GetLogger<Api>(), HttpClient);
        }

        public static PlexApi GetPlexApi()
        {
            return new PlexApi(GetApi(), BaseDependanciesTest.GetLogger<PlexApi>());
        }

        public static PlexApiService GetPlexApiService()
        {
            return new PlexApiService(GetPlexApi(), BaseDependanciesTest.GetMapper());
        }


        public static PlexRipperHttpClient HttpClient { get; } = new PlexRipperHttpClient();
    }
}
