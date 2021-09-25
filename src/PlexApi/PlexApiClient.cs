using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using PlexRipper.Application.Common;
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

        private static readonly List<HttpStatusCode> invalidStatusCodes = new()
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

        public async Task<Result<T>> SendRequestAsync<T>(RestRequest request, Action<PlexApiClientProgress> action = null)
        {
            IRestResponse<T> response;

            request = AddHeaders(request);

            var retryAttemptCount = 3;
            var policyResult = await Policy
                .Handle<WebException>()
                .OrInner<InvalidOperationException>()
                .OrInner<HttpRequestException>()
                .OrResult<IRestResponse<T>>(x => invalidStatusCodes.Contains(x.StatusCode) || x.StatusCode == 0)
                .WaitAndRetryAsync(retryAttemptCount, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying again.");
                        return timeToWait;
                    },
                    (outcome, timespan, retryAttempt, _) =>
                    {
                        if (action is not null)
                        {
                            action(new PlexApiClientProgress
                            {
                                TimeToNextRetry = (int)timespan.TotalSeconds,
                                RetryAttemptIndex = retryAttempt,
                                RetryAttemptCount = retryAttemptCount,
                                Message = outcome.Result.ErrorMessage,
                            });
                        }

                        Log.Error(outcome.Result.ErrorMessage);
                    }
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
                if (action is not null)
                {
                    action(new PlexApiClientProgress
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = "Request successful.",
                        ConnectionSuccessful = true,
                        Completed = true,
                    });
                }

                var result = Result.Ok(response.Data)
                    .WithReason(new Success($"Request to {response.ResponseUri} was successful!"))
                    .LogDebug();
                Log.Verbose($"Response was: {response.Content}");

                return result;
            }

            if (response.ErrorException != null)
            {
                if (action is not null)
                {
                    action(new PlexApiClientProgress
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = response.ErrorMessage,
                        ConnectionSuccessful = false,
                        Completed = true,
                    });
                }

                return Result.Fail(new ExceptionalError(response.ErrorException).WithMessage(
                            $"PlexApi Error: Error on request to {response.ResponseUri} ({response.StatusCode}) - {response.Content}")
                        .WithMetadata(metaData))
                    .LogError();
            }

            string msg = $"Time-out error on request {request.Resource}";
            if (action is not null)
            {
                action(new PlexApiClientProgress
                {
                    StatusCode = (int)response.StatusCode,
                    Message = response.ErrorMessage,
                    ConnectionSuccessful = false,
                    Completed = true,
                });
            }

            return Result.Fail(new Error(msg).WithMetadata(metaData)).LogError();
        }

        public async Task<Result<byte[]>> SendImageRequestAsync(RestRequest request)
        {
            try
            {
                request = AddHeaders(request);
                var response = await Policy
                    .Handle<WebException>()
                    .WaitAndRetryAsync(1, retryAttempt =>
                        {
                            var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                            Log.Warning($"Waiting {timeToWait.TotalSeconds} seconds before retrying again.");
                            return timeToWait;
                        }
                    ).ExecuteAsync(async () =>
                    {
                        try
                        {
                            return await Task.Run(() => _client.DownloadData(request));
                        }
                        catch (Exception e)
                        {
                            Log.Error(e.Message);

                            // Needs to throw to catch and retry again
                            throw;
                        }
                    });

                if (response == null || response.Length < 200)
                {
                    return Result.Fail(new Error($"Image response was empty - Url: {request.Resource}")).LogError();
                }

                return Result.Ok(response);
            }
            catch (Exception e)
            {
                var result = Result.Fail(new ExceptionalError(e));
                if (e.Message.Contains("The operation has timed out"))
                {
                    return result.Add408RequestTimeoutError().LogError();
                }

                return result;
            }
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