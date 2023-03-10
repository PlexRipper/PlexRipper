using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetPlexServerConnectionByPlexServerIdQueryValidator : AbstractValidator<GetPlexServerConnectionByPlexServerIdQuery>
{
    public GetPlexServerConnectionByPlexServerIdQueryValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class GetPlexServerConnectionByPlexServerIdQueryHandler : BaseHandler,
    IRequestHandler<GetPlexServerConnectionByPlexServerIdQuery, Result<PlexServerConnection>>
{
    public GetPlexServerConnectionByPlexServerIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<PlexServerConnection>> Handle(GetPlexServerConnectionByPlexServerIdQuery request, CancellationToken cancellationToken)
    {
        var plexServer = await _dbContext
            .PlexServers
            .Include(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus)
            .FirstOrDefaultAsync(x => x.Id == request.PlexServerId, cancellationToken);

        var plexServerConnections = plexServer.PlexServerConnections;
        if (!plexServerConnections.Any())
            return Result.Fail($"PlexServer with id {plexServer.Id} and name {plexServer.Name} has no connections available!").LogError();

        // Find based on preference
        if (plexServer.PreferredConnectionId > 0)
        {
            var connection = plexServerConnections.Find(x => x.Id == plexServer.PreferredConnectionId);
            if (connection is not null)
                return Result.Ok(connection);

            _log.Here()
                .Warning("Could not find preferred connection with id {PlexServerConnectionId} for server {Name}", plexServer.PreferredConnectionId,
                    plexServer.Name);
        }

        // Find based on public address
        _log.Here().Debug("Attempting to find PlexServerConnection that matches the PlexServer public address: {PublicAddress}", plexServer.PublicAddress);

        var publicConnection = plexServerConnections.Find(x => x.Address == plexServer.PublicAddress);
        if (publicConnection is not null)
            return Result.Ok(publicConnection);

        _log.Here().Warning("Could not find connection based on public address: {PublicAddress} for server {Name}", plexServer.PublicAddress, plexServer.Name);

        // Find based on what's successful
        var successPlexServerConnections = plexServerConnections
            .Where(x => x.PlexServerStatus.Any(y => y.IsSuccessful))
            .ToList();
        if (successPlexServerConnections.Any())
            return Result.Ok(successPlexServerConnections.First());

        // Find anything...
        _log.Warning("Could not find a recent successful connection. We're just gonna YOLO this and pick the first connection: {PlexServerConnectionUrl}",
            plexServerConnections.First());
        return Result.Ok(plexServerConnections.First());
    }
}