using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GetPlexServerByIdEndpointRequest(int PlexServerId);

public class GetPlexServerByIdEndpointRequestValidator : Validator<GetPlexServerByIdEndpointRequest>
{
    public GetPlexServerByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class GetPlexServerByIdEndpoint : BaseEndpoint<GetPlexServerByIdEndpointRequest, PlexServerDTO>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}";

    public GetPlexServerByIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetPlexServerByIdEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexServerByIdEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var plexServer = await _dbContext.PlexServers.Include(x => x.PlexAccountServers).GetAsync(req.PlexServerId, ct);

        if (plexServer is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        await SendFluentResult(Result.Ok(plexServer), x => x.ToDTO(), ct);
    }
}
