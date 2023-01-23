using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexLibraries;

public class AddOrUpdatePlexLibrariesValidator : AbstractValidator<AddOrUpdatePlexLibrariesCommand>
{
    public AddOrUpdatePlexLibrariesValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
        RuleFor(x => x.PlexLibraries).NotEmpty();
        RuleForEach(x => x.PlexLibraries)
            .ChildRules(library =>
            {
                library.RuleFor(x => x.PlexServerId).GreaterThan(0);
                library.RuleFor(x => x.Title).NotEmpty();
                library.RuleFor(x => x.Uuid).NotEmpty();
            });
    }
}

public class AddOrUpdatePlexLibrariesCommandHandler : BaseHandler, IRequestHandler<AddOrUpdatePlexLibrariesCommand, Result>
{
    public AddOrUpdatePlexLibrariesCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(AddOrUpdatePlexLibrariesCommand command, CancellationToken cancellationToken)
    {
        var plexLibraries = command.PlexLibraries;
        var plexAccountId = command.PlexAccountId;
        var plexServerId = plexLibraries[0].PlexServerId;

        var plexAccount = await _dbContext.PlexAccounts.FindAsync(plexAccountId);
        var plexServer = await _dbContext.PlexServers.FindAsync(plexServerId);

        // Add or update the PlexLibraries in the database
        Log.Information($"Starting the add or update process of the PlexLibraries for PlexServer: {plexServer.Name}.");

        foreach (var plexLibrary in plexLibraries)
        {
            var plexLibraryDB = await _dbContext.PlexLibraries
                .FirstOrDefaultAsync(x => x.PlexServer.Id == plexLibrary.PlexServerId && x.Uuid == plexLibrary.Uuid, cancellationToken);

            plexLibrary.SetNull();

            if (plexLibraryDB == null)
            {
                Log.Debug($"Adding PlexLibrary {plexLibrary.Title} to the database.");
                await _dbContext.PlexLibraries.AddAsync(plexLibrary, cancellationToken);
            }
            else
            {
                // PlexServer already exists
                Log.Debug($"Updating PlexLibrary {plexLibrary.Title} with id: {plexLibrary.Id} in the database.");
                plexLibrary.Id = plexLibraryDB.Id;
                _dbContext.PlexLibraries.Update(plexLibrary);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Add or update the PlexAccount, PlexServer and PlexLibrary relationships
        Log.Information($"Adding or updating the PlexAccount ({plexAccount.DisplayName}) association with PlexLibraries now.");
        foreach (var plexLibrary in plexLibraries)
        {
            // Check if this PlexAccount has been associated with the PlexLibrary already
            var plexAccountLibrary = await _dbContext
                .PlexAccountLibraries
                .Where(x => x.PlexAccountId == plexAccountId
                            && x.PlexLibraryId == plexLibrary.Id
                            && x.PlexServerId == plexLibrary.PlexServerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (plexAccountLibrary == null)
            {
                // Add entry
                Log.Debug(
                    $"PlexAccount: {plexAccount.DisplayName} does not have an association with PlexLibrary: {plexLibrary.Name} of PlexServer: {plexServer.Name} creating one now with the authentication token now.");

                await _dbContext.PlexAccountLibraries.AddAsync(new PlexAccountLibrary
                {
                    PlexAccountId = plexAccountId,
                    PlexLibraryId = plexLibrary.Id,
                    PlexServerId = plexServerId,
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

        return Result.Ok();
    }
}