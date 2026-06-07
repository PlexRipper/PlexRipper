namespace Reaparr.Application;

public record CheckAllConnectionsStatusByPlexServerRequest(int PlexServerId);

public class CheckAllConnectionsStatusByPlexServerRequestValidator
    : Validator<CheckAllConnectionsStatusByPlexServerRequest>
{
    public CheckAllConnectionsStatusByPlexServerRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerEndpoint
    : BaseEndpoint<CheckAllConnectionsStatusByPlexServerRequest, List<PlexServerStatusDTO>>
{
    private readonly ICommandExecutor _commandExecutor;

    public CheckAllConnectionsStatusByPlexServerEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexServerConnectionController + "/check/by-server/{PlexServerId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerStatusDTO>>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CheckAllConnectionsStatusByPlexServerRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(
            new CheckAllConnectionsStatusByPlexServerCommand(req.PlexServerId),
            ct
        );
        if (result.IsFailed)
            await Send.FluentResult(result.ToResult(), ct);
        else
            await Send.FluentResult(result, x => x.ToDTO(), ct);
    }
}
