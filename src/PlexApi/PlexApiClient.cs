using System.Net;
using System.Text.Json;
using FluentResultExtensions;
using Logging.Interface;
using PlexApi.Contracts;
using PlexRipper.PlexApi.Extensions;
using Polly;
using RestSharp;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

namespace PlexRipper.PlexApi;

public class PlexApiClient
{
    private readonly ILog _log;
    private readonly RestClient _client;

    public static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new LongToDateTime() },
    };

    public PlexApiClient(ILog log, HttpClient httpClient)
    {
        _log = log;
        var options = new RestClientOptions()
        {
            MaxTimeout = 10000,
            ThrowOnAnyError = false,
        };
        _client = new RestClient(httpClient, options);
        _client.UseSystemTextJson(SerializerOptions);
        _client.UseDotNetXmlSerializer();
    }

    public async Task<Result<T>> SendRequestAsync<T>(RestRequest request, int retryCount = 2, Action<PlexApiClientProgress> action = null) where T : class
    {
        _log.Debug("Sending request to: {Request}", _client.BuildUri(request));

        var response = await _client.SendRequestWithPolly<T>(request, retryCount, action);
        return GenerateResponseResult(response);
    }

    /// <summary>
    /// This method is used to send a request to the Plex API and download the image.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<byte[]>> SendImageRequestAsync(RestRequest request)
    {
        try
        {
            var response = await Policy
                .Handle<WebException>()
                .WaitAndRetryAsync(1, retryAttempt =>
                    {
                        var timeToWait = TimeSpan.FromSeconds(retryAttempt * 1);
                        _log.Warning("Waiting {TotalSeconds} seconds before retrying again", timeToWait.TotalSeconds);
                        return timeToWait;
                    }
                )
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        return await _client.DownloadDataAsync(request);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);

                        // Needs to throw to catch and retry again
                        throw;
                    }
                });

            if (response == null || response.Length < 200)
                return Result.Fail(new Error($"Image response was empty - Url: {request.Resource}")).LogError();

            return Result.Ok(response);
        }
        catch (Exception e)
        {
            var result = Result.Fail(new ExceptionalError(e));
            if (e.Message.Contains("The operation has timed out"))
                return result.Add408RequestTimeoutError().LogError();

            return result;
        }
    }

    /// <summary>
    /// Generates a <see cref="Result{T}"/> from the <see cref="RestResponse{T}"/>
    /// </summary>
    /// <param name="response"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static Result<T> GenerateResponseResult<T>(RestResponse<T> response) where T : class
    {
        var isSuccessful = response.IsSuccessful;

        var statusCode = (int)response.StatusCode;
        var statusDescription = isSuccessful ? response.StatusDescription : response.ErrorMessage;
        if (statusCode == 0 && statusDescription.Contains("Timeout"))
            statusCode = HttpCodes.Status504GatewayTimeout;

        if (isSuccessful)
            return Result.Ok(response.Data).AddStatusCode(statusCode, statusDescription).LogDebug();

        return ParsePlexErrors(response);
    }

    /// <summary>
    /// Parses the Plex errors and returns a <see cref="Result{T}"/>
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private static Result ParsePlexErrors(RestResponse response)
    {
        var requestUrl = response.Request.Resource;
        var statusCode = (int)response.StatusCode;
        var errorMessage = response.ErrorMessage;

        var result = Result.Fail($"Request to {requestUrl} failed with status code: {statusCode} - {errorMessage}");
        try
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage) && response.ErrorMessage.Contains("Timeout"))
                result.Add408RequestTimeoutError(response.ErrorMessage);
            else
                result.AddStatusCode(statusCode, errorMessage);

            var content = response.Content;
            if (!string.IsNullOrEmpty(content))
            {
                var errorsResponse = JsonSerializer.Deserialize<PlexErrorsResponseDTO>(content, SerializerOptions) ?? new PlexErrorsResponseDTO();
                if (errorsResponse.Errors != null && errorsResponse.Errors.Any())
                    result.WithErrors(errorsResponse.ToResultErrors());
            }
        }
        catch (Exception)
        {
            return result.WithError(new Error($"Failed to deserialize: {response}"));
        }

        return result.LogError();
    }
}