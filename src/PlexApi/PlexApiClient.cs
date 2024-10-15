using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Contracts;
using HttpClientToCurl;
using PlexApi.Contracts;
using PlexRipper.Domain.Config;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using Serilog.Events;
using ILog = Logging.Interface.ILog;

namespace PlexRipper.PlexApi;

public class PlexApiClient : IPlexApiClient
{
    private readonly ILog _log;

    private readonly HttpClient _defaultClient;

    private readonly PlexApiClientOptions _options;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _policyWrap;

    public PlexApiClient(ILog log, HttpClient httpClient, PlexApiClientOptions options)
    {
        _log = log;
        _defaultClient = httpClient;
        _defaultClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _options = options;

        if (_options.ConnectionUrl != string.Empty)
        {
            _defaultClient.BaseAddress = new Uri(_options.ConnectionUrl);
        }

        _defaultClient.Timeout = TimeSpan.FromSeconds(_options.Timeout);

        // Combine policies using Policy.WrapAsync
        _policyWrap = Policy.WrapAsync(
            Policy.TimeoutAsync<HttpResponseMessage>(_options.Timeout),
            Policy
                .Handle<TimeoutRejectedException>()
                .Or<HttpRequestException>(e => e.InnerException is TimeoutException) // Retry on request-level timeout
                .OrResult<HttpResponseMessage>(r =>
                    r.StatusCode
                        is HttpStatusCode.RequestTimeout
                            or HttpStatusCode.GatewayTimeout
                            or HttpStatusCode.ServiceUnavailable
                            or HttpStatusCode.InternalServerError
                )
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
                )
        );
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        // Remove the auto-generated user-agent header from Plex SDK
        request.Headers.Remove("user-agent");

        if (_log.IsLogLevelEnabled(LogEventLevel.Verbose))
        {
            var curl = _defaultClient.GenerateCurlInString(request);
            _log.Verbose("Request CURL: {RequestUrl}", curl);
        }
        else
            _log.Debug("Request: {RequestUrl}", request.RequestUri?.ToString());

        HttpResponseMessage? response = null;
        var requestUri = request.RequestUri?.ToString();
        try
        {
            response = await _policyWrap.ExecuteAsync(
                async (context) =>
                {
                    context["RequestUri"] = requestUri;

                    // Clone the request to avoid issues with the content being disposed
                    request = await CloneAsync(request);

                    try
                    {
                        response = await _defaultClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                        if (!response.IsSuccessStatusCode)
                        {
                            response.Content = ToJsonResponse(response);
                        }

                        return response;
                    }
                    catch (TaskCanceledException e)
                    {
                        if (ShouldLog(request))
                        {
                            _log.Here().Warning("{message} to {Url}", e.Message, requestUri);
                        }

                        return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
                    }
                    catch (HttpRequestException e)
                    {
                        if (ShouldLog(request))
                        {
                            _log.Here().Warning("{message} to {Url}", e.Message, requestUri);
                        }

                        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    }
                    catch (Exception e)
                    {
                        if (ShouldLog(request))
                        {
                            Result.Fail(new ExceptionalError(e)).LogError();
                        }

                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }
                },
                new Context { { "RequestUri", requestUri } }
            );
        }
        catch (TimeoutRejectedException)
        {
            _log.Error("Request to {Url} timed out.", requestUri);
            response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent("The gateway timed out while attempting to process the request."),
                ReasonPhrase = "Gateway Timeout",
                RequestMessage = request,
            };
        }
        catch (HttpRequestException ex)
        {
            _log.Here()
                .Error(
                    "Exception Error ({ExceptionName}) sending request to {Url}",
                    nameof(HttpRequestException),
                    requestUri
                );
            response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(ex.Message),
                ReasonPhrase = ex.HttpRequestError.ToString(),
                RequestMessage = request,
            };
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error("Exception Error ({ExceptionName}) sending request to {Url}", nameof(ex.GetType), requestUri);
            response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(ex.Message),
                ReasonPhrase = ex.Message,
                RequestMessage = request,
            };
        }
        finally
        {
            // Send final progress update
            SendProgressUpdate(_options.Action, response, _options.RetryCount, _options.RetryCount);
        }

        return response;
    }

    public async Task<Stream?> DownloadStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await SendAsync(request);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // Copy the request's content (via a MemoryStream) into the cloned object
        var ms = new MemoryStream();
        if (request.Content != null)
        {
            await request.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            // Copy the content headers
            foreach (var h in request.Content.Headers)
                clone.Content.Headers.Add(h.Key, h.Value);
        }

        clone.Version = request.Version;

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }

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

        if (response is { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.RequestTimeout })
        {
            msg =
                $"Request to: {url} failed, waiting {timeToWaitSeconds} seconds before retrying again ({retryAttempt} of {retryCount})";
        }

        if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.RequestTimeout })
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

    private bool ShouldLog(HttpRequestMessage request)
    {
        // Don't log identity requests
        return !request.RequestUri?.PathAndQuery.Contains("identity", StringComparison.OrdinalIgnoreCase) ?? true;
    }

    private HttpContent ToJsonResponse(HttpResponseMessage message)
    {
        if (message.Content.Headers.ContentType?.MediaType != "text/html")
        {
            return message.Content;
        }

        return new StringContent(
            JsonSerializer.Serialize(
                new PlexError(message.ReasonPhrase ?? "Unknown Reason")
                {
                    Code = (int)message.StatusCode,
                    Status = (int)message.StatusCode,
                },
                DefaultJsonSerializerOptions.ConfigStandard
            ),
            Encoding.UTF8,
            "application/json"
        );
    }

    public void Dispose()
    {
        _defaultClient.Dispose();
    }
}
