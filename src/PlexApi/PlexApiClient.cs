using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
using RestSharp.Serialization.Json;
using RestSharp.Serialization.Xml;
using RestSharp.Serializers.SystemTextJson;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

        public async Task<Result<T>> SendRequestAsync<T>(RestRequest request, int retryCount = 2, Action<PlexApiClientProgress> action = null)
        {
            Serilog.Log.Verbose($"Sending request: {request.Resource}");

            IRestResponse<T> response = new RestResponse<T>();
            var policyResult = await Policy
                .Handle<WebException>()
                .OrInner<InvalidOperationException>()
                .OrInner<HttpRequestException>()
                .OrResult<IRestResponse<T>>(x => invalidStatusCodes.Contains(x.StatusCode) || x.StatusCode == 0)
                .WaitAndRetryAsync(retryCount, retryAttempt =>
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
                                Message = outcome.Result.ErrorMessage,
                            });
                        }

                        var result = outcome.Result;
                        Log.Error($"Error Response URI: {result.ResponseUri} - ({(int)result.StatusCode}){result.StatusDescription}");

                        if (!string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            Log.Error($"Error Message: {result.ErrorMessage}");
                        }

                        if (!string.IsNullOrEmpty(result.Content))
                        {
                            Log.Error($"Error Content: {result.Content}");
                        }
                    }
                ).ExecuteAndCaptureAsync(async () =>
                {
                    response = await _client.ExecuteAsync<T>(request);
                    return response;
                });

            if (policyResult.Outcome == OutcomeType.Successful)
            {
                response = policyResult.Result;
            }
            else
            {
                response.Request = request;
                response.ErrorException = policyResult.FinalException;
            }

            return GenerateResult(response);
        }

        public async Task<Result<byte[]>> SendImageRequestAsync(RestRequest request)
        {
            try
            {
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

        private Result<T> GenerateResult<T>(IRestResponse<T> response, Action<PlexApiClientProgress> action = null)
        {
            Result<T> result;
            if (response.IsSuccessful)
            {
                result = Result.Ok(response.Data);
                result.AddStatusCode((int)response.StatusCode);
                result.WithSuccess(new Success($"Request to {response.ResponseUri} ({response.StatusCode}) was successful!"));
                result.LogDebug();
            }
            else
            {
                result = Result.Fail($"PlexApi Error: Error on request to {response.ResponseUri} ({response.StatusCode}) - {response.Content}");

                // Plex sometimes gives some errors back
                result = ParsePlexErrors(result, response.Content);

                result.AddStatusCode((int)response.StatusCode);
                if (response.ErrorException != null)
                {
                    result.Errors.Add(new ExceptionalError(response.ErrorException));
                }

                result.LogError();
            }

            Log.Verbose($"Response was: {response.Content}");

            if (action is not null)
            {
                action(new PlexApiClientProgress
                {
                    StatusCode = (int)response.StatusCode,
                    Message = response.IsSuccessful ? "Request successful!" : response.ErrorMessage,
                    ConnectionSuccessful = response.IsSuccessful,
                    Completed = true,
                });
            }

            return result;
        }

        private static Result ParsePlexErrors(Result result, string jsonString)
        {
            try
            {
                if (!string.IsNullOrEmpty(jsonString))
                {
                    var errorsResponse = JsonSerializer.Deserialize<PlexErrorsResponse>(jsonString, SerializerOptions) ?? new PlexErrorsResponse();
                    if (errorsResponse.Errors.Any())
                    {
                        result.WithErrors(errorsResponse.Errors);
                    }
                }
            }
            catch (Exception e)
            {
                return result.WithError(new Error("Failed to deserialize").WithMessage(jsonString));
            }

            return result;
        }
    }
}