namespace Reaparr.Application;

public record ConfigureSonarrIntegrationRequest
{
    public required string Url { get; init; }

    public required string ApiKey { get; init; }
}

public class ConfigureSonarrIntegrationRequestValidator : Validator<ConfigureSonarrIntegrationRequest>
{
    public ConfigureSonarrIntegrationRequestValidator()
    {
        RuleFor(x => x.Url).NotEmpty().WithMessage("URL cannot be empty.");
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API Key cannot be empty.");
    }
}

public class ConfigureSonarrIntegrationEndpoint : BaseEndpoint<ConfigureSonarrIntegrationRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISonarrSettings _sonarrSettings;

    public ConfigureSonarrIntegrationEndpoint(
        ILogger log,
        ICommandExecutor commandExecutor,
        ISonarrSettings sonarrSettings
    )
    {
        _log = log.ForContext<ConfigureSonarrIntegrationEndpoint>();
        _commandExecutor = commandExecutor;
        _sonarrSettings = sonarrSettings;
    }

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Sonarr/Configure");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ConfigureSonarrIntegrationRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        _sonarrSettings.SonarrBaseUrl = req.Url.TrimEnd('/');
        _sonarrSettings.SonarrApiKey = req.ApiKey;

        // Upsert download client
        var setupDownloadClient = await _commandExecutor.Send(new SetupSonarrDownloadClientCommand(), ct);
        if (!setupDownloadClient.IsSuccess)
        {
            _sonarrSettings.IsConfigured = false;
            await Send.FluentResult(setupDownloadClient.ToResult(), ct);
            return;
        }

        // Upsert indexer, linking to the client
        var setupIndexerClient = await _commandExecutor.Send(
            new SetupSonarrIndexerCommand { DownloadClientId = setupDownloadClient.Value.DownloadClientId },
            ct
        );
        if (!setupIndexerClient.IsSuccess)
        {
            _sonarrSettings.IsConfigured = false;
            await Send.FluentResult(setupIndexerClient.ToResult(), ct);
            return;
        }

        _sonarrSettings.IsConfigured = true;
        await Send.FluentResult(Result.Ok(), ct);
    }
}
