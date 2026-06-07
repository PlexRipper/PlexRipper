namespace Reaparr.Application;

public record QueueInspectPlexServerJobEndpointRequest(int PlexServerId);

public class QueueInspectPlexServerJobEndpointRequestValidator : Validator<QueueInspectPlexServerJobEndpointRequest>
{
    public QueueInspectPlexServerJobEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueInspectPlexServerJobEndpoint
    : BaseEndpoint<QueueInspectPlexServerJobEndpointRequest, ResultDTO<PlexServerDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public QueueInspectPlexServerJobEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexServerController + "/{PlexServerId}/inspect");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(QueueInspectPlexServerJobEndpointRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new QueueInspectPlexServerJobCommand([req.PlexServerId]), ct);
        await Send.FluentResult(result, ct);
    }
}
