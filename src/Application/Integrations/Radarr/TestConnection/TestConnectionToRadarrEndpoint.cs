using System.Net.Http.Headers;

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
        RuleFor(x => x.Url).NotEmpty().WithMessage("Provided Radarr URL cannot be empty.");
        RuleFor(x => x.Url).Must(BeValidUrl).WithMessage("Provided Radarr URL must be a valid http/https URL.");
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("Provided Radarr API Key cannot be empty.");
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var trimmed = url.TrimEnd('/');

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class TestConnectionToRadarrEndpoint
    : Endpoint<TestConnectionToRadarrEndpointRequest, TestConnectionToRadarrEndpointResponse>
{
    private readonly ILogger _log;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public TestConnectionToRadarrEndpoint(ILogger log, IRadarrHttpClientFactory radarrHttpClientFactory)
    {
        _log = log.ForContext<TestConnectionToRadarrEndpoint>();
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public override void Configure()
    {
        Get(ApiRoutes.IntegrationController + "/Radarr/TestConnection");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<TestConnectionToRadarrEndpointResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(TestConnectionToRadarrEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var clientResult = _radarrHttpClientFactory.Create(req.Url, req.ApiKey);
        if (clientResult.IsFailed)
        {
            await Send.FluentResult(clientResult.ToResult<TestConnectionToRadarrEndpointResponse>(), ct);
            return;
        }

        using var client = clientResult.Value;
        using var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "api/v3/system/status");

        var result = await Result.Try(async Task () =>
        {
            using var httpResponse = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
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
        });

        if (result.IsCancelled)
        {
            _log.Here().Error("HTTP request to Radarr instance was cancelled.");
            await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
            return;
        }

        if (result.IsFailed)
        {
            _log.Here().Error("HTTP request to Radarr instance failed, could be offline");
            await SendTestResult(TestConnectionStatus.ConnectionFailed, ct);
        }
    }

    private async Task SendTestResult(TestConnectionStatus status, CancellationToken ct)
    {
        await Send.FluentResult(Result.Ok(new TestConnectionToRadarrEndpointResponse(status)), ct);
    }
}
