using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Pause a currently downloading <see cref="DownloadTaskGeneric"/>.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to pause.</param>
/// <returns>Is successful.</returns>
public record PauseDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class PauseDownloadTaskEndpointRequestValidator : Validator<PauseDownloadTaskEndpointRequest>
{
    public PauseDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class PauseDownloadTaskEndpoint : BaseEndpoint<PauseDownloadTaskEndpointRequest>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/pause/{DownloadTaskGuid}";

    public PauseDownloadTaskEndpoint(IMediator mediator)
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

    public override async Task HandleAsync(PauseDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var pauseResult = await _mediator.Send(new PauseDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(pauseResult, ct);
    }
}
