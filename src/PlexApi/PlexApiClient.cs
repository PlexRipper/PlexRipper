using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;
using PlexRipper.PlexApi.Api;
using PlexRipper.PlexApi.Config.Converters;
using Polly;
using RestSharp;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers.SystemTextJson;

namespace PlexRipper.PlexApi
{
    public class PlexApiClient : RestClient
    {
        public static JsonSerializerOptions SerializerOptions =>
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new LongToDateTime() },
            };

        public PlexApiClient()
        {
            this.UseSystemTextJson();
            this.UseDotNetXmlSerializer();
            this.ThrowOnAnyError = true;
            Timeout = 10000;

            // TODO Ignore all bad SSL certificates based on user option set
        }

        public async Task<T> SendRequestAsync<T>(RestRequest request)
        {
            request = AddHeaders(request);

            return await Policy
                .Handle<WebException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying ({request.Resource}) again.");
                        return timeToWait;
                    },
                    (exception, pollyRetryCount, context) =>
                    {
                        Log.Warning($"An exception occured at {exception.Source} with message {exception.Message}");
                    })
                .ExecuteAsync(async () =>
                {
                    var response = await ExecuteAsync<T>(request);
                    if (response.IsSuccessful)
                    {
                        Log.Debug($"Request to {request.Resource} was successful!");
                        Log.Verbose($"Response was: {response.Content}");
                    }
                    else
                    {
                        Log.Error(response.ErrorException,
                            $"PlexApi Error: Error on request to {request.Resource} ({response.StatusCode}) - {response.Content}");
                    }

                    return response.Data;
                });
        }

        public async Task<Result<IRestResponse>> SendRequestAsync(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await ExecuteAsync(request);
            return ResultFromResponse(response);
        }

        public async Task<byte[]> SendImageRequestAsync(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await ExecuteAsync(request);

            return response.RawBytes;
        }

        /// <summary>
        /// This will add the necessary headers to the request.
        /// </summary>
        /// <param name="request">The request to change.</param>
        /// <returns>The request with headers added.</returns>
        private RestRequest AddHeaders(RestRequest request)
        {
            foreach (var headerPair in PlexHeaderData.GetBasicHeaders)
            {
                request.AddHeader(headerPair.Key, headerPair.Value);
            }

            return request;
        }

        private Result<IRestResponse> ResultFromResponse(IRestResponse response)
        {
            if (response.IsSuccessful)
            {
                Log.Information($"Request to {response.Request.Resource} was successful!");
                Log.Debug($"Response was: {response.Content}");
                return Result.Ok(response);
            }

            var msg = $"PlexApi Error: Error on request to {response.Request.Resource} ({response.StatusCode}) - {response.Content}";
            Log.Error(response.ErrorException, msg);

            var metadata = new Dictionary<string, object>
            {
                { "StatusCode", response.StatusCode },
                { "Message", response.ErrorMessage },
                { "Resource", response.Request.Resource },
            };
            var error = new Error("Plex Api Request Error").WithMetadata(metadata);
            return Result.Fail(error);
        }
    }
}