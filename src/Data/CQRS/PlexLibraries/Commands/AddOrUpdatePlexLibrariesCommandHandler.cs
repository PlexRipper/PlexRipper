using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexLibraries
{
    public class AddOrUpdatePlexLibrariesValidator : AbstractValidator<AddOrUpdatePlexLibrariesCommand>
    {
        public AddOrUpdatePlexLibrariesValidator()
        {
            RuleFor(x => x.PlexServer).NotNull();
            RuleFor(x => x.PlexServer.Id).GreaterThan(0);
            RuleFor(x => x.PlexLibraries).NotEmpty();
        }
    }

    public class AddOrUpdatePlexLibrariesCommandHandler : BaseHandler, IRequestHandler<AddOrUpdatePlexLibrariesCommand, Result>
    {
        public AddOrUpdatePlexLibrariesCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(AddOrUpdatePlexLibrariesCommand command, CancellationToken cancellationToken)
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
                    .FirstOrDefaultAsync(x => x.PlexServer.Id == plexServer.Id && x.Key == plexLibrary.Key, cancellationToken);

                if (plexLibraryDB == null)
                {
                    // Create plexServer
                    Log.Debug($"Adding PlexLibrary {plexLibrary.Title} to the database.");
                    plexLibrary.PlexServerId = plexServer.Id;
                    await _dbContext.PlexLibraries.AddAsync(plexLibrary, cancellationToken);
                }
                else
                {
                    // PlexServer already exists
                    Log.Debug($"Updating PlexLibrary {plexLibrary.Title} with id: {plexLibrary.Id} in the database.");
                    plexLibrary.Id = plexLibraryDB.Id;
                    plexLibrary.PlexServerId = plexServer.Id;
                    _dbContext.PlexLibraries.Update(plexLibrary);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

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
                    .FirstOrDefaultAsync(cancellationToken);

                if (plexAccountLibrary == null)
                {
                    // Add entry
                    Log.Debug(
                        $"PlexAccount: {plexAccount.DisplayName} does not have an association with PlexLibrary: {plexLibrary.Name} of PlexServer: {plexServer.Name} creating one now with the authentication token now.");

                    await _dbContext.PlexAccountLibraries.AddAsync(new PlexAccountLibrary
                    {
                        PlexAccountId = plexAccount.Id,
                        PlexLibraryId = plexLibrary.Id,
                        PlexServerId = plexServer.Id,
                    }, cancellationToken);
                }
                else
                {
                    // Update entry
                    Log.Debug(
                        $"PlexAccount: {plexAccount.DisplayName} already has an association with PlexLibrary: {plexLibrary.Name} of PlexServer: {plexServer.Name} skipping for now.");
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Remove plexLibraries the PlexAccount no longer has access to.
            // TODO Add a separate function that cleans up the database for libraries that have no PlexAccounts which can access them

            //Log.Information("Removing PlexAccount associations with PlexServers that are not accessible anymore");
            //var currentList = new List<PlexAccountLibrary>();
            //foreach (var plexLibrary in plexLibraries)
            //{
            //    var plexAccountLibrary = await _dbContext.PlexAccountLibraries.FirstOrDefaultAsync(x =>
            //        x.PlexAccountId == plexAccount.Id
            //        && x.PlexLibraryId == plexLibrary.Id
            //        && x.PlexServerId == plexServer.Id);
            //    currentList.Add(plexAccountLibrary);
            //}

            // Check if there are plexLibraries that can be deleted because there no PlexAccounts who have access to them anymore.
            //var removalList = await _dbContext.PlexAccountLibraries.AsTracking()
            //    .Where(x => x.PlexAccountId == plexAccount.Id && x.PlexServerId == plexServer.Id)
            //    .ToListAsync();

            //removalList = currentList.Except(removalList).ToList();

            //removalList.Where(x => !currentList.Any(y => y.))

            //if (removalList.Any())
            //{
            //    _dbContext.PlexAccountLibraries.RemoveRange(removalList);

            //    foreach (var plexAccountLibrary in removalList)
            //    {
            //        var list = new List<PlexAccountLibrary> { plexAccountLibrary };
            //        var foundList = await _dbContext.PlexAccountLibraries.Where(x =>
            //            x.PlexLibraryId == plexAccountLibrary.PlexLibraryId
            //            && x.PlexServerId == plexAccountLibrary.PlexServerId).ToListAsync();
            //        foundList = foundList.Except(list).ToList();

            //        if (foundList.Count == 0)
            //        {
            //            var entity = await _dbContext.PlexLibraries.FirstOrDefaultAsync(x => x.Id == plexAccountLibrary.PlexLibraryId);
            //            _dbContext.PlexLibraries.Remove(entity);
            //        }
            //    }
            //    await _dbContext.SaveChangesAsync();
            //}

            return Result.Ok();
        }
    }
}