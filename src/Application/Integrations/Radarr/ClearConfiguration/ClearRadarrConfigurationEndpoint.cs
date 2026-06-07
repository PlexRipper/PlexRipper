namespace Reaparr.Application;

public class ClearRadarrConfigurationEndpoint : BaseEndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly IRadarrSettings _radarrSettings;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Radarr/Configuration";

    public ClearRadarrConfigurationEndpoint(ILogger log, IRadarrSettings radarrSettings)
    {
        _log = log.ForContext<ClearRadarrConfigurationEndpoint>();
        _radarrSettings = radarrSettings;
    }

    public override void Configure()
    {
        Delete(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        _radarrSettings.Reset();

        await Send.FluentResult(Result.Ok(), ct);
    }
}
