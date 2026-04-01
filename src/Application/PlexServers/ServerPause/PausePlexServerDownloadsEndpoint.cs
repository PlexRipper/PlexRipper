using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public record PausePlexServerDownloadsEndpointRequest(int PlexServerId);

public class PausePlexServerDownloadsEndpointRequestValidator : Validator<PausePlexServerDownloadsEndpointRequest>
{
    public PausePlexServerDownloadsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class PausePlexServerDownloadsEndpoint : BaseEndpoint<PausePlexServerDownloadsEndpointRequest>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/server/pause/{PlexServerId}";

    public PausePlexServerDownloadsEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(PausePlexServerDownloadsEndpointRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new PausePlexServerDownloadsCommand(req.PlexServerId), ct);

        await SendFluentResult(result, ct);
    }
}
