namespace Reaparr.Application;

public record RestartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class RestartDownloadTaskEndpointRequestValidator : Validator<RestartDownloadTaskEndpointRequest>
{
    public RestartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    : Endpoint<RestartDownloadTaskEndpointRequest>
{
    public override void Configure()
    {
        Put(ApiRoutes.DownloadController + "/restart/{DownloadTaskGuid}");

        Description(x =>
            x.Accepts<RestartDownloadTaskEndpointRequest>()
                .Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RestartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var restartResult = await commandExecutor.Send(new RestartDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await Send.FluentResult(restartResult, ct);
    }
}
