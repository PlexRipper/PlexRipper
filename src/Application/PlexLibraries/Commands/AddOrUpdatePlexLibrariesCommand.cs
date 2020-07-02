using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.JoinTables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexLibraries.Commands
{
    public class AddOrUpdatePlexLibrariesCommand : IRequest<Result<bool>>
    {
        public PlexAccount PlexAccount { get; }
        public PlexServer PlexServer { get; }
        public List<PlexLibrary> PlexLibraries { get; }

        public AddOrUpdatePlexLibrariesCommand(PlexAccount plexAccount, PlexServer plexServer, List<PlexLibrary> plexLibraries)
        {
            PlexAccount = plexAccount;
            PlexServer = plexServer;
            PlexLibraries = plexLibraries;
        }
    }

    public class AddOrUpdatePlexLibrariesValidator : AbstractValidator<AddOrUpdatePlexLibrariesCommand>
    {
        public AddOrUpdatePlexLibrariesValidator()
        {
            RuleFor(x => x.PlexServer).NotNull();
            RuleFor(x => x.PlexServer.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibraries).NotEmpty();
        }
    }

    public class AddOrUpdatePlexLibrariesHandler : IRequestHandler<AddOrUpdatePlexLibrariesCommand, Result<bool>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public AddOrUpdatePlexLibrariesHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(AddOrUpdatePlexLibrariesCommand command, CancellationToken cancellationToken)
        {
            var plexAccount = command.PlexAccount;
            var plexServer = command.PlexServer;
            var plexLibraries = command.PlexLibraries;

            // Add or update the PlexLibraries in the database
            Log.Information($"Starting the add or update process of the PlexLibraries for PlexServer: {plexServer.Name}.");
            Log.Information("Adding or updating PlexServers now.");
            foreach (var plexLibrary in plexLibraries)
            {
                var plexLibraryDB = await _dbContext.PlexLibraries
                    .Include(x => x.PlexServer)
                    .FirstOrDefaultAsync(x =>
                    x.PlexServer.Id == plexServer.Id && x.Key == plexLibrary.Key);


                if (plexLibraryDB != null)
                {
                    // PlexServer already exists
                    Log.Debug($"Updating PlexLibrary {plexLibrary.Title} with id: {plexLibrary.Id} in the database.");
                    plexLibrary.Id = plexLibraryDB.Id;
                    _dbContext.PlexLibraries.Update(plexLibrary);
                }
                else
                {
                    // Create plexServer
                    Log.Debug($"Adding PlexLibrary {plexLibrary.Title} to the database.");
                    await _dbContext.PlexLibraries.AddAsync(plexLibrary);
                }

            }

            await _dbContext.SaveChangesAsync();

            // Add or update the PlexAccount, PlexServer and PlexLibrary relationships 
            Log.Information("Adding or updating the PlexAccount association with PlexLibraries now.");
            foreach (var plexLibrary in plexLibraries)
            {
                // Check if this PlexAccount has been associated with the PlexLibrary already
                var plexAccountLibrary = await _dbContext
                    .PlexAccountLibraries
                    .Where(x => x.PlexAccountId == plexAccount.Id
                                && x.PlexLibraryId == plexLibrary.Id
                                && x.PlexServerId == plexServer.Id)
                    .FirstOrDefaultAsync();

                if (plexAccountLibrary == null)
                {
                    // Add entry
                    Log.Debug($"PlexAccount: {plexAccount.DisplayName} does not have an association with PlexLibrary: {plexLibrary.Name} of PlexServer: {plexServer.Name} creating one now with the authentication token now.");

                    await _dbContext.PlexAccountLibraries.AddAsync(new PlexAccountLibrary
                    {
                        PlexAccountId = plexLibrary.Id,
                        PlexLibraryId = plexLibrary.Id,
                        PlexServerId = plexLibrary.Id
                    }, cancellationToken);
                }
                // Update entry
                Log.Debug($"PlexAccount: {plexAccount.DisplayName} already has an association with PlexLibrary: {plexLibrary.Name} of PlexServer: {plexServer.Name} skipping for now.");

            }

            await _dbContext.SaveChangesAsync();

            // Remove plexLibraries the PlexAccount no longer has access to.
            Log.Information("Removing PlexAccount associations with PlexServers now that are not accessible anymore");
            var currentList = new List<PlexAccountLibrary>();
            foreach (var plexLibrary in plexLibraries)
            {
                var plexAccountLibrary = await _dbContext.PlexAccountLibraries.FirstOrDefaultAsync(x =>
                    x.PlexAccountId == plexAccount.Id
                    && x.PlexLibraryId == plexLibrary.Id
                    && x.PlexServerId == plexServer.Id);
                currentList.Add(plexAccountLibrary);
            }

            var removalList = _dbContext.PlexAccountLibraries.AsTracking()
                .Where(x => x.PlexAccountId == plexAccount.Id && x.PlexServerId == plexServer.Id)
                .Except(currentList);

            _dbContext.PlexAccountLibraries.RemoveRange(removalList);

            // Check if there are plexLibraries that can be deleted because there no PlexAccounts who have access to them anymore. 
            foreach (var plexAccountLibrary in removalList)
            {
                var list = new List<PlexAccountLibrary> { plexAccountLibrary };
                var foundList = await _dbContext.PlexAccountLibraries.Where(x =>
                    x.PlexLibraryId == plexAccountLibrary.PlexLibraryId
                    && x.PlexServerId == plexAccountLibrary.PlexServerId).Except(list).ToListAsync();
                if (foundList.Count == 0)
                {
                    var entity = await _dbContext.PlexLibraries.FirstOrDefaultAsync(x => x.Id == plexAccountLibrary.PlexLibraryId);
                    _dbContext.PlexLibraries.Remove(entity);
                }

            }
            await _dbContext.SaveChangesAsync();

            return Result.Ok(true);
        }
    }
}
