using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record CreateDownloadTasksEndpointRequest
{
    [FromBody]
    public CreateDownloadTasksRequest Request { get; set; }
}

public class CreateDownloadTasksEndpoint : BaseEndpoint<CreateDownloadTasksEndpointRequest>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/create";

    public CreateDownloadTasksEndpoint(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CreateDownloadTasksEndpointRequest req, CancellationToken ct)
    {
        _log.DebugLine("Attempting to add download task orders: ");
        foreach (var downloadMediaDto in req.Request.DownloadMedias)
            _log.Debug("DownloadMediaDTO: {@DownloadMediaDto} ", downloadMediaDto);

        var result = await _mediator.Send(new CreateDownloadTasksCommand(req.Request), ct);

        await SendFluentResult(result, ct);
    }
}
