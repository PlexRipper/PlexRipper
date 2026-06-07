namespace Reaparr.Application;

public record StopDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StopDownloadTaskEndpointRequestValidator : Validator<StopDownloadTaskEndpointRequest>
{
    public StopDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskEndpoint : Endpoint<StopDownloadTaskEndpointRequest, BaseResultDTO>
{
    private readonly ICommandExecutor _commandExecutor;

    public StopDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(ApiRoutes.DownloadController + "/stop/{DownloadTaskGuid}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(StopDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await Send.FluentResult(stopResult, ct);
    }
}
