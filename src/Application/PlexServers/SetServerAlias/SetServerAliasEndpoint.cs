using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Settings.Contracts;

namespace PlexRipper.Application;

public record SetServerAliasRequest
{
    public int PlexServerId { get; init; }

    [QueryParam]
    public string ServerAlias { get; init; } = string.Empty;
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
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-alias";

    public SetServerAlias(IPlexRipperDbContext dbContext, IServerSettingsModule serverSettingsModule)
    {
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(SetServerAliasRequest req, CancellationToken ct)
    {
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
