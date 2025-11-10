using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record ConfigureSonarrIntegrationRequest
{
    public required string Url { get; init; }

    public required string ApiKey { get; init; }
}

public class ConfigureSonarrIntegrationEndpoint : BaseEndpoint<ConfigureSonarrIntegrationRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISonarrSettings _sonarrSettings;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Sonarr/Configure";

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
        Post(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ConfigureSonarrIntegrationRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        // derive Reaparr base URL from the current request
        var reqScheme = HttpContext.Request.Scheme;
        var reqHost = HttpContext.Request.Host.Value;
        var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : string.Empty;
        var reaparrBase = $"{reqScheme}://{reqHost}{pathBase}".TrimEnd('/');

        if (!Uri.TryCreate(reaparrBase, UriKind.Absolute, out var reaparrBaseUri))
        {
            await SendFluentResult(Result.Fail("Could not derive Reaparr base URL from request.").LogError(), ct);
            return;
        }

        // Upsert download client
        var setupDownloadClient = await _commandExecutor.Send(new SetupSonarrDownloadClientCommand(reaparrBaseUri), ct);
        if (!setupDownloadClient.IsSuccess)
        {
            _sonarrSettings.IsConfigured = false;
            await SendFluentResult(setupDownloadClient.ToResult(), ct);
            return;
        }

        // Upsert indexer, linking to the client
        var setupIndexerClient = await _commandExecutor.Send(
            new SetupSonarrIndexerCommand
            {
                ReaparrBaseUri = reaparrBaseUri,
                DownloadClientId = setupDownloadClient.Value.DownloadClientId,
            },
            ct
        );
        if (!setupIndexerClient.IsSuccess)
        {
            _sonarrSettings.IsConfigured = false;
            await SendFluentResult(setupIndexerClient.ToResult(), ct);
            return;
        }

        _sonarrSettings.IsConfigured = true;
        await SendFluentResult(Result.Ok(), ct);
    }
}
