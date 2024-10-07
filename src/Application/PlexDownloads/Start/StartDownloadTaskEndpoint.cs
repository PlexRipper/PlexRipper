using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record StartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StartDownloadTaskEndpointRequestValidator : Validator<StartDownloadTaskEndpointRequest>
{
    public StartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StartDownloadTaskEndpoint : BaseEndpoint<StartDownloadTaskEndpointRequest>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/start/{DownloadTaskGuid}";

    public StartDownloadTaskEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(StartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var startResult = await _mediator.Send(new StartDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(startResult, ct);
    }
}
