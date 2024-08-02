using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Settings.Contracts;

namespace PlexRipper.Application;

public record SetServerHiddenRequest
{
    public int PlexServerId { get; init; }

    [QueryParam]
    public bool Hidden { get; init; } = false;
}

public class SetServerHiddenRequestValidator : Validator<SetServerHiddenRequest>
{
    public SetServerHiddenRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SetServerHiddenRequestEndpoint : BaseEndpoint<SetServerHiddenRequest>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-hidden";

    public SetServerHiddenRequestEndpoint(IPlexRipperDbContext dbContext, IServerSettingsModule serverSettingsModule)
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

    public override async Task HandleAsync(SetServerHiddenRequest req, CancellationToken ct)
    {
        var machineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(req.PlexServerId, ct);
        if (machineIdentifier == string.Empty)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        _serverSettingsModule.SetServerHiddenState(machineIdentifier, req.Hidden);

        await SendFluentResult(Result.Ok(), ct);
    }
}
