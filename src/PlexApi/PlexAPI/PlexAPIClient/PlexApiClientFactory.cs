using LukeHagar.PlexAPI.SDK;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class PlexApiClientFactory : IPlexApiClientFactory
{
    private readonly Func<PlexApiClientOptions?, IPlexApiClient> _clientFactory;

    public PlexApiClientFactory(Func<PlexApiClientOptions?, IPlexApiClient> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public IPlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, accessToken: authToken);

    public IPlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null)
    {
        options ??= new PlexApiClientOptions { ConnectionUrl = "https://plex.tv/api/v2" };
        return new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, accessToken: authToken);
    }
}
