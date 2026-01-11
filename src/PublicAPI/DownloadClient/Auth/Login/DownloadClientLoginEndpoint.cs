using FastEndpoints;
using FluentValidation;
using Reaparr.Identity.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public record DownloadClientLoginEndpointRequest
{
    [BindFrom("username")]
    public required string Username { get; init; }

    [BindFrom("password")]
    public required string Password { get; init; }
}

public class DownloadClientLoginEndpointRequestValidator : Validator<DownloadClientLoginEndpointRequest>
{
    public DownloadClientLoginEndpointRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class DownloadClientLoginEndpoint : Endpoint<DownloadClientLoginEndpointRequest>
{
    private readonly ILogger _log;
    private readonly IAuthDbContext _authDbContext;
    private readonly IIntegrationsSettings _integrations;

    private static readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);

    public DownloadClientLoginEndpoint(ILogger logger, IAuthDbContext authDbContext, IIntegrationsSettings integrations)
    {
        _log = logger.ForContext<DownloadClientLoginEndpoint>();
        _authDbContext = authDbContext;
        _integrations = integrations;
    }

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/auth/login");
        Description(x => x.IsDownloadClient());
        AllowFormData(urlEncoded: true);
        AllowAnonymous();
    }

    public override async Task HandleAsync(DownloadClientLoginEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        var username = req.Username;
        var password = req.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Validate against IntegrationSettings
        if (
            !string.Equals(username, _integrations.DownloadClientUsername, StringComparison.Ordinal)
            || !string.Equals(password, _integrations.DownloadClientPassword, StringComparison.Ordinal)
        )
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // Create session
        var session = await CreateSession(username);

        // Set SID cookie (qBittorrent compatible)
        var isHttps = HttpContext.Request.IsHttps;
        var cookieOptions = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = session.ExpiresAt,
        };
        HttpContext.Response.Cookies.Append("SID", session.Sid, cookieOptions);

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
