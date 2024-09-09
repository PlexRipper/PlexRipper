using System.Net;
using System.Net.Http.Headers;
using Application.Contracts;
using HttpClientToCurl;
using LukeHagar.PlexAPI.SDK.Utils;
using Polly;
using Polly.Wrap;
using Serilog.Events;
using ILog = Logging.Interface.ILog;

namespace PlexRipper.PlexApi;

public record PlexApiClientOptions
{
    public PlexServerConnection? Connection { get; init; }

    public string AuthToken { get; init; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int Timeout { get; init; } = 10;

    public int RetryCount { get; init; } = 1;

    public Action<PlexApiClientProgress>? Action { get; init; } = null;
}

public class NewPlexApiClient : ISpeakeasyHttpClient
{
    private readonly ILog _log;

    private readonly HttpClient _defaultClient;

    private readonly PlexApiClientOptions _options;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _policyWrap;

    public NewPlexApiClient(ILog log, IHttpClientFactory httpClientFactory, PlexApiClientOptions? options = null)
    {
        _log = log;
        _defaultClient = httpClientFactory.CreateClient();
        _defaultClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _options = options ?? new PlexApiClientOptions();

        _defaultClient.BaseAddress = new Uri(_options.Connection?.Url ?? "https://plex.tv/api/v2/");
        _defaultClient.Timeout = TimeSpan.FromSeconds(_options.Timeout);

        // Combine policies using Policy.WrapAsync
        _policyWrap = Policy.WrapAsync(
            Policy.TimeoutAsync<HttpResponseMessage>(_options.Timeout),
            Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _options.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    (response, timeSpan, retryAttempt, context) =>
                    {
                        _log.Warning(
                            "Request to {Url} failed, retrying {RetryAttempt} of {RetryCount} in {Delay}s",
                            context["RequestUri"],
                            retryAttempt,
                            _options.RetryCount,
                            timeSpan.TotalSeconds
                        );

                        SendProgressUpdate(
                            _options.Action,
                            response.Result,
                            retryAttempt,
                            _options.RetryCount,
                            (int)timeSpan.TotalSeconds
                        );
                    }
                ),
            Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(1, TimeSpan.FromSeconds(_options.Timeout))
        );
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        if (_log.IsLogLevelEnabled(LogEventLevel.Verbose))
        {
            var curl = _defaultClient.GenerateCurlInString(request);
            _log.Verbose("Request CURL: {RequestUrl}", curl);
        }
        else
            _log.Debug("Request: {RequestUrl}", request.RequestUri?.ToString());

        HttpResponseMessage response = null;

        try
        {
            response = await _policyWrap.ExecuteAsync(
                async (context) =>
                {
                    context["RequestUri"] = request.RequestUri?.ToString();

                    response = await _defaultClient.SendAsync(request);
                    return response;
                },
                new Context { { "RequestUri", request.RequestUri?.ToString() } }
            );
        }
        catch (Exception ex)
        {
            response = HandleTimeout(request);
        }
        finally
        {
            // Send final progress update
            SendProgressUpdate(_options.Action, response, _options.RetryCount, _options.RetryCount);
        }

        return response;
    }

    public Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request) => throw new NotImplementedException();

    private void SendProgressUpdate(
        Action<PlexApiClientProgress>? action,
        HttpResponseMessage? response,
        int retryAttempt,
        int retryCount,
        int timeToWaitSeconds = 0
    )
    {
        if (action == null || response == null)
            return;

        var request = response.RequestMessage;
        var url = request?.RequestUri?.ToString() ?? "Unknown";
        var msg = "Request successful!";

        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.RequestTimeout)
        {
            msg =
                $"Request to: {url} failed, waiting {timeToWaitSeconds} seconds before retrying again ({retryAttempt} of {retryCount})";
        }

        if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.RequestTimeout)
        {
            msg =
                $"Request to: {url} timed-out, waiting {timeToWaitSeconds} seconds before retrying again ({retryAttempt} of {retryCount})";
        }

        action.Invoke(
            new PlexApiClientProgress
            {
                StatusCode = (int)response.StatusCode,
                Message = msg,
                RetryAttemptIndex = retryAttempt,
                RetryAttemptCount = retryCount,
                TimeToNextRetry = timeToWaitSeconds,
                Completed = response.IsSuccessStatusCode || retryAttempt == retryCount,
                ErrorMessage = response.ReasonPhrase ?? "",
                ConnectionSuccessful = response.IsSuccessStatusCode,
            }
        );
    }

    private HttpResponseMessage HandleTimeout(HttpRequestMessage request)
    {
        _log.Error("Request to {Url} timed out.", request.RequestUri?.ToString());
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.GatewayTimeout,
            RequestMessage = request,
            Content = new StringContent("The gateway timed out while attempting to process the request."),
            ReasonPhrase = "Gateway Timeout",
        };
    }
}
