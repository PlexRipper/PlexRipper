using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record RefreshPlexServerConnectionsEndpointRequest(int PlexServerId);

public class RefreshPlexServerConnectionsEndpointRequestValidator : Validator<RefreshPlexServerConnectionsEndpointRequest>
{
    public RefreshPlexServerConnectionsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class RefreshPlexServerConnectionsEndpoint : BaseCustomEndpoint<RefreshPlexServerConnectionsEndpointRequest, ResultDTO<PlexServerDTO>>
{
    private readonly IMediator _mediator;
    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/refresh";

    public RefreshPlexServerConnectionsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerDTO>))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(RefreshPlexServerConnectionsEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshPlexServerConnectionsCommand(req.PlexServerId), ct);
        if (result.IsFailed)
            await SendResult(result, ct);
        else
            await SendResult(Result.Ok(result.Value.ToDTO()), ct);
    }
}