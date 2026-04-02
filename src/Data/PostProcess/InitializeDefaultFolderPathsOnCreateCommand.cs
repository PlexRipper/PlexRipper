using System.IO.Abstractions;

namespace Reaparr.Data;

public record InitializeDefaultFolderPathsOnCreateCommand : ICommand<Result>;

public class InitializeDefaultFolderPathsOnCreateCommandValidator
    : AbstractValidator<InitializeDefaultFolderPathsOnCreateCommand>
{
    public InitializeDefaultFolderPathsOnCreateCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class InitializeDefaultFolderPathsOnCreateCommandHandler
    : ICommandHandler<InitializeDefaultFolderPathsOnCreateCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IPathProvider _pathProvider;
    private readonly IDirectory _directory;

    public InitializeDefaultFolderPathsOnCreateCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IPathProvider pathProvider,
        IDirectory directory
    )
    {
        _log = log.ForContext<InitializeDefaultFolderPathsOnCreateCommandHandler>();
        _dbContext = dbContext;
        _pathProvider = pathProvider;
        _directory = directory;
    }

    public async Task<Result> ExecuteAsync(
        InitializeDefaultFolderPathsOnCreateCommand command,
        CancellationToken cancellationToken
    )
    {
        var defaultPathsById = ReaparrDBContextSeed.GetDefaultFolderPaths();
        var targetIds = defaultPathsById.Select(x => x.Id).ToList();

        var existingPaths = await _dbContext
            .FolderPaths.AsTracking()
            .Where(x => targetIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        _log.Here().Information("Initializing setting the default FolderPath directory paths on database create.");

        var updatedCount = 0;
        foreach (var existingPath in existingPaths)
        {
            var oldPath = existingPath.DirectoryPath;
            var newPath = existingPath.MediaType.ToDefaultDestinationLocation();
            if (oldPath != newPath)
            {
                existingPath.DirectoryPath = newPath;
                _log.Here()
                    .Debug(
                        "Updating default FolderPath for MediaType {MediaType}, from \"{OldPath}\" to New Path: \"{NewPath}\".",
                        existingPath.MediaType,
                        oldPath,
                        newPath
                    );
                updatedCount++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (updatedCount > 0)
        {
            _log.Here()
                .Information(
                    "Initialized default FolderPath directory paths on database create. Updated rows: {UpdatedCount}",
                    updatedCount
                );
        }

        return Result.Ok();
    }
}
