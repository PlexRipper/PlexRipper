using LukeHagar.PlexAPI.SDK;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public interface IPlexApiClientFactory
{
    IPlexAPI CreateClient(string authToken, PlexApiClientOptions options);
    IPlexAPI CreateClient(PlexApiClientOptions options);

    IPlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null);
}
