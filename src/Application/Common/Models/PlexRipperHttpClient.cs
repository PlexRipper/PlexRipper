using PlexRipper.Application.Common.Interfaces.API;
using Serilog;
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
    public class PlexRipperHttpClient : IPlexRipperHttpClient
    {

        private static HttpClient _client;
        private static HttpClientHandler _handler;

        public PlexRipperHttpClient(ILogger logger)
        {
            Log = logger.ForContext<PlexRipperHttpClient>();
        }

        public ILogger Log { get; }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            Setup();
            try
            {
                return await _client.SendAsync(request);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Exception in {nameof(PlexRipperHttpClient)}:");
                throw;
            }
        }

        public async Task<string> GetStringAsync(Uri requestUri)
        {
            Setup();
            return await _client.GetStringAsync(requestUri);
        }

        public async Task<HttpResponseMessage> GetAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            Setup();
            try
            {
                return await _client.GetAsync(request.RequestUri, httpCompletionOption);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void Setup()
        {
            if (_client == null)
            {
                try
                {
                    _handler ??= new HttpClientHandler();
                    // Disable server SSL certificate validation 
                    // https://stackoverflow.com/a/44540071
                    _handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    _handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    _client = new HttpClient(_handler);
                    _client.DefaultRequestHeaders.Add("User-Agent", "PlexClient"); //TODO Debate if we should have PlexRipper in here
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Exception in {nameof(PlexRipperHttpClient)}:");
                    throw;
                }
            }
        }
    }
}
