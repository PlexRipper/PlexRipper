using EFCore.BulkExtensions;
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
                await _dbContext.PlexServers.FirstOrDefaultAsync(x => x.MachineIdentifier == plexServer.MachineIdentifier, cancellationToken);

            if (plexServerDB != null)
            {
                // PlexServer already exists
                Log.Debug($"Updating PlexServer with id: {plexServerDB.Id} in the database.");
                plexServer.Id = plexServerDB.Id;
                _dbContext.PlexServers.Update(plexServer);
            }
            else
            {
                // Create plexServer
                Log.Debug($"Adding PlexServer with name: {plexServer.Name} to the database.");
                await _dbContext.PlexServers.AddAsync(plexServer, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var plexServer in plexServers)
            await _dbContext.BulkInsertOrUpdateOrDeleteAsync(plexServer.PlexServerConnections, cancellationToken: cancellationToken);

        return Result.Ok();
    }
}