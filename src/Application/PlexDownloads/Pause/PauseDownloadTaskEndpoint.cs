namespace Reaparr.Application;

/// <summary>
/// Pause a currently downloading <see cref="DownloadTaskGeneric"/>.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to pause.</param>
/// <returns>Is successful.</returns>
public record PauseDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class PauseDownloadTaskEndpointRequestValidator : Validator<PauseDownloadTaskEndpointRequest>
{
    public PauseDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class PauseDownloadTaskEndpoint : Endpoint<PauseDownloadTaskEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public PauseDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(ApiRoutes.DownloadController + "/pause/{DownloadTaskGuid}");

        Description(x =>
            x.Accepts<PauseDownloadTaskEndpointRequest>()
                .Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(PauseDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var pauseResult = await _commandExecutor.Send(new PauseDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await Send.FluentResult(pauseResult, ct);
    }
}
