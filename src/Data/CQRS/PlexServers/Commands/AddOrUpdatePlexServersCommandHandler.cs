using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers
{
    public class AddOrUpdatePlexServersValidator : AbstractValidator<AddOrUpdatePlexServersCommand>
    {
        public AddOrUpdatePlexServersValidator()
        {
            RuleFor(x => x.PlexAccount).NotNull();
            RuleFor(x => x.PlexAccount.Id).GreaterThan(0);
            RuleFor(x => x.PlexServers).NotEmpty();
        }
    }

    public class AddOrUpdatePlexServersHandler : BaseHandler, IRequestHandler<AddOrUpdatePlexServersCommand, Result>
    {
        public AddOrUpdatePlexServersHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(AddOrUpdatePlexServersCommand command, CancellationToken cancellationToken)
        {
            var plexAccount = command.PlexAccount;
            var plexServers = command.PlexServers;

            // Add or update the PlexServers in the database
            Log.Information($"Starting the Add or update process of the PlexServers for PlexAccount: {plexAccount.Id}.");
            Log.Information("Adding or updating PlexServers now.");
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

            // Add or update the PlexAccount and PlexServer relationships
            Log.Information("Adding or updating the PlexAccount association with PlexServers now.");
            foreach (var plexServer in plexServers)
            {
                // Check if this PlexAccount has been associated with the plexServer already
                var plexAccountServer = await _dbContext
                    .PlexAccountServers
                    .AsTracking()
                    .Where(x => x.PlexAccountId == plexAccount.Id && x.PlexServerId == plexServer.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (plexAccountServer != null)
                {
                    // Update entry
                    Log.Debug(
                        $"PlexAccount {plexAccount.DisplayName} already has an association with PlexServer: {plexServer.Name}, updating authentication token now.");
                    plexAccountServer.AuthToken = plexServer.AccessToken; // TODO need a better method to transfer the plexAccount tokens from the DTO
                    plexAccountServer.AuthTokenCreationDate = DateTime.Now;
                    plexAccountServer.Owned = plexAccount.PlexId == plexServer.OwnerId;
                }
                else
                {
                    // Add entry
                    Log.Debug(
                        $"PlexAccount {plexAccount.DisplayName} does not have an association with PlexServer: {plexServer.Name}, creating one now with the authentication token now.");
                    await _dbContext.PlexAccountServers.AddAsync(new PlexAccountServer
                    {
                        PlexAccountId = plexAccount.Id,
                        PlexServerId = plexServer.Id,
                        AuthToken = plexServer.AccessToken, // TODO need a better method to transfer the plexAccount tokens from the DTO
                        Owned = plexAccount.PlexId == plexServer.OwnerId,
                        AuthTokenCreationDate = DateTime.Now,
                    }, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            Log.Information("Removing PlexAccount associations with PlexServers now that are not accessible anymore");

            // The list of all past and current serverId's the plexAccount has access too
            List<int> currentList = await _dbContext.PlexAccountServers
                .Where(x => x.PlexAccountId == plexAccount.Id)
                .Select(x => x.PlexServerId).ToListAsync(cancellationToken);

            // The list which contains the serverId's the plexAccount has access too after the update.
            List<int> newList = plexServers.Select(x => x.Id).ToList();

            // Remove plexServer associations which the PlexAccount has no longer access too.
            List<int> removalList = currentList.Except(newList).ToList();
            foreach (int serverId in removalList)
            {
                var entity = await _dbContext.PlexAccountServers.AsTracking()
                    .FirstOrDefaultAsync(x => x.PlexAccountId == plexAccount.Id && x.PlexServerId == serverId, cancellationToken);
                if (entity != null)
                {
                    _dbContext.PlexAccountServers.Remove(entity);
                }
            }

            return Result.Ok();
        }
    }
}