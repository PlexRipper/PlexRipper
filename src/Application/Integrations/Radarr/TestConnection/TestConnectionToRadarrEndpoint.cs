using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public record TestConnectionToRadarrEndpointRequest
{
    [QueryParam, BindFrom("url")]
    public required string Url { get; init; }

    [QueryParam, BindFrom("apiKey")]
    public required string ApiKey { get; init; }
}

public record TestConnectionToRadarrEndpointResponse
{
    [SetsRequiredMembers]
    public TestConnectionToRadarrEndpointResponse(TestConnectionStatus result)
    {
        Result = result;
    }

    public required TestConnectionStatus Result { get; init; }
}

public class TestConnectionToRadarrEndpointRequestValidator : Validator<TestConnectionToRadarrEndpointRequest>
{
    public TestConnectionToRadarrEndpointRequestValidator()
    {
        RuleFor(x => x.Url).NotEmpty().WithMessage("URL cannot be empty.");
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API Key cannot be empty.");
    }
}

public class TestConnectionToRadarrEndpoint
    : BaseEndpoint<TestConnectionToRadarrEndpointRequest, TestConnectionToRadarrEndpointResponse>
{
    private readonly ILogger _log;
    private readonly HttpClient _client;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Radarr/TestConnection";

    public TestConnectionToRadarrEndpoint(ILogger log, IHttpClientFactory httpClientFactory)
    {
        _log = log.ForContext<TestConnectionToRadarrEndpoint>();
        _client = httpClientFactory.CreateRadarrHttpClient();
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<TestConnectionToRadarrEndpointResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(TestConnectionToRadarrEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var baseUrl = req.Url.TrimEnd('/');

        if (
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
        )
        {
            _log.Here().Warning("Provided Radarr URL is invalid: {Url}", req.Url);
            await SendTestResult(TestConnectionStatus.UrlIsInvalid, ct);
            return;
        }

        var url = $"{baseUrl}/api/v3/system/status";

        using var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("X-Api-Key", req.ApiKey);

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (TaskCanceledException e)
        {
            _log.Here().Error(e, "HTTP request to Radarr instance failed.");
            await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
            return;
        }
        catch (HttpRequestException e)
        {
            _log.Here().Error(e, "HTTP request to Radarr instance failed.");
            await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
            return;
        }

        if (httpResponse.IsSuccessStatusCode)
        {
            await SendTestResult(TestConnectionStatus.Success, ct);
            return;
        }

        var statusCode = (int)httpResponse.StatusCode;
        if (statusCode == 401)
        {
            await SendTestResult(TestConnectionStatus.InvalidApiKey, ct);
            return;
        }

        var reason = httpResponse.ReasonPhrase ?? $"HTTP {(int)httpResponse.StatusCode}";
        _log.Here().Warning("Radarr connection test failed: {Reason}", reason);
        await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
    }

    private async Task SendTestResult(TestConnectionStatus status, CancellationToken ct)
    {
        await SendFluentResult(Result.Ok(new TestConnectionToRadarrEndpointResponse(status)), ct);
    }
}
