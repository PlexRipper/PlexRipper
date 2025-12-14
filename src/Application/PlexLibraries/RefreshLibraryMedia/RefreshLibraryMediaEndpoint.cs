using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

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
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/refresh/{PlexLibraryId}";

    public RefreshLibraryMediaEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<RefreshLibraryMediaEndpoint>();
        _commandExecutor = commandExecutor;
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
        _log.Here().DebugApiCall(HttpContext, req);

        var result = await _commandExecutor.Send(new QueueLibrarySyncJobCommand([req.PlexLibraryId]), ct);

        await SendFluentResult(result, ct);
    }
}
