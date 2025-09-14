using FastEndpoints;

namespace Reaparr.PublicAPI;

public class LoginEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/auth/login");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // generate fake session token
        var sid = Guid.NewGuid().ToString("N");

        // TODO Add real identity server authentication here
        HttpContext.Response.Headers.Append("Set-Cookie", $"SID={sid}; Path=/; HttpOnly");
       
        await Send.StringAsync("Ok.", cancellation: ct);
    }
}