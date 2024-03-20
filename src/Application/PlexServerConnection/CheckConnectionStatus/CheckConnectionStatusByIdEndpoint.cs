using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record CheckConnectionStatusByIdRequest(int PlexServerConnectionId);

public class CheckConnectionStatusByIdRequestValidator : Validator<CheckConnectionStatusByIdRequest>
{
    public CheckConnectionStatusByIdRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class CheckConnectionStatusByIdEndpoint : BaseEndpoint<CheckConnectionStatusByIdRequest, PlexServerStatusDTO>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/check/{PlexServerConnectionId}";

    public CheckConnectionStatusByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerStatusDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CheckConnectionStatusByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckConnectionStatusByIdCommand(req.PlexServerConnectionId), ct);
        if (result.IsFailed)
            await SendFluentResult(result.ToResult(), ct);
        else
            await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}