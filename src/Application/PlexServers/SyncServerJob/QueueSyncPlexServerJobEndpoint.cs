using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record QueueSyncPlexServerJobEndpointRequest(int PlexServerId, bool ForceSync = false);

public class QueueSyncPlexServerJobEndpointRequestValidator : Validator<QueueSyncPlexServerJobEndpointRequest>
{
    public QueueSyncPlexServerJobEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueSyncPlexServerJobEndpoint : BaseEndpoint<QueueSyncPlexServerJobEndpointRequest, ResultDTO>
{
    private readonly IMediator _mediator;
    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/sync";

    public QueueSyncPlexServerJobEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(QueueSyncPlexServerJobEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new QueueSyncServerJobCommand(req.PlexServerId, req.ForceSync), ct);
       await SendFluentResult(result, ct);
    }
}