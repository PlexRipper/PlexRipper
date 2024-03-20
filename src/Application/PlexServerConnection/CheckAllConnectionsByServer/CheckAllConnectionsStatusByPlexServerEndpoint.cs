using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record CheckAllConnectionsStatusByPlexServerRequest(int PlexServerId);

public class CheckAllConnectionsStatusByPlexServerRequestValidator : Validator<CheckAllConnectionsStatusByPlexServerRequest>
{
    public CheckAllConnectionsStatusByPlexServerRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerEndpoint : BaseCustomEndpoint<CheckAllConnectionsStatusByPlexServerRequest,
    List<PlexServerStatusDTO>>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/check/by-server/{PlexServerId}";

    public CheckAllConnectionsStatusByPlexServerEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerStatusDTO>>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CheckAllConnectionsStatusByPlexServerRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckAllConnectionsStatusByPlexServerCommand(req.PlexServerId), ct);
        if (result.IsFailed)
            await SendFluentResult(result.ToResult(), ct);
        else
            await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}