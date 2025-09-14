using FastEndpoints;

namespace Reaparr.PublicAPI;

public class WebApiVersionEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/app/webapiVersion");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("2.8.3", cancellation: ct);
    }
}