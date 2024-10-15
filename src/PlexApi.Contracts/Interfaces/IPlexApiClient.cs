using LukeHagar.PlexAPI.SDK.Utils;

namespace PlexApi.Contracts;

public interface IPlexApiClient : IDisposable, ISpeakeasyHttpClient
{
    Task<Stream?> DownloadStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
