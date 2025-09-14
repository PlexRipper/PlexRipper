using FastEndpoints;

namespace Reaparr.PublicAPI.GetPreferences;

public class GetPreferencesEndpoint : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient +  "/app/preferences");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Normally qBittorrent returns a big JSON with settings
        // You only need a minimal subset that Sonarr expects
        var prefs = new
        {
            // must exist, Sonarr checks these
            save_path = "/downloads",
            temp_path_enabled = false
        };

        await Send.OkAsync(prefs, cancellation: ct);
    }
}