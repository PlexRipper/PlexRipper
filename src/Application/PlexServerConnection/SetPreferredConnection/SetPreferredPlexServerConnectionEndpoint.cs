using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Sets the preferred connection for a <see cref="PlexServer"/>
/// </summary>
/// <param name="PlexServerId"> The id of the <see cref="PlexServer"/> to set the preferred connection for.</param>
/// <param name="PlexServerConnectionId"> The id of the <see cref="PlexServerConnection"/> to set as preferred.</param>
/// <returns></returns>
public record SetPreferredPlexServerConnectionEndpointRequest(int PlexServerId, int PlexServerConnectionId);

public class SetPreferredPlexServerConnectionEndpointRequestValidator : Validator<SetPreferredPlexServerConnectionEndpointRequest>
{
    public SetPreferredPlexServerConnectionEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class SetPreferredPlexServerConnectionEndpoint : BaseCustomEndpoint<SetPreferredPlexServerConnectionEndpointRequest, ResultDTO>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/preferred-connection/{PlexServerConnectionId}";

    public SetPreferredPlexServerConnectionEndpoint(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(SetPreferredPlexServerConnectionEndpointRequest req, CancellationToken ct)
    {
        var plexServerConnectionId = req.PlexServerConnectionId;
        var plexServerId = req.PlexServerId;

        _log.Debug("Setting the preferred {NameOfPlexServerConnection} for {PlexServerIdName}: {PlexServerId}", nameof(PlexServerConnection),
            nameof(plexServerId), plexServerId);

        var plexServer = await _dbContext.PlexServers
            .Include(x => x.PlexServerConnections)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == plexServerId, ct);

        if (plexServer is null)
        {
           await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId).LogError(), ct);
            return;
        }

        var connectionIds = plexServer.PlexServerConnections.Select(x => x.Id).ToList();
        if (!connectionIds.Contains(plexServerConnectionId))
        {
           await SendFluentResult(Result
                .Fail($"PlexServer with id {plexServerId} has no connections with id {plexServerConnectionId} and can not set that as preferred")
                .LogError(), ct);
            return;
        }

        plexServer.PreferredConnectionId = plexServerConnectionId;

        await _dbContext.SaveChangesAsync(ct);

       await SendFluentResult(Result.Ok(), ct);
    }
}