using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public record TestConnectionToSonarrEndpointRequest
{
    [QueryParam, BindFrom("url")]
    public required string Url { get; init; }

    [QueryParam, BindFrom("apiKey")]
    public required string ApiKey { get; init; }
}

public record TestConnectionToSonarrEndpointResponse
{
    [SetsRequiredMembers]
    public TestConnectionToSonarrEndpointResponse(TestConnectionStatus result)
    {
        Result = result;
    }

    public required TestConnectionStatus Result { get; init; }
}

public class TestConnectionToSonarrEndpointRequestValidator : Validator<TestConnectionToSonarrEndpointRequest>
{
    public TestConnectionToSonarrEndpointRequestValidator()
    {
        RuleFor(x => x.Url).NotEmpty().WithMessage("URL cannot be empty.");

        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API Key cannot be empty.");
    }
}

public class TestConnectionToSonarrEndpoint
    : BaseEndpoint<TestConnectionToSonarrEndpointRequest, TestConnectionToSonarrEndpointResponse>
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Sonarr/TestConnection";

    public TestConnectionToSonarrEndpoint(ILogger log, IHttpClientFactory httpClientFactory)
    {
        _log = log.ForContext<TestConnectionToSonarrEndpoint>();
        _httpClientFactory = httpClientFactory;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<TestConnectionToSonarrEndpointResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(TestConnectionToSonarrEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        var baseUrl = req.Url.TrimEnd('/');

        if (
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
        )
        {
            _log.Here().Warning("Provided Sonarr URL is invalid: {Url}", req.Url);
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
            httpResponse = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (TaskCanceledException e)
        {
            _log.Here().Error(e, "HTTP request to Sonarr instance failed.");
            await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
            return;
        }
        catch (HttpRequestException e)
        {
            _log.Here().Error(e, "HTTP request to Sonarr instance failed.");
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
		var responseBody = await httpResponse.Content.ReadAsStringAsync(ct);
		if (!string.IsNullOrWhiteSpace(responseBody))
		{
			var truncatedBody = responseBody.Length > 512 ? responseBody.Substring(0, 512) + "..." : responseBody;
			reason = $"{reason} | Body: {truncatedBody}";
		}
		await SendFluentResult(Result.Fail(reason).LogError(), ct);
    }

    private async Task SendTestResult(TestConnectionStatus status, CancellationToken ct)
    {
        await SendFluentResult(Result.Ok(new TestConnectionToSonarrEndpointResponse(status)), ct);
    }
}
