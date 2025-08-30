using LukeHagar.PlexAPI.SDK.Utils;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

public interface IPlexApiClient : IDisposable, ISpeakeasyHttpClient
{
    Task<ThrottledStream?> DownloadStreamAsync(
        HttpRequestMessage request,
        int downloadSpeedLimit,
        CancellationToken cancellationToken
    );
}
