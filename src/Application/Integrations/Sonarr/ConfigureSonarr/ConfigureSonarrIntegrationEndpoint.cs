using Reaparr.Application.Contracts;

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

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Sonarr/Configure";

    public ConfigureSonarrIntegrationEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<ConfigureSonarrIntegrationEndpoint>();
        _commandExecutor = commandExecutor;
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
        var upsertClientResult = await _commandExecutor.Send(new SetupSonarrDownloadClientCommand(reaparrBaseUri), ct);
        if (!upsertClientResult.IsSuccess)
        {
            await SendFluentResult(upsertClientResult.ToResult(), ct);
            return;
        }

        // Upsert indexer, linking to the client
        var upsertIndexerResult = await _commandExecutor.Send(
            new SetupSonarrIndexerCommand
            {
                ReaparrBaseUri = reaparrBaseUri,
                DownloadClientId = upsertClientResult.Value.DownloadClientId,
            },
            ct
        );
        if (!upsertIndexerResult.IsSuccess)
        {
            await SendFluentResult(upsertIndexerResult.ToResult(), ct);
            return;
        }

        await SendFluentResult(Result.Ok(), ct);
    }
}
