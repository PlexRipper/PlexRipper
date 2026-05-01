namespace Reaparr.Application;

/// <summary>
/// Downloads the latest desktop update package when an update is available.
/// </summary>
public class DownloadUpdateEndpoint : BaseEndpointWithoutRequest
{
    private readonly UpdateManager _velopackManager;
    private readonly IAppBuildInfo _appBuildInfo;
    private readonly IProgressHubService _progressHub;
    private readonly ILogger _log;

    public override string EndpointPath => ApiRoutes.UpdateController + "/DownloadUpdate";

    public DownloadUpdateEndpoint(
        ILogger log,
        IAppBuildInfo appBuildInfo,
        UpdateManager velopackManager,
        IProgressHubService progressHub
    )
    {
        _log = log.ForContext<DownloadUpdateEndpoint>();
        _appBuildInfo = appBuildInfo;
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

        if (!_appBuildInfo.IsDesktopMode)
        {
            _log.Here().Debug("Skipping update download — not running in desktop mode");
            await SendFluentResult(Result.Fail("Desktop updates are not supported in the current runtime mode"), ct);
            return;
        }

        var result = await Result.Try(async Task () =>
        {
            _log.Here().Information("Checking for Velopack update before download");

            // Single network call: fetch latest update info, then download it.
            var updateInfo = await _velopackManager.CheckForUpdatesAsync();
            if (updateInfo is null)
            {
                _log.Here().Information("No Velopack update available to download");
                return;
            }

            _log.Here().Information("Downloading Velopack update {Version}", updateInfo.TargetFullRelease.Version);

            await _velopackManager.DownloadUpdatesAsync(
                updateInfo,
                progress =>
                {
                    var dto = new AppUpdateDownloadProgressDTO(progress);
                    _ = _progressHub.SendAppUpdateDownloadProgressAsync(dto, ct);
                },
                ct
            );

            _log.Here()
                .Information(
                    "Downloaded Velopack update {Version}; PendingRestart: {HasPendingRestart}",
                    updateInfo.TargetFullRelease.Version,
                    _velopackManager.UpdatePendingRestart is not null
                );
        });

        await SendFluentResult(result, ct);
    }
}
