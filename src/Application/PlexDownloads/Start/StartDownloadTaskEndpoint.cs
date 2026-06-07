namespace Reaparr.Application;

public record StartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StartDownloadTaskEndpointRequestValidator : Validator<StartDownloadTaskEndpointRequest>
{
    public StartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StartDownloadTaskEndpoint : BaseEndpoint<StartDownloadTaskEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/start/{DownloadTaskGuid}";

    public StartDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Accepts<StartDownloadTaskEndpointRequest>()
                .Produces(StatusCodes.Status202Accepted, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(StartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var startResult = await _commandExecutor.Send(new StartDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await Send.FluentResult(startResult, ct);
    }
}
