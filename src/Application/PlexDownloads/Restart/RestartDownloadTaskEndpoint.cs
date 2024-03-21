using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace PlexRipper.Application;

public record RestartDownloadTaskEndpointRequest(Guid Guid);

public class RestartDownloadTaskEndpointRequestValidator : Validator<RestartDownloadTaskEndpointRequest>
{
    public RestartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.Guid).NotEmpty();
    }
}

public class RestartDownloadTaskEndpoint : BaseEndpoint<RestartDownloadTaskEndpointRequest>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/restart";

    public RestartDownloadTaskEndpoint(IMediator mediator)
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
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(RestartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var restartResult = await _mediator.Send(new RestartDownloadTaskCommand(req.Guid), ct);

        await SendFluentResult(restartResult, ct);
    }
}