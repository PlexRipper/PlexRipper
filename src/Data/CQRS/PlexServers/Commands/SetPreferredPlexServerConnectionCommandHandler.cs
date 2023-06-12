using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class SetPreferredPlexServerConnectionCommandValidator : AbstractValidator<SetPreferredPlexServerConnectionCommand>
{
    public SetPreferredPlexServerConnectionCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class SetPreferredPlexServerConnectionCommandHandler : BaseHandler, IRequestHandler<SetPreferredPlexServerConnectionCommand, Result>
{
    public SetPreferredPlexServerConnectionCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(SetPreferredPlexServerConnectionCommand command, CancellationToken cancellationToken)
    {
        var plexServerConnectionId = command.PlexServerConnectionId;
        var plexServerId = command.PlexServerId;

        _log.Debug("Setting the preferred {NameOfPlexServerConnection} for {PlexServerIdName}: {PlexServerId}", nameof(PlexServerConnection),
            nameof(plexServerId), plexServerId);

        var plexServer = await _dbContext.PlexServers
            .Include(x => x.PlexServerConnections)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == plexServerId, cancellationToken);

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId).LogError();

        var connectionIds = plexServer.PlexServerConnections.Select(x => x.Id).ToList();
        if (!connectionIds.Contains(plexServerConnectionId))
        {
            return Result
                .Fail($"PlexServer with id {plexServerId} has no connections with id {plexServerConnectionId} and can not set that as preferred")
                .LogError();
        }

        plexServer.PreferredConnectionId = plexServerConnectionId;

        await SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}