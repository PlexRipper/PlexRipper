using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record SetServerAliasRequest
{
    public required int PlexServerId { get; init; }

    [QueryParam, BindFrom("serverAlias")]
    public required string ServerAlias { get; init; }
}

public class SetServerAliasRequestValidator : Validator<SetServerAliasRequest>
{
    public SetServerAliasRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SetServerAlias : BaseEndpoint<SetServerAliasRequest>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-alias";

    public SetServerAlias(ILogger log, IReaparrDbContext dbContext, IServerSettingsModule serverSettingsModule)
    {
        _log = log.ForContext<SetServerAlias>();
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetServerAliasRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var machineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(req.PlexServerId, ct);
        if (machineIdentifier == string.Empty)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        _serverSettingsModule.SetServerName(machineIdentifier, req.ServerAlias);

        await SendFluentResult(Result.Ok(), ct);
    }
}
