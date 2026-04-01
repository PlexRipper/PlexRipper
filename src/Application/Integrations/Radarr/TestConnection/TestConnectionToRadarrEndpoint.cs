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

        var url = new Url(baseUrl).AppendPathSegments("api", "v3", "system", "status");

        using var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.ToString());
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("X-Api-Key", req.ApiKey);

        var result = await Result.Try(async Task () =>
        {
            using var httpResponse = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
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
        await SendFluentResult(Result.Ok(new TestConnectionToRadarrEndpointResponse(status)), ct);
    }
}
