using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class AddOrUpdatePlexServersValidator : AbstractValidator<AddOrUpdatePlexServersCommand>
{
    public AddOrUpdatePlexServersValidator()
    {
        RuleFor(x => x.PlexServers).NotEmpty();
        RuleForEach(x => x.PlexServers)
            .ChildRules(server =>
            {
                server.RuleFor(x => x.PlexServerConnections).NotEmpty();
                server.RuleForEach(x => x.PlexServerConnections)
                    .ChildRules(connection =>
                    {
                        connection.RuleFor(x => x.Protocol).NotEmpty();
                        connection.RuleFor(x => x.Address).NotEmpty();
                        connection.RuleFor(x => x.Port).NotEmpty();
                    });
            });
    }
}

public class AddOrUpdatePlexServersCommandHandler : BaseHandler, IRequestHandler<AddOrUpdatePlexServersCommand, Result>
{
    public AddOrUpdatePlexServersCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(AddOrUpdatePlexServersCommand command, CancellationToken cancellationToken)
    {
        var plexServers = command.PlexServers;

        // Add or update the PlexServers in the database
        Log.Information($"Adding or updating {plexServers.Count} PlexServers now.");
        foreach (var plexServer in plexServers)
        {
            var plexServerDB =
                await _dbContext.PlexServers
                    .Include(x => x.PlexServerConnections)
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.MachineIdentifier == plexServer.MachineIdentifier, cancellationToken);

            if (plexServerDB != null)
            {
                // PlexServer already exists
                _log.Debug("Updating PlexServer with id: {PlexServerDbId} in the database", plexServerDB.Id);
                plexServer.Id = plexServerDB.Id;

                _dbContext.Entry(plexServerDB).CurrentValues.SetValues(plexServer);

                SyncPlexServerConnections(plexServer, plexServerDB);
            }
            else
            {
                // Create plexServer
                _log.Debug("Adding PlexServer with name: {PlexServerName} to the database", plexServer.Name);
                _dbContext.PlexServers.Add(plexServer);
                foreach (var plexServerConnection in plexServer.PlexServerConnections)
                {
                    _log.Debug("Creating connection {@PlexServerConnection} from {PlexServerName} in the database", plexServerConnection, plexServer.Name, 0);
                    plexServerConnection.PlexServerId = plexServer.Id;
                }

                _dbContext.PlexServerConnections.AddRange(plexServer.PlexServerConnections);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private void SyncPlexServerConnections(PlexServer plexServer, PlexServer plexServerDB)
    {
        // Create or Update PlexServerConnections
        foreach (var plexServerConnection in plexServer.PlexServerConnections)
        {
            plexServerConnection.PlexServerId = plexServer.Id;

            var connectionDb = plexServerDB.PlexServerConnections.Find(x => x.Address == plexServerConnection.Address);
            if (connectionDb is null)
            {
                // Creating Connection
                _log.Debug("Creating connection {@PlexServerConnection} from {PlexServerName} in the database", plexServerConnection, plexServerDB.Name, 0);
                _dbContext.PlexServerConnections.Add(plexServerConnection);
            }
            else
            {
                // Updating Connection
                _log.Debug("Updating connection {@PlexServerConnection} from {PlexServerName} in the database", plexServerConnection, plexServerDB.Name, 0);
                plexServerConnection.Id = connectionDb.Id;
                _dbContext.Entry(connectionDb).CurrentValues.SetValues(plexServerConnection);
            }
        }

        // Delete connections that are not given
        for (var i = plexServerDB.PlexServerConnections.Count - 1; i >= 0; i--)
        {
            var plexServerConnectionDB = plexServerDB.PlexServerConnections[i];
            var connection = plexServer.PlexServerConnections.Find(x => x.Address == plexServerConnectionDB.Address);
            if (connection is null)
            {
                _log.Debug("Removing connection {@PlexServerConnection} from {PlexServerName} in the database", plexServerConnectionDB, plexServerDB.Name, 0);

                _dbContext.Entry(plexServerConnectionDB).State = EntityState.Deleted;
            }
        }
    }
}