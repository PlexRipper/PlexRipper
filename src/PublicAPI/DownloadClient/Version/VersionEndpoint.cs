using FastEndpoints;

namespace Reaparr.PublicAPI;

public class VersionEndpoint : EndpointWithoutRequest<string>
{
    private readonly ILogger _log;

    public VersionEndpoint(ILogger logger)
    {
        _log = logger.ForContext<VersionEndpoint>();
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/app/version");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        await Send.StringAsync("v5.1.4", cancellation: ct);
    }
}
