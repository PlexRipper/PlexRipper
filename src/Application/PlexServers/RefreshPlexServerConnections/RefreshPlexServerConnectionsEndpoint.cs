using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record RefreshPlexServerConnectionsEndpointRequest(int PlexServerId);

public class RefreshPlexServerConnectionsEndpointRequestValidator
    : Validator<RefreshPlexServerConnectionsEndpointRequest>
{
    public RefreshPlexServerConnectionsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class RefreshPlexServerConnectionsEndpoint : BaseEndpoint<RefreshPlexServerConnectionsEndpointRequest>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/refresh";

    public RefreshPlexServerConnectionsEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<RefreshPlexServerConnectionsEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
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

    public override async Task HandleAsync(RefreshPlexServerConnectionsEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await _dbContext.ChoosePlexAccountToConnect(req.PlexServerId, ct);
        if (plexAccountResult.IsFailed)
        {
            await SendFluentResult(plexAccountResult.ToResult(), ct);
            return;
        }

        var refreshResult = await _commandExecutor.Send(
            new RefreshPlexServerAccessCommand(plexAccountResult.Value.Id),
            ct
        );

        await SendFluentResult(refreshResult.ToResult(), ct);
    }
}
