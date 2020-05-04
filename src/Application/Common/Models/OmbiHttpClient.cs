using PlexRipper.Application.Common.Interfaces.API;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Models
{
    /// <summary>
    /// The purpose of this class is simple, keep one instance of the HttpClient in play.
    /// There are many articles related to when using multiple HttpClient's keeping the socket in a WAIT state
    /// https://blogs.msdn.microsoft.com/alazarev/2017/12/29/disposable-finalizers-and-httpclient/
    /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    /// </summary>
    public class OmbiHttpClient : IOmbiHttpClient
    {
        public OmbiHttpClient()
        {

        }

        private static HttpClient _client;
        private static HttpMessageHandler _handler;

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            Setup();
            try
            {
                return await _client.SendAsync(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<string> GetStringAsync(Uri requestUri)
        {
            Setup();
            return await _client.GetStringAsync(requestUri);
        }

        private void Setup()
        {
            if (_client == null)
            {
                _handler ??= GetHandler();
                _client = new HttpClient(_handler);
                _client.DefaultRequestHeaders.Add("User-Agent", "PlexRipper");
            }
        }

        private HttpMessageHandler GetHandler()
        {
            return new HttpClientHandler();
        }
    }
}
