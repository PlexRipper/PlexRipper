using RestSharp;

namespace HttpClient.Contracts;

public interface IPlexRipperHttpClient
{
    Task<Stream> DownloadStream(RestRequest request);
}