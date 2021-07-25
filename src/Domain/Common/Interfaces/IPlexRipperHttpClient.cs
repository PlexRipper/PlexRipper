using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Domain
{
    public interface IPlexRipperHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);

        Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);
    }
}