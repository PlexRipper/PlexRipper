using FastEndpoints;

namespace Reaparr.PublicAPI;

public class WebApiVersionEndpoint : EndpointWithoutRequest<string>
{
    private readonly ILogger _log;

    public WebApiVersionEndpoint(ILogger logger)
    {
        _log = logger.ForContext<WebApiVersionEndpoint>();
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/app/webapiVersion");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        await Send.StringAsync("2.11.4", cancellation: ct);
    }
}
