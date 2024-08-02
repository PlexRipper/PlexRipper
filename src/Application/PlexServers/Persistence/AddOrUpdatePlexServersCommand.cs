using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class AddOrUpdatePlexServersCommand : IAddOrUpdatePlexServersCommand
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly AddOrUpdatePlexServersValidator _validator;

    public AddOrUpdatePlexServersCommand(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
        _validator = new AddOrUpdatePlexServersValidator();
    }

    public async Task<Result> ExecuteAsync(List<PlexServer> plexServers, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(plexServers, cancellationToken);
        if (!result.IsValid)
            return result.ToFluentResult().LogError();

        // Add or update the PlexServers in the database
        _log.Information("Adding or updating {PlexServersCount} PlexServers now", plexServers.Count);
        foreach (var plexServer in plexServers)
        {
            var plexServerDB = await _dbContext
                .PlexServers.Include(x => x.PlexServerConnections)
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
                    _log.Here()
                        .Debug(
                            "Creating connection {PlexServerConnection} from {PlexServerName} in the database",
                            plexServerConnection.ToString(),
                            plexServer.Name
                        );
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
                _log.Here()
                    .Debug(
                        "Creating connection {PlexServerConnection} from {PlexServerName} in the database",
                        plexServerConnection.ToString(),
                        plexServerDB.Name
                    );
                _dbContext.PlexServerConnections.Add(plexServerConnection);
            }
            else
            {
                // Updating Connection
                _log.Here()
                    .Debug(
                        "Updating connection {PlexServerConnection} from {PlexServerName} in the database",
                        plexServerConnection.ToString(),
                        plexServerDB.Name
                    );
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
                _log.Here()
                    .Debug(
                        "Removing connection {PlexServerConnection} from {PlexServerName} in the database",
                        plexServerConnectionDB.ToString(),
                        plexServerDB.Name
                    );

                _dbContext.Entry(plexServerConnectionDB).State = EntityState.Deleted;
            }
        }
    }
}

public class AddOrUpdatePlexServersValidator : AbstractValidator<List<PlexServer>>
{
    public AddOrUpdatePlexServersValidator()
    {
        RuleFor(x => x).NotEmpty();
        RuleForEach(x => x)
            .ChildRules(server =>
            {
                server.RuleFor(x => x.PlexServerConnections).NotEmpty();
                server
                    .RuleForEach(x => x.PlexServerConnections)
                    .ChildRules(connection =>
                    {
                        connection.RuleFor(x => x.Protocol).NotEmpty();
                        connection.RuleFor(x => x.Address).NotEmpty();
                        connection.RuleFor(x => x.Port).NotEmpty();
                    });
            });
    }
}
