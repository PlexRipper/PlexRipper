using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.API
{
    public interface IOmbiHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<string> GetStringAsync(Uri requestUri);
    }
}
