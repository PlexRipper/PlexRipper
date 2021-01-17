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
    public class PlexApiClient
    {
        private readonly IRestClient _client;

        public static JsonSerializerOptions SerializerOptions =>
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new LongToDateTime() },
            };

        private readonly IAsyncPolicy<IRestResponse> _policy2;

        private static readonly List<HttpStatusCode> invalidStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.BadGateway,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Forbidden,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.ServiceUnavailable,
        };

        public PlexApiClient(IRestClient client)
        {
            _client = client;
            _client.UseSystemTextJson(SerializerOptions);
            _client.UseDotNetXmlSerializer();
            _client.Timeout = 10000;

            _client.ThrowOnAnyError = true;
        }

        public async Task<Result<T>> SendRequestAsync<T>(RestRequest request)
        {
            IRestResponse<T> response = null;

            request = AddHeaders(request);

            var policyResult = await Policy
                .Handle<WebException>()
                .OrResult<IRestResponse<T>>(x => invalidStatusCodes.Contains(x.StatusCode) || x.StatusCode == 0)
                .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying again.");
                        return timeToWait;
                    }, (result, span) => { Log.Error(result.Result.ErrorException); }
                ).ExecuteAndCaptureAsync(() => _client.ExecuteAsync<T>(request));

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                response = policyResult.Result;
            }
            else
            {
                response = new RestResponse<T>
                {
                    Request = request,
                    ErrorException = policyResult.FinalException,
                };
            }

            var metaData = GetMetadata(response);
            if (response.IsSuccessful)
            {
                var result = Result.Ok(response.Data)
                    .WithReason(new Success($"Request to {response.ResponseUri} was successful!"))
                    .LogDebug();
                Log.Verbose($"Response was: {response.Content}");
                return result;
            }

            if (response.ErrorException != null)
            {
                return Result.Fail(new ExceptionalError(response.ErrorException).WithMessage(
                            $"PlexApi Error: Error on request to {response.ResponseUri} ({response.StatusCode}) - {response.Content}")
                        .WithMetadata(metaData))
                    .LogError();
            }

            return Result.Fail(new Error($"Time-out error on request {request.Resource}").WithMetadata(metaData)).LogError();
        }

        public async Task<byte[]> SendImageRequestAsync(RestRequest request)
        {
            request = AddHeaders(request);
            var _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying again.");
                        return timeToWait;
                    }
                );

            return await _policy
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        var response = await Task.Run(() => _client.DownloadData(request));
                        return response;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        throw;
                    }
                });
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