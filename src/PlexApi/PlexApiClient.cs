using System.Net;
using System.Text.Json;
using Application.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Polly;
using RestSharp;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.Xml;

namespace PlexRipper.PlexApi;

public class PlexApiClient
{
    #region Fields

    private readonly RestClient _client;
    private readonly ILog _log;

    #endregion

    #region Constructors

    public PlexApiClient(ILog log, HttpClient httpClient)
    {
        _log = log;
        var options = new RestClientOptions()
        {
            MaxTimeout = 60000,
            ThrowOnAnyError = false,
        };
        _client = new RestClient(httpClient, options);

        // HTTPS connections expect an user agent to be set
        _client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (X11; Linux x86_64)");
        _client.UseSystemTextJson(SerializerOptions);
        _client.UseDotNetXmlSerializer();
    }

    #endregion

    #region Properties

    public static JsonSerializerOptions SerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new LongToDateTime() },
    };

    #endregion

    #region Methods

    #region Public

    public async Task<Result<T>> SendRequestAsync<T>(RestRequest request, int retryCount = 2, Action<PlexApiClientProgress> action = null) where T : class
    {
        _log.Debug("Sending request to: {Request}", _client.BuildUri(request));

        var response = await _client.SendRequestWithPolly<T>(request, retryCount, action);
        return response.ToResponseResult();
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
            {
                var errorMsg = $"Image response was empty - Url: {_client.BuildUri(request)}";
                var result = Result.Fail(errorMsg);
                result.Add408RequestTimeoutError(errorMsg).LogError();
                return result;
            }

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

    #endregion

    #endregion
}