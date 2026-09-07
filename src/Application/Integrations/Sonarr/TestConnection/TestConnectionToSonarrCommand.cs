namespace Reaparr.Application;

public record TestConnectionToSonarrCommand(Guid? IntegrationId, string? Url, string? ApiKey)
    : ICommand<Result<TestConnectionResult>>;

public class TestConnectionToSonarrCommandValidator : AbstractValidator<TestConnectionToSonarrCommand>
{
    public TestConnectionToSonarrCommandValidator()
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
            .WithMessage("Provided Sonarr URL must be a valid http/https URL.")
            .When(x => !x.IntegrationId.HasValue && !string.IsNullOrWhiteSpace(x.Url));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url?.TrimEnd('/'), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public class TestConnectionToSonarrCommandHandler
    : ICommandHandler<TestConnectionToSonarrCommand, Result<TestConnectionResult>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public TestConnectionToSonarrCommandHandler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ISonarrHttpClientFactory sonarrHttpClientFactory
    )
    {
        _log = log.ForContext<TestConnectionToSonarrCommandHandler>();
        _dbContextFactory = dbContextFactory;
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<TestConnectionResult>> ExecuteAsync(
        TestConnectionToSonarrCommand command,
        CancellationToken ct
    )
    {
        if (!command.IntegrationId.HasValue)
            return await TestAsync(command.Url!, command.ApiKey!, ct);

        using var dbContext = await _dbContextFactory.CreateAsync();
        var integration = await dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == command.IntegrationId.Value, ct);
        if (integration is null)
            return ResultExtensions.EntityNotFound(nameof(SonarrIntegration), command.IntegrationId.Value);

        var result = await TestAsync(integration.BaseUrl, integration.SonarrApiKey, ct);
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

        var clientResult = _sonarrHttpClientFactory.Create(url, apiKey);
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
            _log.Here().Warning("Sonarr connection test failed: {Reason}", error);
            return Result.Ok(CreateResult(status, statusCode, error));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ResultExtensions.TaskIsCancelled(nameof(TestConnectionToSonarrCommand))
                .ToResult<TestConnectionResult>()
                .LogWarning();
        }
        catch (TaskCanceledException exception)
        {
            _log.Here().Warning(exception, "Sonarr connection test timed out");
            return Result.Ok(CreateResult(TestConnectionStatus.ConnectionFailed, null, "Connection timed out."));
        }
        catch (HttpRequestException exception)
        {
            _log.Here().Warning(exception, "Sonarr connection test failed");
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
