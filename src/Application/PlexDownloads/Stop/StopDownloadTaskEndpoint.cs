using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record StopDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StopDownloadTaskEndpointRequestValidator : Validator<StopDownloadTaskEndpointRequest>
{
    public StopDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskEndpoint : BaseEndpoint<StopDownloadTaskEndpointRequest>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/stop/{DownloadTaskGuid}";

    public StopDownloadTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(StopDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var stopResult = await _mediator.Send(new StopDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(stopResult, ct);
    }
}
