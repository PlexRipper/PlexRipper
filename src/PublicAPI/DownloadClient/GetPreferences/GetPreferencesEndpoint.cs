using FastEndpoints;
using Reaparr.Data.Contracts;

namespace Reaparr.PublicAPI.GetPreferences;

public class GetPreferencesEndpoint : EndpointWithoutRequest<object>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetPreferencesEndpoint(ILogger logger,IReaparrDbContext dbContext)
    {
        _log = logger.ForContext<GetPreferencesEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(PublicApiRoutes.DownloadClient + "/app/preferences");
        Description(x => x.IsDownloadClient());
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<EmptyRequest>>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        var downloadFolder = await _dbContext.GetDownloadFolder();
        
        // Normally qBittorrent returns a big JSON with settings
        // You only need a minimal subset that Sonarr expects
        var prefs = new
        {
            // must exist, Sonarr checks these
            save_path = downloadFolder.DirectoryPath,
            temp_path_enabled = false,
        };

        await Send.OkAsync(prefs, cancellation: ct);
    }
}