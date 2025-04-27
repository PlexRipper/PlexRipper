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

public class RefreshLibraryMediaEndpoint : BaseEndpoint<RefreshLibraryMediaEndpointRequest, PlexLibraryDTO>
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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshLibraryMediaCommand(req.PlexLibraryId, _ => { }), ct);

        await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}
