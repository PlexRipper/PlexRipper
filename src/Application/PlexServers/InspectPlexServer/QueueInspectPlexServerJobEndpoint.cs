using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

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
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/inspect";

    public QueueInspectPlexServerJobEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(QueueInspectPlexServerJobEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new QueueInspectPlexServerJobCommand(req.PlexServerId), ct);
        await SendFluentResult(result, ct);
    }
}
