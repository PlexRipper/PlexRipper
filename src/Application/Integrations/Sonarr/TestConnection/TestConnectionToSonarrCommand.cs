namespace Reaparr.Application;

public record TestConnectionToSonarrCommand(Guid? IntegrationId, string? Url, string? ApiKey)
    : ICommand<Result<TestConnectionResult>>;

public class TestConnectionToSonarrCommandValidator : AbstractValidator<TestConnectionToSonarrCommand>
{
    public TestConnectionToSonarrCommandValidator()
    {
        RuleFor(x => x)
            .Must(x =>
            {
                var hasUrl = !string.IsNullOrWhiteSpace(x.Url);
                var hasApiKey = !string.IsNullOrWhiteSpace(x.ApiKey);
                return x.IntegrationId.HasValue ? hasUrl == hasApiKey : hasUrl && hasApiKey;
            })
            .WithMessage("Provide either an integration ID or a URL and API key.");
        RuleFor(x => x.IntegrationId).NotEmpty().When(x => x.IntegrationId.HasValue);
        RuleFor(x => x.Url)
            .Must(url => url is not null && url.TrimEnd('/').IsValidHttpUrl())
            .WithMessage("Provided Sonarr URL must be a valid http/https URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));
    }
}

public class TestConnectionToSonarrCommandHandler
    : ICommandHandler<TestConnectionToSonarrCommand, Result<TestConnectionResult>>
{
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ISonarrHttpClientFactory _sonarrHttpClientFactory;

    public TestConnectionToSonarrCommandHandler(
        IReaparrDbContextFactory dbContextFactory,
        ISonarrHttpClientFactory sonarrHttpClientFactory
    )
    {
        _dbContextFactory = dbContextFactory;
        _sonarrHttpClientFactory = sonarrHttpClientFactory;
    }

    public async Task<Result<TestConnectionResult>> ExecuteAsync(
        TestConnectionToSonarrCommand command,
        CancellationToken ct
    )
    {
        if (!command.IntegrationId.HasValue)
            return await TestAsync(command.Url, command.ApiKey, ct);

        using var dbContext = await _dbContextFactory.CreateAsync();
        var integration = await dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == command.IntegrationId.Value, ct);
        if (integration is null)
            return ResultExtensions.EntityNotFound(nameof(SonarrIntegration), command.IntegrationId.Value);

        var useStoredCredentials = string.IsNullOrWhiteSpace(command.Url) && string.IsNullOrWhiteSpace(command.ApiKey);
        var result = useStoredCredentials
            ? await TestAsync(integration.BaseUrl, integration.SonarrApiKey, ct)
            : await TestAsync(command.Url, command.ApiKey, ct);
        if (result.IsFailed)
            return result.LogIfFailed();

        if (!useStoredCredentials)
            return result;

        integration.LastConnectionTestStatus = result.Value.Status;
        integration.LastConnectionTestHttpStatusCode = result.Value.HttpStatusCode;
        integration.LastConnectionTestErrorMessage = result.Value.ErrorMessage;
        integration.LastConnectionTestedAt = result.Value.TestedAt;
        await dbContext.SaveChangesAsync(ct);
        return result;
    }

    private async Task<Result<TestConnectionResult>> TestAsync(string? url, string? apiKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.TrimEnd('/').IsValidHttpUrl())
            return Result.Ok(CreateResult(TestConnectionStatus.UrlIsInvalid, null, "URL is invalid."));
        if (string.IsNullOrWhiteSpace(apiKey))
            return Result.Ok(CreateResult(TestConnectionStatus.InvalidApiKey, null, "API key is invalid."));

        var clientResult = _sonarrHttpClientFactory.Create(url, apiKey);
        if (clientResult.IsFailed)
            return clientResult.ToResult<TestConnectionResult>().LogIfFailed();

        using var client = clientResult.Value;
        return await client.TestSonarrConnectionAsync(ct);
    }

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
