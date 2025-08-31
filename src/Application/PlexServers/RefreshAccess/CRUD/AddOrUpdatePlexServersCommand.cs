using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public record AddOrUpdatePlexServersCommand(List<PlexServer> PlexServers) : ICommand<Result<PlexServerRapport>>;

public class AddOrUpdatePlexServersCommandValidator : AbstractValidator<AddOrUpdatePlexServersCommand>
{
    public AddOrUpdatePlexServersCommandValidator()
    {
        RuleFor(x => x.PlexServers).NotEmpty().WithMessage("PlexServers list cannot be empty.");

        RuleForEach(x => x.PlexServers)
            .ChildRules(server =>
            {
                server
                    .RuleForEach(s => s.PlexServerConnections)
                    .ChildRules(connection =>
                    {
                        connection.RuleFor(c => c.Protocol).NotEmpty().WithMessage("Protocol is required.");

                        connection.RuleFor(c => c.Address).NotEmpty().WithMessage("Address is required.");

                        connection.RuleFor(c => c.Port).NotEmpty().WithMessage("Port is required.");
                        connection
                            .RuleFor(c => c.LatestConnectionStatus)
                            .Null()
                            .WithMessage("PlexServerStatus should be null.");
                    });
            });
    }
}

public class AddOrUpdatePlexServersCommandHandler
    : ICommandHandler<AddOrUpdatePlexServersCommand, Result<PlexServerRapport>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public AddOrUpdatePlexServersCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<AddOrUpdatePlexServersCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<PlexServerRapport>> ExecuteAsync(
        AddOrUpdatePlexServersCommand command,
        CancellationToken cancellationToken
    )
    {
        var incomingPlexServers = command.PlexServers;
        var rapport = new PlexServerRapport();

        // Add or update the PlexServers in the database
        _log.Here().Information("Adding or updating {PlexServersCount} PlexServers now", incomingPlexServers.Count);

        var machineIds = incomingPlexServers.Select(x => x.MachineIdentifier).ToList();
        var plexServerDbList = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .Where(x => machineIds.Contains(x.MachineIdentifier))
            .AsTracking()
            .ToListAsync(cancellationToken);

        foreach (var incomingPlexServer in incomingPlexServers)
        {
            var existingServer = plexServerDbList.FirstOrDefault(x =>
                x.MachineIdentifier == incomingPlexServer.MachineIdentifier
            );
            if (existingServer != null)
            {
                // PlexServer already exists
                _log.Here().Debug("Updating PlexServer with id: {PlexServerDbId} in the database", existingServer.Id);
                incomingPlexServer.Id = existingServer.Id;

                _dbContext.Entry(existingServer).CurrentValues.SetValues(incomingPlexServer);

                SyncPlexServerConnections(incomingPlexServer, existingServer);
                rapport.Updated.Add(incomingPlexServer.Id);
            }
            else
            {
                // Create plexServer
                _log.Here().Debug("Adding PlexServer with name: {PlexServerName} to the database", incomingPlexServer.Name);
                foreach (var plexServerConnection in incomingPlexServer.PlexServerConnections)
                    plexServerConnection.PlexServerId = incomingPlexServer.Id;

                _dbContext.PlexServers.Add(incomingPlexServer);
                _dbContext.PlexServerConnections.AddRange(incomingPlexServer.PlexServerConnections);
                rapport.Created.Add(incomingPlexServer.Id);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _log.Here().Information(rapport.ToString());

        return Result.Ok(rapport);
    }

    private void SyncPlexServerConnections(PlexServer plexServer, PlexServer existingServer)
    {
        // Create or Update PlexServerConnections
        foreach (var newConnection in plexServer.PlexServerConnections)
        {
            newConnection.PlexServerId = plexServer.Id;

            var existingConnection = existingServer.PlexServerConnections.FirstOrDefault(x => x.Equals(newConnection));

            if (existingConnection is null)
            {
                _log.Here()
                    .Debug(
                        "Creating connection {PlexServerConnection} from {PlexServerName} in the database",
                        newConnection.ToString(),
                        existingServer.Name
                    );
                _dbContext.PlexServerConnections.Add(newConnection);
            }
            else
            {
                _log.Here()
                    .Debug(
                        "Updating connection {PlexServerConnection} from {PlexServerName} in the database",
                        newConnection.ToString(),
                        existingServer.Name
                    );
                newConnection.Id = existingConnection.Id;
                _dbContext.Entry(existingConnection).CurrentValues.SetValues(newConnection);
            }
        }

        foreach (var existingConnection in existingServer.PlexServerConnections.ToList())
        {
            if (
                !existingConnection.IsCustom && !plexServer.PlexServerConnections.Any(x => x.Equals(existingConnection))
            )
            {
                _log.Here()
                    .Debug(
                        "Removing connection {PlexServerConnection} from {PlexServerName} in the database",
                        existingConnection.ToString(),
                        existingServer.Name
                    );
                _dbContext.Entry(existingConnection).State = EntityState.Deleted;
            }
        }
    }
}

public record PlexServerRapport
{
    public List<int> Created { get; } = [];

    public List<int> Updated { get; } = [];

    public override string ToString() =>
        $@"
        Created {nameof(PlexServer)} Access: {Created}
        Updated {nameof(PlexServer)} Access: {Updated}";
}
