using LukeHagar.PlexAPI.SDK;

namespace Reaparr.PlexApi;

public class PlexApiClientFactory : IPlexApiClientFactory
{
    private readonly Func<PlexApiClientOptions?, IPlexApiClient> _clientFactory;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public PlexApiClientFactory(
        Func<PlexApiClientOptions?, IPlexApiClient> clientFactory,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _clientFactory = clientFactory;
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc/>
    public IPlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, token: authToken);

    /// <inheritdoc/>
    public async Task<Result<IPlexAPI>> CreateClientAsync(
        int plexServerId,
        int timeout = 10,
        int retryCount = 1,
        Action<HttpRequestRetryProgress>? retryProgressAction = null
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var authTokenResult = await dbContext.GetPlexServerTokenAsync(plexServerId, CancellationToken.None);
        if (authTokenResult.IsFailed)
            return authTokenResult.ToResult().LogIfFailed();

        var plexServerConnectionResult = await dbContext.ChoosePlexServerConnection(
            plexServerId,
            CancellationToken.None
        );
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogIfFailed();

        var client = CreateClient(
            authTokenResult.Value,
            new PlexApiClientOptions
            {
                ConnectionUrl = plexServerConnectionResult.Value.Url,
                Timeout = timeout,
                RetryCount = retryCount,
                RetryProgressAction = retryProgressAction,
            }
        );

        return Result.Ok(client);
    }

    /// <inheritdoc/>
    public IPlexAPI CreateClient(PlexApiClientOptions options) =>
        new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, token: string.Empty);

    /// <inheritdoc/>
    public IPlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null)
    {
        options ??= new PlexApiClientOptions { ConnectionUrl = "https://plex.tv/api/v2" };
        return new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, token: authToken);
    }
}
