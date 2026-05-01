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
            _log.Here().Warning("Skipping Velopack update apply because no update is staged");
            await SendFluentResult(Result.Fail("No update staged"), ct);
            return;
        }

        _log.Here()
            .Warning(
                "Preparing to apply Velopack update and restart Reaparr; AppId: {AppId}; CurrentVersion: {CurrentVersion}; PackageId: {PackageId}; PackageVersion: {PackageVersion}",
                _velopackManager.AppId,
                _velopackManager.CurrentVersion,
                asset.PackageId,
                asset.Version
            );

        HttpContext.Response.OnCompleted(() =>
        {
            try
            {
                _log.Here()
                    .Warning(
                        "Applying Velopack update and restarting Reaparr; AppId: {AppId}; CurrentVersion: {CurrentVersion}; PackageId: {PackageId}; PackageVersion: {PackageVersion}",
                        _velopackManager.AppId,
                        _velopackManager.CurrentVersion,
                        asset.PackageId,
                        asset.Version
                    );

                _velopackManager.ApplyUpdatesAndRestart(asset, []);

                _log.Here()
                    .Warning(
                        "Velopack ApplyUpdatesAndRestart returned without terminating the current process; AppId: {AppId}; CurrentVersion: {CurrentVersion}; PackageId: {PackageId}; PackageVersion: {PackageVersion}",
                        _velopackManager.AppId,
                        _velopackManager.CurrentVersion,
                        asset.PackageId,
                        asset.Version
                    );
            }
            catch (Exception ex)
            {
                _log.Here()
                    .Error(
                        ex,
                        "Velopack ApplyUpdatesAndRestart failed; AppId: {AppId}; CurrentVersion: {CurrentVersion}; PackageId: {PackageId}; PackageVersion: {PackageVersion}",
                        _velopackManager.AppId,
                        _velopackManager.CurrentVersion,
                        asset.PackageId,
                        asset.Version
                    );
            }

            return Task.CompletedTask;
        });

        await SendFluentResult(Result.Ok(), ct);
    }
}
