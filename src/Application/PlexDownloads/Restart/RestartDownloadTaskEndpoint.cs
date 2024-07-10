using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record RestartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class RestartDownloadTaskEndpointRequestValidator : Validator<RestartDownloadTaskEndpointRequest>
{
    public RestartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskEndpoint(IMediator mediator) : BaseEndpoint<RestartDownloadTaskEndpointRequest>
{
    private readonly IMediator _mediator = mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/restart/{DownloadTaskGuid}";

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
        var restartResult = await _mediator.Send(new RestartDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(restartResult, ct);
    }
}