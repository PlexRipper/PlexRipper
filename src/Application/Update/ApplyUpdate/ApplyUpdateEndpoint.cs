namespace Reaparr.Application;

/// <summary>
/// Applies a downloaded desktop update and restarts the application.
/// </summary>
public class ApplyUpdateEndpoint : BaseEndpointWithoutRequest
{
    private readonly ILogger _log;
    private readonly IUpdateManager _updateManager;

    public override string EndpointPath => ApiRoutes.UpdateController + "/execute";

    public ApplyUpdateEndpoint(ILogger log, IUpdateManager updateManager)
    {
        _log = log.ForContext<ApplyUpdateEndpoint>();
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
            _log.Here().Debug("Skipping update apply — not running in desktop mode");
            await SendFluentResult(Result.Fail("Desktop updates are not supported in the current runtime mode"), ct);
            return;
        }

        await SendFluentResult(Result.Ok(), ct);

        _updateManager.ApplyUpdateAndRestart();
    }
}
