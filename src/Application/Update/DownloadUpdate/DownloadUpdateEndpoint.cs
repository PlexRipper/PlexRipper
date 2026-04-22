namespace Reaparr.Application;

/// <summary>
/// Downloads the latest desktop update package when an update is available.
/// </summary>
public class DownloadUpdateEndpoint : BaseEndpointWithoutRequest
{
    private readonly UpdateManager _velopackManager;
    private readonly IProgressHubService _progressHub;
    private readonly ILogger _log;

    public override string EndpointPath => ApiRoutes.UpdateController + "/DownloadUpdate";

    public DownloadUpdateEndpoint(ILogger log, UpdateManager velopackManager, IProgressHubService progressHub)
    {
        _log = log.ForContext<DownloadUpdateEndpoint>();
        _velopackManager = velopackManager;
        _progressHub = progressHub;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        if (!EnvironmentExtensions.IsDesktopMode())
        {
            _log.Here().Debug("Skipping update download — not running in desktop mode");
            await SendFluentResult(Result.Fail("Desktop updates are not supported in the current runtime mode"), ct);
            return;
        }

        var result = await Result.Try(async Task () =>
        {
            // Single network call: fetch latest update info, then download it.
            var updateInfo = await _velopackManager.CheckForUpdatesAsync();
            if (updateInfo is null)
                return;

            await _velopackManager.DownloadUpdatesAsync(
                updateInfo,
                progress =>
                {
                    var dto = new AppUpdateDownloadProgressDTO(progress);
                    _ = _progressHub.SendAppUpdateDownloadProgressAsync(dto, ct);
                },
                ct
            );
        });

        await SendFluentResult(result, ct);
    }
}
