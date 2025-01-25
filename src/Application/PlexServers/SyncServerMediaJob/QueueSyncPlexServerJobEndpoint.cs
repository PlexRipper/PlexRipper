using System.ComponentModel;
using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record QueueSyncPlexServerJobEndpointRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    public QueueSyncPlexServerJobEndpointRequest(bool forceSync = false)
    {
        ForceSync = forceSync;
    }

    public int PlexServerId { get; init; }

    [QueryParam, BindFrom("forceSync")]
    [DefaultValue(false)]
    public bool ForceSync { get; init; }
}

public class QueueSyncPlexServerJobEndpointRequestValidator : Validator<QueueSyncPlexServerJobEndpointRequest>
{
    public QueueSyncPlexServerJobEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueSyncPlexServerJobEndpoint : BaseEndpoint<QueueSyncPlexServerJobEndpointRequest, BaseResultDTO>
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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(QueueSyncPlexServerJobEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new QueueSyncServerMediaJobCommand(req.PlexServerId, req.ForceSync), ct);
        await SendFluentResult(result, ct);
    }
}
