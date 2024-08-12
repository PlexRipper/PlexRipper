using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record AddOrUpdatePlexServersCommand(List<PlexServer> PlexServers) : IRequest<Result>;

public class AddOrUpdatePlexServersCommandValidator : AbstractValidator<AddOrUpdatePlexServersCommand>
{
    public AddOrUpdatePlexServersCommandValidator()
    {
        RuleFor(x => x.PlexServers).NotEmpty().WithMessage("PlexServers list cannot be empty.");

        RuleForEach(x => x.PlexServers)
            .ChildRules(server =>
            {
                server
                    .RuleFor(s => s.PlexServerConnections)
                    .NotEmpty()
                    .WithMessage("PlexServerConnections list cannot be empty.");

                server
                    .RuleForEach(s => s.PlexServerConnections)
                    .ChildRules(connection =>
                    {
                        connection.RuleFor(c => c.Protocol).NotEmpty().WithMessage("Protocol is required.");

                        connection.RuleFor(c => c.Address).NotEmpty().WithMessage("Address is required.");

                        connection.RuleFor(c => c.Port).NotEmpty().WithMessage("Port is required.");
                    });
            });
    }
}

public class AddOrUpdatePlexServersCommandHandler : IRequestHandler<AddOrUpdatePlexServersCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public AddOrUpdatePlexServersCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(AddOrUpdatePlexServersCommand command, CancellationToken cancellationToken)
    {
        var plexServers = command.PlexServers;

        // Add or update the PlexServers in the database
        _log.Information("Adding or updating {PlexServersCount} PlexServers now", plexServers.Count);

        var machineIds = plexServers.Select(x => x.MachineIdentifier).ToList();
        var plexServerDbList = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .Where(x => machineIds.Contains(x.MachineIdentifier))
            .AsTracking()
            .ToListAsync(cancellationToken);

        foreach (var plexServer in plexServers)
        {
            var existingServer = plexServerDbList.FirstOrDefault(x =>
                x.MachineIdentifier == plexServer.MachineIdentifier
            );
            if (existingServer != null)
            {
                // PlexServer already exists
                _log.Debug("Updating PlexServer with id: {PlexServerDbId} in the database", existingServer.Id);
                plexServer.Id = existingServer.Id;

                _dbContext.Entry(existingServer).CurrentValues.SetValues(plexServer);

                SyncPlexServerConnections(plexServer, existingServer);
            }
            else
            {
                // Create plexServer
                _log.Debug("Adding PlexServer with name: {PlexServerName} to the database", plexServer.Name);
                foreach (var plexServerConnection in plexServer.PlexServerConnections)
                    plexServerConnection.PlexServerId = plexServer.Id;

                _dbContext.PlexServers.Add(plexServer);
                _dbContext.PlexServerConnections.AddRange(plexServer.PlexServerConnections);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private void SyncPlexServerConnections(PlexServer plexServer, PlexServer existingServer)
    {
        // Create or Update PlexServerConnections
        foreach (var newPlexServerConnection in plexServer.PlexServerConnections)
        {
            newPlexServerConnection.PlexServerId = plexServer.Id;

            var connectionDb = existingServer.PlexServerConnections.FirstOrDefault(x =>
                x.Equals(newPlexServerConnection)
            );
            if (connectionDb is null)
            {
                // Creating Connection
                _log.Here()
                    .Debug(
                        "Creating connection {PlexServerConnection} from {PlexServerName} in the database",
                        newPlexServerConnection.ToString(),
                        existingServer.Name
                    );
                _dbContext.PlexServerConnections.Add(newPlexServerConnection);
            }
            else
            {
                // Updating Connection
                _log.Here()
                    .Debug(
                        "Updating connection {PlexServerConnection} from {PlexServerName} in the database",
                        newPlexServerConnection.ToString(),
                        existingServer.Name
                    );
                newPlexServerConnection.Id = connectionDb.Id;
                _dbContext.Entry(connectionDb).CurrentValues.SetValues(newPlexServerConnection);
            }
        }

        // Delete connections that are not given
        for (var i = existingServer.PlexServerConnections.Count - 1; i >= 0; i--)
        {
            var plexServerConnectionDB = existingServer.PlexServerConnections[i];
            var connection = plexServer.PlexServerConnections.FirstOrDefault(x => x.Equals(plexServerConnectionDB));
            if (connection is null)
            {
                _log.Here()
                    .Debug(
                        "Removing connection {PlexServerConnection} from {PlexServerName} in the database",
                        plexServerConnectionDB.ToString(),
                        existingServer.Name
                    );

                _dbContext.Entry(plexServerConnectionDB).State = EntityState.Deleted;
            }
        }
    }
}
