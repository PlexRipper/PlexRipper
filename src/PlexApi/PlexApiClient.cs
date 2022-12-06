using System.Net;
using System.Text.Json;
using PlexRipper.Application;
using PlexRipper.PlexApi.Converters;
using PlexRipper.PlexApi.Extensions;
using Polly;
using RestSharp;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

namespace PlexRipper.PlexApi;

public class PlexApiClient
{
    private readonly RestClient _client;

    public static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new LongToDateTime() },
    };

    public PlexApiClient(HttpClient httpClient)
    {
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
        Log.Debug($"Sending request to: {_client.BuildUri(request)}");

        return await _client.SendRequestWithPolly<T>(request, retryCount, action);
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
                )
                .ExecuteAsync(async () =>
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

    private static Result ParsePlexErrors(Result result, RestResponse response)
    {
        try
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                if (response.ErrorMessage.Contains("Timeout"))
                    result.Add408RequestTimeoutError(response.ErrorMessage);

                return result;
            }

            var content = response.Content;
            if (!string.IsNullOrEmpty(content))
            {
                var errorsResponse = JsonSerializer.Deserialize<PlexErrorsResponse>(content, SerializerOptions) ?? new PlexErrorsResponse();
                if (errorsResponse.Errors.Any())
                    result.WithErrors(errorsResponse.Errors);
            }
        }
        catch (Exception)
        {
            return result.WithError(new Error($"Failed to deserialize: {response}"));
        }

        return result;
    }
}