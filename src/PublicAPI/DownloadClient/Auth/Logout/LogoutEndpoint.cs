using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Reaparr.Identity.Contracts;

namespace Reaparr.PublicAPI;

public class LogoutEndpoint : EndpointWithoutRequest
{
    private readonly IAuthDbContext _authDbContext;
    private readonly ILogger _log;

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/auth/logout");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public LogoutEndpoint(ILogger logger, IAuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
        _log = logger.ForContext<DownloadClientLoginEndpoint>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.DebugApiCall(HttpContext);

        if (HttpContext.Request.Cookies.TryGetValue("SID", out var sid))
        {
            await _authDbContext.DownloadClientSessions.Where(x => x.Sid == sid)
                .ExecuteDeleteAsync(cancellationToken: ct);
        }

        // Clear cookie by setting expired SID
        HttpContext.Response.Headers.Append("Set-Cookie",
            "SID=; Path=/; HttpOnly; Expires=Thu, 01 Jan 1970 00:00:00 GMT");

        await Send.StringAsync("Ok.", cancellation: ct);
    }
}