using LukeHagar.PlexAPI.SDK.Utils;

namespace Reaparr.PlexApi.Contracts;

public interface IPlexApiClient : IDisposable, ISpeakeasyHttpClient
{
    Task<Result<ThrottledStream>> DownloadStreamAsync(
        HttpRequestMessage request,
        int downloadSpeedLimit,
        CancellationToken cancellationToken
    );
}
