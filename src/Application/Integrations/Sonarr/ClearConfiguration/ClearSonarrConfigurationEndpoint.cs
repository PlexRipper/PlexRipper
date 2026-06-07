namespace Reaparr.Application;

public class ClearSonarrConfigurationEndpoint : EndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly ISonarrSettings _sonarrSettings;

    public ClearSonarrConfigurationEndpoint(ILogger log, ISonarrSettings sonarrSettings)
    {
        _log = log.ForContext<ClearSonarrConfigurationEndpoint>();
        _sonarrSettings = sonarrSettings;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.IntegrationController + "/Sonarr/Configuration");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        _sonarrSettings.Reset();

        await Send.FluentResult(Result.Ok(), ct);
    }
}
