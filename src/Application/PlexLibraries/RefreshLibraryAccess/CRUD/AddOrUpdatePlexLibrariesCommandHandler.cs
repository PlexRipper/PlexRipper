using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record AddOrUpdatePlexLibrariesCommand : IRequest<Result>
{
    public int PlexAccountId { get; }

    public List<PlexLibrary> PlexLibraries { get; }

    public AddOrUpdatePlexLibrariesCommand(int plexAccountId, List<PlexLibrary> plexLibraries)
    {
        PlexAccountId = plexAccountId;
        PlexLibraries = plexLibraries;
    }
}

public class AddOrUpdatePlexLibrariesValidator : AbstractValidator<AddOrUpdatePlexLibrariesCommand>
{
    #region Constructors

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

    #endregion
}

public class AddOrUpdatePlexLibrariesCommandHandler : IRequestHandler<AddOrUpdatePlexLibrariesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    #region Constructors

    public AddOrUpdatePlexLibrariesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    #endregion

    #region Methods

    #region Public

    public async Task<Result> Handle(AddOrUpdatePlexLibrariesCommand command, CancellationToken cancellationToken)
    {
        var plexLibraries = command.PlexLibraries;
        var plexAccountId = command.PlexAccountId;

        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);

        if (plexAccount is null)
            return ResultExtensions.IsNull(nameof(plexAccount));

        foreach (var plexLibrary in plexLibraries)
        {
            var plexLibraryDB = await _dbContext.PlexLibraries.FirstOrDefaultAsync(
                x => x.PlexServerId == plexLibrary.PlexServerId && x.Uuid == plexLibrary.Uuid,
                cancellationToken
            );

            if (plexLibraryDB is null)
            {
                _log.Debug("Adding PlexLibrary {PlexLibraryName} to the database", plexLibrary.Title);
                await _dbContext.PlexLibraries.AddAsync(plexLibrary, cancellationToken);
            }
            else
            {
                _log.Debug(
                    "Updating PlexLibrary {PlexLibraryName} with id: {PlexLibraryId} in the database",
                    plexLibrary.Title,
                    plexLibrary.Id
                );

                plexLibrary.Id = plexLibraryDB.Id;
                _dbContext.PlexLibraries.Update(plexLibrary);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Add or update the PlexAccount, PlexServer and PlexLibrary relationships
        _log.Information(
            "Adding or updating the PlexAccount ({PlexAccountDisplayName}) association with PlexLibraries now",
            plexAccount.DisplayName
        );

        var plexAccountLibraries = await _dbContext
            .PlexAccountLibraries.Where(x => x.PlexAccountId == plexAccountId)
            .ToListAsync(cancellationToken);

        foreach (var plexLibrary in plexLibraries)
        {
            // Check if this PlexAccount has been associated with the PlexLibrary already
            var plexAccountLibrary = plexAccountLibraries.Find(x =>
                x.PlexAccountId == plexAccountId
                && x.PlexLibraryId == plexLibrary.Id
                && x.PlexServerId == plexLibrary.PlexServerId
            );

            var plexServerName = await _dbContext.GetPlexServerNameById(plexLibrary.PlexServerId, cancellationToken);
            if (plexAccountLibrary is null)
            {
                // Add entry
                _log.Here()
                    .Debug(
                        "PlexAccount: {PlexAccountDisplayName} does not have an association with PlexLibrary: {PlexLibraryName} of PlexServer: {PlexServerName} creating one with the authentication token now",
                        plexAccount.DisplayName,
                        plexLibrary.Name,
                        plexServerName
                    );

                await _dbContext.PlexAccountLibraries.AddAsync(
                    new PlexAccountLibrary
                    {
                        PlexAccountId = plexAccountId,
                        PlexLibraryId = plexLibrary.Id,
                        PlexServerId = plexLibrary.PlexServerId,
                    },
                    cancellationToken
                );
            }
            else
            {
                _log.Here()
                    .Debug(
                        "PlexAccount: {PlexAccountDisplayName} already has an association with PlexLibrary: {PlexLibraryName} of PlexServer: {PlexServerName} skipping for now",
                        plexAccount.DisplayName,
                        plexLibrary.Name,
                        plexServerName
                    );
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    #endregion

    #endregion
}
