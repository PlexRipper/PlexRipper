namespace Reaparr.Application;

/// <summary>
/// Sends a <see cref="CheckForUpdatesCommand"/> immediately to check for a Velopack application update.
/// </summary>
public class CheckForUpdateEndpoint : EndpointWithoutRequest<ResultDTO<AppUpdateCheckDTO>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public CheckForUpdateEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckForUpdateEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.UpdateController + "/Check");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<AppUpdateCheckDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);

        var result = await _commandExecutor.Send(new CheckForUpdatesCommand(), ct);
        await Send.FluentResult(result, x => x.ToDTO(), ct);
    }
}
