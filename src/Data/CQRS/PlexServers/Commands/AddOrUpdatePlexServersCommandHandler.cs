using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
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
    public AddOrUpdatePlexServersCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

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
                Log.Debug($"Updating PlexServer with id: {plexServerDB.Id} in the database.");
                plexServer.Id = plexServerDB.Id;

                _dbContext.Entry(plexServerDB).CurrentValues.SetValues(plexServer);

                SyncPlexServerConnections(plexServer, plexServerDB);
            }
            else
            {
                // Create plexServer
                Log.Debug($"Adding PlexServer with name: {plexServer.Name} to the database.");
                _dbContext.PlexServers.Add(plexServer);
                foreach (var plexServerConnection in plexServer.PlexServerConnections)
                {
                    Log.Debug($"Creating connection {plexServerConnection.ToString()} from {plexServer.Name} in the database.");
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
                Log.Debug($"Creating connection {plexServerConnection.ToString()} from {plexServerDB.Name} in the database.");
                _dbContext.PlexServerConnections.Add(plexServerConnection);
            }
            else
            {
                // Updating Connection
                Log.Debug($"Updating connection {plexServerConnection.ToString()} from {plexServerDB.Name} in the database.");
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
                Log.Debug($"Removing connection {plexServerConnectionDB.ToString()} from {plexServerDB.Name} in the database.");

                _dbContext.Entry(plexServerConnectionDB).State = EntityState.Deleted;
            }
        }
    }
}