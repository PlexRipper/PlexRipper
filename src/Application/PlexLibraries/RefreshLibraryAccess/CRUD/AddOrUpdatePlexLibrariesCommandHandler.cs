using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record AddOrUpdatePlexLibrariesCommand : ICommand<Result<List<PlexLibraryAccessRapport>>>
{
    public required int PlexAccountId { get; init; }

    public required List<PlexLibrary> PlexLibraries { get; init; }
}

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

public class AddOrUpdatePlexLibrariesCommandHandler
    : ICommandHandler<AddOrUpdatePlexLibrariesCommand, Result<List<PlexLibraryAccessRapport>>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly List<PlexLibraryAccessRapport> _list = [];

    public AddOrUpdatePlexLibrariesCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<AddOrUpdatePlexLibrariesCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexLibraryAccessRapport>>> ExecuteAsync(
        AddOrUpdatePlexLibrariesCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;

        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);

        if (plexAccount is null)
            return ResultExtensions.IsNull(nameof(plexAccount));

        var plexServerLibrariesDict = command
            .PlexLibraries.GroupBy(x => x.PlexServerId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var (_, incomingPlexLibraries) in plexServerLibrariesDict)
        {
            foreach (var incomingPlexLibrary in incomingPlexLibraries)
            {
                var plexLibraryDb = await _dbContext
                    .PlexLibraries.AsTracking()
                    .FirstOrDefaultAsync(
                        x => x.PlexServerId == incomingPlexLibrary.PlexServerId && x.Uuid == incomingPlexLibrary.Uuid,
                        cancellationToken
                    );

                if (plexLibraryDb is null)
                {
                    _log.Here()
                        .Debug("Adding PlexLibrary {PlexLibraryName} to the database", incomingPlexLibrary.Title);
                    await _dbContext.PlexLibraries.AddAsync(incomingPlexLibrary, cancellationToken);
                }
                else
                {
                    incomingPlexLibrary.Id = plexLibraryDb.Id;
                    incomingPlexLibrary.SyncedAt = plexLibraryDb.SyncedAt;
                    incomingPlexLibrary.DefaultDestinationId = plexLibraryDb.DefaultDestinationId;

                    _log.Here()
                        .Debug(
                            "Updating PlexLibrary {PlexLibraryName} with id: {PlexLibraryId} in the database",
                            incomingPlexLibrary.Title,
                            incomingPlexLibrary.Id
                        );

                    incomingPlexLibrary.DefaultDestinationId = plexLibraryDb.DefaultDestinationId;
                    _dbContext.Entry(plexLibraryDb).CurrentValues.SetValues(incomingPlexLibrary);
                }
            }

            // NOTE: We don't delete libraries here, access can be temporarily suspended due to missing PlexServer access or offline.
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Add or update the PlexAccount, PlexServer and PlexLibrary relationships
        _log.Here()
            .Information(
                "Adding, updating or removing the PlexAccount: {PlexAccountDisplayName} association with PlexLibraries now",
                plexAccount.DisplayName
            );

        foreach (var (plexServerId, incomingPlexLibraries) in plexServerLibrariesDict)
        {
            var currentPlexLibraryAccessList = await _dbContext
                .PlexAccountLibraries.Where(x => x.PlexAccountId == plexAccountId && x.PlexServerId == plexServerId)
                .ToListAsync(cancellationToken);

            // When a server is owned by an account, then mark all libraries as owned as well
            var ownedServerIds = await _dbContext
                .PlexAccountServers.Where(x => x.PlexAccountId == plexAccountId && x.IsServerOwned)
                .Select(x => x.PlexServerId)
                .ToListAsync(cancellationToken);

            var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken);
            var rapport = FindOrCreate(plexServerId, plexAccount.DisplayName, plexServerName);

            foreach (var incomingPlexLibrary in incomingPlexLibraries)
            {
                var plexLibraryId = incomingPlexLibrary.Id;

                // Check if this PlexAccount has been associated with the PlexLibrary already
                var hasAccess = currentPlexLibraryAccessList.Any(x =>
                    x.PlexAccountId == plexAccountId
                    && x.PlexLibraryId == plexLibraryId
                    && x.PlexServerId == incomingPlexLibrary.PlexServerId
                );

                if (!hasAccess)
                {
                    // Add entry
                    _log.Here()
                        .Debug(
                            "PlexAccount: {PlexAccountDisplayName} does not have an association with PlexLibrary: {PlexLibraryName} of PlexServer: {PlexServerName} creating one with the authentication token now",
                            plexAccount.DisplayName,
                            incomingPlexLibrary.Name,
                            plexServerName
                        );

                    await _dbContext.PlexAccountLibraries.AddAsync(
                        new PlexAccountLibrary
                        {
                            PlexAccountId = plexAccountId,
                            PlexLibraryId = plexLibraryId,
                            PlexServerId = incomingPlexLibrary.PlexServerId,
                            IsLibraryOwned = ownedServerIds.Contains(incomingPlexLibrary.PlexServerId),
                        },
                        cancellationToken
                    );

                    rapport.AddGranted(plexLibraryId, incomingPlexLibrary.Name);
                }
                else
                {
                    _log.Here()
                        .Debug(
                            "PlexAccount: {PlexAccountDisplayName} already has an association with PlexLibrary: {PlexLibraryName} of PlexServer: {PlexServerName} skipping for now",
                            plexAccount.DisplayName,
                            incomingPlexLibrary.Name,
                            plexServerName
                        );

                    rapport.AddUpdated(plexLibraryId, incomingPlexLibrary.Name);
                }
            }

            // Determine if PlexAccount lost access
            var lostLibraryIds = currentPlexLibraryAccessList
                .Select(x => x.PlexLibraryId)
                .Except(incomingPlexLibraries.Select(x => x.Id))
                .ToList();

            if (lostLibraryIds.Any())
            {
                await _dbContext
                    .PlexAccountLibraries.Where(x =>
                        x.PlexAccountId == plexAccountId
                        && x.PlexServerId == plexServerId
                        && lostLibraryIds.Contains(x.PlexLibraryId)
                    )
                    .ExecuteDeleteAsync(cancellationToken);

                foreach (var lostLibraryId in lostLibraryIds)
                {
                    var libraryName = await _dbContext.GetPlexLibraryNameById(lostLibraryId, CancellationToken.None);
                    rapport.AddRevoked(lostLibraryId, libraryName);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var rapport in _list)
            _log.Here().Information(rapport.ToString());

        return Result.Ok(_list);
    }

    private PlexLibraryAccessRapport FindOrCreate(int plexServerId, string plexAccountName, string plexServerName)
    {
        var x = _list.Find(x => x.PlexServerId == plexServerId);
        if (x is not null)
        {
            return x;
        }

        _list.Add(new PlexLibraryAccessRapport(plexAccountName, plexServerId, plexServerName));
        return _list.Last();
    }
}
