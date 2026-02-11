using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public class ClearSonarrConfigurationEndpoint : BaseEndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly ISonarrSettings _sonarrSettings;

    public override string EndpointPath => ApiRoutes.IntegrationController + "/Sonarr/Configuration";

    public ClearSonarrConfigurationEndpoint(ILogger log, ISonarrSettings sonarrSettings)
    {
        _log = log.ForContext<ClearSonarrConfigurationEndpoint>();
        _sonarrSettings = sonarrSettings;
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

        _sonarrSettings.Reset();

        await SendFluentResult(Result.Ok(), ct);
    }
}
