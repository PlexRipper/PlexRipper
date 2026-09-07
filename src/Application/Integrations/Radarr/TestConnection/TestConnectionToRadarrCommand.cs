namespace Reaparr.Application;

public record TestConnectionToRadarrCommand(Guid? IntegrationId, string? Url, string? ApiKey)
    : ICommand<Result<TestConnectionResult>>;

public class TestConnectionToRadarrCommandValidator : AbstractValidator<TestConnectionToRadarrCommand>
{
    public TestConnectionToRadarrCommandValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                x.IntegrationId.HasValue
                    ? string.IsNullOrWhiteSpace(x.Url) && string.IsNullOrWhiteSpace(x.ApiKey)
                    : !string.IsNullOrWhiteSpace(x.Url) && !string.IsNullOrWhiteSpace(x.ApiKey)
            )
            .WithMessage("Provide either an integration ID or a URL and API key.");
        RuleFor(x => x.IntegrationId).NotEmpty().When(x => x.IntegrationId.HasValue);
        RuleFor(x => x.Url)
            .Must(BeValidUrl)
            .WithMessage("Provided Radarr URL must be a valid http/https URL.")
            .When(x => !x.IntegrationId.HasValue && !string.IsNullOrWhiteSpace(x.Url));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url?.TrimEnd('/'), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public class TestConnectionToRadarrCommandHandler
    : ICommandHandler<TestConnectionToRadarrCommand, Result<TestConnectionResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IRadarrHttpClientFactory _radarrHttpClientFactory;

    public TestConnectionToRadarrCommandHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IRadarrHttpClientFactory radarrHttpClientFactory
    )
    {
        _log = log.ForContext<TestConnectionToRadarrCommandHandler>();
        _dbContextFactory = dbContextFactory;
        _radarrHttpClientFactory = radarrHttpClientFactory;
    }

    public async Task<Result<TestConnectionResult>> ExecuteAsync(
        TestConnectionToRadarrCommand command,
        CancellationToken ct
    )
    {
        if (!command.IntegrationId.HasValue)
            return await TestAsync(command.Url!, command.ApiKey!, ct);

        using var dbContext = await _dbContextFactory.CreateAsync();
        var integration = await dbContext
            .RadarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == command.IntegrationId.Value, ct);
        if (integration is null)
            return ResultExtensions.EntityNotFound(nameof(RadarrIntegration), command.IntegrationId.Value);

        var result = await TestAsync(integration.BaseUrl, integration.RadarrApiKey, ct);
        if (result.IsCancelled)
            return result.LogWarning();
        if (result.IsFailed)
            return result.LogError();

        integration.LastConnectionTestStatus = result.Value.Status;
        integration.LastConnectionTestHttpStatusCode = result.Value.HttpStatusCode;
        integration.LastConnectionTestErrorMessage = result.Value.ErrorMessage;
        integration.LastConnectionTestedAt = result.Value.TestedAt;
        await dbContext.SaveChangesAsync(ct);
        return result;
    }

    private async Task<Result<TestConnectionResult>> TestAsync(string url, string apiKey, CancellationToken ct)
    {
        if (!Uri.TryCreate(url.TrimEnd('/'), UriKind.Absolute, out var uri) || !IsHttp(uri))
            return Result.Ok(CreateResult(TestConnectionStatus.UrlIsInvalid, null, "URL is invalid."));
        if (string.IsNullOrWhiteSpace(apiKey))
            return Result.Ok(CreateResult(TestConnectionStatus.InvalidApiKey, null, "API key is invalid."));

        var clientResult = _radarrHttpClientFactory.Create(url, apiKey);
        if (clientResult.IsCancelled)
            return clientResult.ToResult<TestConnectionResult>().LogWarning();
        if (clientResult.IsFailed)
            return clientResult.ToResult<TestConnectionResult>().LogError();

        using var client = clientResult.Value;
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v3/system/status");
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            var statusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
                return Result.Ok(CreateResult(TestConnectionStatus.Success, statusCode, null));

            var status = statusCode == StatusCodes.Status401Unauthorized
                ? TestConnectionStatus.InvalidApiKey
                : TestConnectionStatus.ConnectionFailed;
            var error = response.ReasonPhrase ?? $"HTTP {statusCode}";
            _log.Here().Warning("Radarr connection test failed: {Reason}", error);
            return Result.Ok(CreateResult(status, statusCode, error));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ResultExtensions.TaskIsCancelled(nameof(TestConnectionToRadarrCommand))
                .ToResult<TestConnectionResult>()
                .LogWarning();
        }
        catch (TaskCanceledException exception)
        {
            _log.Here().Warning(exception, "Radarr connection test timed out");
            return Result.Ok(CreateResult(TestConnectionStatus.ConnectionFailed, null, "Connection timed out."));
        }
        catch (HttpRequestException exception)
        {
            _log.Here().Warning(exception, "Radarr connection test failed");
            return Result.Ok(CreateResult(TestConnectionStatus.ConnectionFailed, null, "Connection failed."));
        }
    }

    private static bool IsHttp(Uri uri) => uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;

    private static TestConnectionResult CreateResult(
        TestConnectionStatus status,
        int? httpStatusCode,
        string? errorMessage
    ) =>
        new()
        {
            Status = status,
            HttpStatusCode = httpStatusCode,
            ErrorMessage = errorMessage,
            TestedAt = DateTime.UtcNow,
        };
}
