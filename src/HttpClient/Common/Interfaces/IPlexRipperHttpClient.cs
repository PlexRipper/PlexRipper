using RestSharp;

namespace PlexRipper.HttpClient.Common.Interfaces;

public interface IPlexRipperHttpClient
{
    Task<Stream> DownloadStream(RestRequest request);
}