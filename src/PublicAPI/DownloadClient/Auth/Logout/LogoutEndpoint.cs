using FastEndpoints;

namespace Reaparr.PublicAPI;

public class LogoutEndpoint : EndpointWithoutRequest
{
    private readonly IDownloadClientSessionManager _downloadClientSessionManager;
    private readonly ILogger _log;

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/auth/logout");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public LogoutEndpoint(ILogger logger, IDownloadClientSessionManager downloadClientSessionManager)
    {
        _log = logger.ForContext<LoginEndpoint>();
        _downloadClientSessionManager = downloadClientSessionManager;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.DebugApiCall(HttpContext);

        if (HttpContext.Request.Cookies.TryGetValue("SID", out var sid))
            _downloadClientSessionManager.Remove(sid);

        // Clear cookie by setting expired SID
        HttpContext.Response.Headers.Append("Set-Cookie",
            "SID=; Path=/; HttpOnly; Expires=Thu, 01 Jan 1970 00:00:00 GMT");

        await Send.StringAsync("Ok.", cancellation: ct);
    }
}