using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Sync all media from a Plex server by queuing sync jobs for all its libraries.
/// </summary>
public record SyncPlexServerMediaEndpointRequest
{
    public int PlexServerId { get; init; }
}

public class SyncPlexServerMediaEndpointRequestValidator : Validator<SyncPlexServerMediaEndpointRequest>
{
    public SyncPlexServerMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SyncPlexServerMediaEndpoint : BaseEndpoint<SyncPlexServerMediaEndpointRequest, BaseResultDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/sync";

    public SyncPlexServerMediaEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<SyncPlexServerMediaEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SyncPlexServerMediaEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var libraryIds = await _dbContext
            .PlexLibraries.Where(x => x.PlexServerId == req.PlexServerId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken: ct);

        if (!libraryIds.Any())
        {
            var name = await _dbContext.GetPlexServerNameById(req.PlexServerId, cancellationToken: ct);
            var warnResult = _log.WarningResult("Plex server {Name} has no libraries to available to sync", name);
            await SendFluentResult(warnResult.Add400BadRequestError(), ct);
            return;
        }

        var result = await _commandExecutor.Send(new QueueLibrarySyncJobCommand(libraryIds), ct);
        await SendFluentResult(result, ct);
    }
}
