using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

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
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/pause/{DownloadTaskGuid}";

    public PauseDownloadTaskEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        // TODO state is changed - use POST / PUT
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(PauseDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var pauseResult = await _commandExecutor.Send(new PauseDownloadTaskCommand(req.DownloadTaskGuid), ct);

        await SendFluentResult(pauseResult, ct);
    }
}
