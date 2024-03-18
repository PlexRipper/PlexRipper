using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record RefreshLibraryMediaEndpointRequest(int PlexLibraryId);

public class RefreshLibraryMediaEndpointRequestValidator : Validator<RefreshLibraryMediaEndpointRequest>
{
    public RefreshLibraryMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaEndpoint : BaseCustomEndpoint<RefreshLibraryMediaEndpointRequest, ResultDTO<PlexLibraryDTO>>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/refresh/{PlexLibraryId}";

    public RefreshLibraryMediaEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(RefreshLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshLibraryMediaCommand(req.PlexLibraryId), ct);
        await SendResult(Result.Ok(result.Value.ToDTO()), ct);
    }
}