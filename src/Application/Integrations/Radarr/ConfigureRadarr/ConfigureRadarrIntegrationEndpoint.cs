using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record ConfigureRadarrIntegrationRequest
{
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
}

public class ConfigureRadarrIntegrationRequestValidator : Validator<ConfigureRadarrIntegrationRequest>
{
    public ConfigureRadarrIntegrationRequestValidator()
    {
        RuleFor(x => x.Url).NotEmpty().WithMessage("URL cannot be empty.");
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("API Key cannot be empty.");
    }
}

public class ConfigureRadarrIntegrationEndpoint : BaseEndpoint<ConfigureRadarrIntegrationRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IRadarrSettings _radarrSettings;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Radarr/Configure";

    public ConfigureRadarrIntegrationEndpoint(
        ILogger log,
        ICommandExecutor commandExecutor,
        IRadarrSettings radarrSettings
    )
    {
        _log = log.ForContext<ConfigureRadarrIntegrationEndpoint>();
        _commandExecutor = commandExecutor;
        _radarrSettings = radarrSettings;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ConfigureRadarrIntegrationRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        // derive Reaparr base URL from the current request
        var reqScheme = HttpContext.Request.Scheme;
        var reqHost = HttpContext.Request.Host.Value;
        var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : string.Empty;
        var reaparrBase = $"{reqScheme}://{reqHost}{pathBase}".TrimEnd('/');

        if (!Uri.TryCreate(reaparrBase, UriKind.Absolute, out var reaparrBaseUri))
        {
            await SendFluentResult(ResultExtensions.Create400BadRequestResult("Could not derive Reaparr base URL from request.").LogError(), ct);
            return;
        }
        
        _radarrSettings.RadarrBaseUrl =  req.Url.TrimEnd('/');
        _radarrSettings.RadarrApiKey = req.ApiKey;

        // Upsert download client
        var setupDownloadClient = await _commandExecutor.Send(new SetupRadarrDownloadClientCommand(reaparrBaseUri), ct);
        if (!setupDownloadClient.IsSuccess)
        {
            _radarrSettings.IsConfigured = false;
            await SendFluentResult(setupDownloadClient.ToResult(), ct);
            return;
        }

        // Upsert indexer, linking to the client
        var setupIndexerClient = await _commandExecutor.Send(
            new SetupRadarrIndexerCommand
            {
                ReaparrBaseUri = reaparrBaseUri,
                DownloadClientId = setupDownloadClient.Value.DownloadClientId,
            },
            ct
        );
        if (!setupIndexerClient.IsSuccess)
        {
            _radarrSettings.IsConfigured = false;
            await SendFluentResult(setupIndexerClient.ToResult(), ct);
            return;
        }

        _radarrSettings.IsConfigured = true;
        await SendFluentResult(Result.Ok(), ct);
    }
}
