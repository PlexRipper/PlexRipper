namespace Reaparr.Application;

/// <summary>
/// Downloads the latest desktop update package when an update is available.
/// </summary>
public class DownloadUpdateEndpoint : BaseEndpointWithoutRequest
{
    private readonly IUpdateManager _updateManager;
    private readonly ILogger _log;

    public override string EndpointPath => ApiRoutes.UpdateController + "/download";

    public DownloadUpdateEndpoint(ILogger log, IUpdateManager updateManager)
    {
        _log = log.ForContext<DownloadUpdateEndpoint>();
        _updateManager = updateManager;
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

        await _updateManager.DownloadUpdateAsync(ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}
