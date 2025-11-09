using FastEndpoints;
using Reaparr.Settings.Contracts;

namespace Reaparr.PublicAPI;

public class LoginEndpoint : EndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly IIntegrationsSettings _integrations;
    private readonly IDownloadClientSessionManager _downloadClientSessionManager;

	public LoginEndpoint(ILogger logger, IDownloadClientSessionManager downloadClientSessionManager, IIntegrationsSettings integrations)
    {
        _log = logger.ForContext<LoginEndpoint>();
        _integrations = integrations;
        _downloadClientSessionManager = downloadClientSessionManager;
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
		var session = _downloadClientSessionManager.CreateSession(username);

		// Set SID cookie (qBittorrent compatible)
		HttpContext.Response.Headers.Append("Set-Cookie", $"SID={session.Sid}; Path=/; HttpOnly");

		await Send.StringAsync("Ok.", cancellation: ct);
	}
}