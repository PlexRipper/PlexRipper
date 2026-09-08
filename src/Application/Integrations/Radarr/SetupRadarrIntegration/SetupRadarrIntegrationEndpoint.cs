namespace Reaparr.Application;

public record SetupRadarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class SetupRadarrIntegrationEndpoint : Endpoint<SetupRadarrIntegrationRequest, ResultDTO<RadarrIntegrationDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public SetupRadarrIntegrationEndpoint(ICommandExecutor commandExecutor) => _commandExecutor = commandExecutor;

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Radarr/{integrationId:guid}/Setup");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<RadarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetupRadarrIntegrationRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new SetupRadarrIntegrationCommand(req.IntegrationId), ct);
        await Send.FluentResult(result, model => model.ToDTO(), ct);
    }
}
