using RestSharp;

namespace PlexRipper.HttpClient;

public interface IPlexRipperHttpClient
{
    Task<Stream> DownloadStream(RestRequest request);
}