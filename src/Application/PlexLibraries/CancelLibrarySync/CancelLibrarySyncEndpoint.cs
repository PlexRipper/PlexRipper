using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;

namespace Reaparr.Application;

public record CancelLibrarySyncEndpointRequest(int PlexLibraryId);

public class CancelLibrarySyncEndpointRequestValidator : Validator<CancelLibrarySyncEndpointRequest>
{
    public CancelLibrarySyncEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class CancelLibrarySyncEndpoint : BaseEndpoint<CancelLibrarySyncEndpointRequest>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/cancel/{PlexLibraryId}";

    public CancelLibrarySyncEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CancelLibrarySyncEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancelLibrarySyncEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var result = await _commandExecutor.Send(new CancelLibrarySyncJobCommand(req.PlexLibraryId), ct);

        await SendFluentResult(result, ct);
    }
}
