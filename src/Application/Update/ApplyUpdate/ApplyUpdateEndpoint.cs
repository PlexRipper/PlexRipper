namespace Reaparr.Application;

/// <summary>
/// Applies a downloaded desktop update and restarts the application.
/// </summary>
public class ApplyUpdateEndpoint : BaseEndpointWithoutRequest
{
    private readonly IAppBuildInfo _appBuildInfo;
    private readonly UpdateManager _velopackManager;
    private readonly ILogger _log;

    public override string EndpointPath => ApiRoutes.UpdateController + "/execute";

    public ApplyUpdateEndpoint(ILogger log, IAppBuildInfo appBuildInfo, UpdateManager velopackManager)
    {
        _log = log.ForContext<ApplyUpdateEndpoint>();
        _appBuildInfo = appBuildInfo;
        _velopackManager = velopackManager;
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
            _log.Here().Debug("Skipping update apply — not running in desktop mode");
            await SendFluentResult(Result.Fail("Desktop updates are not supported in the current runtime mode"), ct);
            return;
        }

        var asset = _velopackManager.UpdatePendingRestart;
        if (asset is null)
        {
            await SendFluentResult(Result.Fail("No update staged"), ct);
            return;
        }

        HttpContext.Response.OnCompleted(() =>
        {
            _velopackManager.ApplyUpdatesAndRestart(asset, []);
            return Task.CompletedTask;
        });

        await SendFluentResult(Result.Ok(), ct);
    }
}
