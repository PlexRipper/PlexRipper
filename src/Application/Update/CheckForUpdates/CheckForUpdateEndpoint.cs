namespace Reaparr.Application;

/// <summary>
/// Queues a one-shot <see cref="CheckForUpdateJob"/> to check for a Velopack application update.
/// </summary>
public class CheckForUpdateEndpoint : BaseEndpointWithoutRequest<AppUpdateCheckResult>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.UpdateController + "/check";

    public CheckForUpdateEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckForUpdateEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<AppUpdateCheckResult>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        var result = await _commandExecutor.Send(new CheckForUpdatesCommand(), ct);
        await SendFluentResult(result, x => x, ct);
    }
}
