using System.Net;
using System.Text.Json;
using HttpClientToCurl.Extensions;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;
using Serilog.Events;

namespace Reaparr.PlexApi;

public class PlexApiClient : IPlexApiClient
{
    private readonly ILogger _log;
    private readonly HttpClient _defaultClient;
    private readonly PlexApiClientOptions _options;

    public PlexApiClient(ILogger log, HttpClient httpClient, PlexApiClientOptions options)
    {
        _log = log.ForContext<PlexApiClient>();
        _defaultClient = httpClient;
        _defaultClient.DefaultRequestHeaders.Accept.Add(ContentType.ApplicationJsonHeaderValue);

        _options = options;

        if (_options.ConnectionUrl != string.Empty)
            _defaultClient.BaseAddress = new Uri(_options.ConnectionUrl);

        _defaultClient.Timeout = TimeSpan.FromSeconds(_options.Timeout);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        request.Headers.Remove("user-agent");
        request.SetRetryCount(_options.RetryCount);
        request.SetRetryProgressCallback(_options.RetryProgressAction);

        if (_log.Here().IsLogLevelVerbose())
        {
            var curl = _defaultClient.GenerateCurlInString(request);
            _log.Here().Verbose("Request CURL: {RequestUrl}", curl);
        }

        HttpResponseMessage response;

        try
        {
            response = await _defaultClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
        catch (TaskCanceledException)
        {
            return CreateErrorResponse(request, HttpStatusCode.RequestTimeout, "Request Timeout");
        }
        catch (HttpRequestException)
        {
            return CreateErrorResponse(request, HttpStatusCode.BadGateway, "Bad Gateway");
        }

        if (!response.IsSuccessStatusCode)
        {
            var originalContent = response.Content;
            var replacementContent = ToJsonResponse(response);

            if (!ReferenceEquals(replacementContent, originalContent))
            {
                originalContent?.Dispose();
                response.Content = replacementContent;
            }
        }

        if (_log.Here().IsLogLevelEnabled(LogEventLevel.Verbose))
            _log.Here().Verbose("Response: {Response}", await response.Content.ReadAsFormattedJsonAsync());

        return response;
    }

    public async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy,
        };

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        return clone;
    }

    private HttpContent ToJsonResponse(HttpResponseMessage message)
    {
        if (message.Content.Headers.ContentType?.MediaType != ContentType.TextHtml)
            return message.Content;

        return JsonSerializer
            .Serialize(
                new PlexError(message.ReasonPhrase ?? "Unknown Reason")
                {
                    Code = (int)message.StatusCode,
                    Status = (int)message.StatusCode,
                },
                DefaultJsonSerializerOptions.ConfigStandard
            )
            .ToStringContent();
    }

    private HttpResponseMessage CreateErrorResponse(
        HttpRequestMessage request,
        HttpStatusCode statusCode,
        string reasonPhrase
    ) =>
        new(statusCode)
        {
            RequestMessage = request,
            ReasonPhrase = reasonPhrase,
            Content = JsonSerializer
                .Serialize(
                    new PlexError(reasonPhrase) { Code = (int)statusCode, Status = (int)statusCode },
                    DefaultJsonSerializerOptions.ConfigStandard
                )
                .ToStringContent(),
        };

    public void Dispose()
    {
        _defaultClient.Dispose();
    }
}
