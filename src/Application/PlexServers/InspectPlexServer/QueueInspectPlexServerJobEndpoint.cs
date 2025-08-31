using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;

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

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/inspect";

    public QueueInspectPlexServerJobEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(QueueInspectPlexServerJobEndpointRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new QueueInspectPlexServerJobCommand([req.PlexServerId]), ct);
        await SendFluentResult(result, ct);
    }
}
