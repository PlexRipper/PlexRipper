namespace Reaparr.Application;

public record SetServerEnabledRequest
{
    [RouteParam, BindFrom("PlexServerId")]
    public int PlexServerId { get; init; }

    public bool IsEnabled { get; init; } = true;
}

public class SetServerEnabledRequestValidator : Validator<SetServerEnabledRequest>
{
    public SetServerEnabledRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SetServerEnabledRequestEndpoint : BaseEndpoint<SetServerEnabledRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-enabled";

    public SetServerEnabledRequestEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        IServerSettingsModule serverSettingsModule
    )
    {
        _log = log.ForContext<SetServerEnabledRequestEndpoint>();
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetServerEnabledRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var machineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(req.PlexServerId, ct);
        if (machineIdentifier == string.Empty)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        _serverSettingsModule.SetServerHiddenState(machineIdentifier, !req.IsEnabled);

        await _dbContext
            .PlexServers.Where(x => x.MachineIdentifier == machineIdentifier)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsEnabled, req.IsEnabled), ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}

public record SetServerOwnedRequest
{
    public int PlexServerId { get; init; }

    [QueryParam, BindFrom("owned")]
    public bool Owned { get; init; }
}

public class SetServerOwnedRequestValidator : Validator<SetServerOwnedRequest>
{
    public SetServerOwnedRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SetServerOwnedRequestEndpoint : BaseEndpoint<SetServerOwnedRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-owned";

    public SetServerOwnedRequestEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<SetServerOwnedRequestEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetServerOwnedRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var plexServerExists = await _dbContext.PlexServers.AnyAsync(x => x.Id == req.PlexServerId, ct);
        if (!plexServerExists)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        await _dbContext
            .PlexAccountServers.Where(x => x.PlexServerId == req.PlexServerId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsServerOwned, req.Owned), ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}
