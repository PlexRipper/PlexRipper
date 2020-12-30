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
            this.UseSystemTextJson(SerializerOptions);
            this.UseDotNetXmlSerializer();
            ThrowOnAnyError = true;
            Timeout = 10000;
        }

        public async Task<Result<T>> SendRequestAsync<T>(RestRequest request)
        {
            request = AddHeaders(request);

            var response = await Policy
                .Handle<WebException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying ({request.Resource}) again.");
                        return timeToWait;
                    },
                    (exception, _, _) => { Log.Warning($"An exception occured at {exception.Source} with message {exception.Message}"); })
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        return await ExecuteAsync<T>(request);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        throw;
                    }
                });

            var metaData = GetMetadata(response);
            if (response.IsSuccessful)
            {
                var result = Result.Ok(response.Data)
                    .WithReason(new Success($"Request to {response.ResponseUri} was successful!")
                        .WithMetadata(metaData))
                    .LogDebug();
                Log.Verbose($"Response was: {response.Content}");
                return result;
            }

            return Result.Fail(new ExceptionalError(response.ErrorException).WithMessage(
                        $"PlexApi Error: Error on request to {request.Resource} ({response.StatusCode}) - {response.Content}")
                    .WithMetadata(metaData))
                .LogError();
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

        private Dictionary<string, object> GetMetadata(IRestResponse response)
        {
            var metaData = new Dictionary<string, object>
            {
                { "StatusCode", response.StatusCode.ToString() },
                { "Resource", response.Request.Resource },
            };

            if (response.ErrorMessage != string.Empty)
            {
                metaData.Add("ErrorMessage", response.ErrorMessage);
            }

            return metaData;
        }
    }
}