using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using System.Threading.Tasks;

namespace PlexRipper.PlexApi
{
    public class PlexWebClient : RestClient
    {
        public ILogger Log { get; }

        public PlexWebClient(ILogger log)
        {
            Log = log;
            this.UseNewtonsoftJson();
        }

        public async Task<T> SendRequestAsync<T>(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await ExecuteAsync<T>(request);
            if (response.IsSuccessful)
            {
                Log.Information($"Request to {request.Resource} was successful!");
            }
            else
            {
                Log.Error(response.ErrorException, $"PlexApi Error: ({response.StatusCode}) {response.Content}");
            }
            return response.Data;
        }

        /// <summary>
        /// This will add the necessary headers to the request
        /// </summary>
        /// <param name="request">The request to change</param>
        /// <returns>The request with headers added</returns>
        private RestRequest AddHeaders(RestRequest request)
        {
            request.AddHeader("User-Agent", "PlexClient");
            request.AddHeader("X-Plex-Client-Identifier", "271938");
            // TODO Debate if we should put PlexRipper here
            request.AddHeader("X-Plex-Product", "Saverr");
            request.AddHeader("X-Plex-Version", "3");
            request.AddHeader("X-Plex-Device", "Ombi");
            request.AddHeader("X-Plex-Platform", "Web");
            return request;
        }
    }
}
