using FastEndpoints;
using Reaparr.Identity.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public class LoginEndpoint : EndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly IAuthDbContext _authDbContext;
    private readonly IIntegrationsSettings _integrations;

    private static readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);

    public LoginEndpoint(ILogger logger, IAuthDbContext authDbContext, IIntegrationsSettings integrations)
    {
        _log = logger.ForContext<LoginEndpoint>();
        _authDbContext = authDbContext;
        _integrations = integrations;
    }

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/auth/login");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.DebugApiCall(HttpContext);

        // Read application/x-www-form-urlencoded
        var form = await HttpContext.Request.ReadFormAsync(ct);
        var username = form["username"].ToString();
        var password = form["password"].ToString();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Validate against IntegrationSettings
        if (!string.Equals(username, _integrations.DownloadClientUsername, StringComparison.Ordinal) ||
            !string.Equals(password, _integrations.DownloadClientPassword, StringComparison.Ordinal))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Create session
        var session = await CreateSession(username);

        // Set SID cookie (qBittorrent compatible)
        HttpContext.Response.Headers.Append("Set-Cookie", $"SID={session.Sid}; Path=/; HttpOnly");

        await Send.StringAsync("Ok.", cancellation: ct);
    }

    public async Task<DownloadClientSession> CreateSession(string username)
    {
        var sid = Guid.NewGuid().ToString("N");
        var entity = new DownloadClientSession
        {
            Sid = sid,
            Username = username,
            ExpiresAt = DateTimeOffset.UtcNow.Add(_defaultTtl),
        };

        _authDbContext.DownloadClientSessions.Add(entity);
        await _authDbContext.SaveChangesAsync();

        return entity;
    }
}