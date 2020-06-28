using PlexRipper.PlexApi.Api;
using RestSharp;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers.SystemTextJson;
using Serilog;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlexRipper.PlexApi
{
    public class PlexWebClient : RestClient
    {
        public ILogger Log { get; }

        public PlexWebClient(ILogger log)
        {
            Log = log;
            this.UseSystemTextJson(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            });
            this.UseDotNetXmlSerializer();
            this.Timeout = 15000;

            // TODO Ignore all bad SSL certificates based on user option set
        }

        public async Task<T> SendRequestAsync<T>(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await ExecuteAsync<T>(request);
            if (response.IsSuccessful)
            {
                Log.Information($"Request to {request.Resource} was successful!");
                Log.Debug($"Response was: {response.Content}");
            }
            else
            {
                Log.Error(response.ErrorException, $"PlexApi Error: Error on request to {request.Resource} ({response.StatusCode}) - {response.Content}");
            }
            return response.Data;
        }

        public async Task<IRestResponse> SendRequestAsync(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                Log.Information($"Request to {request.Resource} was successful!");
                Log.Debug($"Response was: {response.Content}");
            }
            else
            {
                Log.Error(response.ErrorException, $"PlexApi Error: Error on request to {request.Resource} ({response.StatusCode}) - {response.Content}");
            }
            return response;
        }

        /// <summary>
        /// This will add the necessary headers to the request
        /// </summary>
        /// <param name="request">The request to change</param>
        /// <returns>The request with headers added</returns>
        private RestRequest AddHeaders(RestRequest request)
        {
            foreach (var headerPair in PlexHeaderData.GetBasicHeaders)
            {
                request.AddHeader(headerPair.Key, headerPair.Value);
            }
            return request;
        }
    }
}
