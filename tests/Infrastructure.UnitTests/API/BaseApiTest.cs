using PlexRipper.Application.Common.Models;
using PlexRipper.Infrastructure.API.Plex;

namespace Infrastructure.UnitTests.API
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


        public static OmbiHttpClient HttpClient { get; } = new OmbiHttpClient();
    }
}
