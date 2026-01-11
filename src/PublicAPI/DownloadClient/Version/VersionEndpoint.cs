using FastEndpoints;

namespace Reaparr.PublicAPI;

public class VersionEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/app/version");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("v5.1.0", cancellation: ct);
    }
}
