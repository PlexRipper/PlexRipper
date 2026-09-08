namespace Reaparr.Application;

public record SetupSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
}

public class SetupSonarrIntegrationEndpoint : Endpoint<SetupSonarrIntegrationRequest, ResultDTO<SonarrIntegrationDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public SetupSonarrIntegrationEndpoint(ICommandExecutor commandExecutor) => _commandExecutor = commandExecutor;

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}/Setup");
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SonarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetupSonarrIntegrationRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new SetupSonarrIntegrationCommand(req.IntegrationId), ct);
        await Send.FluentResult(result, model => model.ToDTO(), ct);
    }
}
