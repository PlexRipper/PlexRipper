namespace Reaparr.Application;

public record ResumePlexServerDownloadsEndpointRequest(int PlexServerId);

public class ResumePlexServerDownloadsEndpointRequestValidator : Validator<ResumePlexServerDownloadsEndpointRequest>
{
    public ResumePlexServerDownloadsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class ResumePlexServerDownloadsEndpoint : BaseEndpoint<ResumePlexServerDownloadsEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public ResumePlexServerDownloadsEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(ApiRoutes.PlexServerController + "/server/resume/{PlexServerId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ResumePlexServerDownloadsEndpointRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new ResumePlexServerDownloadsCommand(req.PlexServerId), ct);

        await Send.FluentResult(result, ct);
    }
}
