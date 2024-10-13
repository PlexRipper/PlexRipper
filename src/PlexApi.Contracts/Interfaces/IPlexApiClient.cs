using LukeHagar.PlexAPI.SDK.Utils;

namespace PlexRipper.PlexApi;

public interface IPlexApiClient : IDisposable, ISpeakeasyHttpClient
{
    Task<Stream?> DownloadStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
