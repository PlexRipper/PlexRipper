using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlexRipper.Domain;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace PlexRipper.HttpClient
{
    public class PlexRipperHttpClient : IPlexRipperHttpClient
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

        private readonly System.Net.Http.HttpClient _httpClient;

        public PlexRipperHttpClient(System.Net.Http.HttpClient httpClient, AsyncRetryPolicy<HttpResponseMessage> policy = null)
        {
            policy ??= HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2));

            _policy = policy;
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _policy.ExecuteAsync(async () => await _httpClient.GetAsync(requestUri));
        }

        public async Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return await _policy.ExecuteAsync(async () => await _httpClient.GetAsync(requestUri, completionOption));
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return await _policy.ExecuteAsync(async () => await _httpClient.SendAsync(request, completionOption));
        }
    }
}