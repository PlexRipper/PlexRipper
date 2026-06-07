namespace Reaparr.Application;

public record ValidateFolderPathsCommand(PlexMediaType MediaType = PlexMediaType.None) : ICommand<Result>;

public class ValidateFolderPathsCommandValidator : AbstractValidator<ValidateFolderPathsCommand>
{
    public ValidateFolderPathsCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class ValidateFolderPathsCommandHandler : ICommandHandler<ValidateFolderPathsCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IDirectory _directory;

    public ValidateFolderPathsCommandHandler(IReaparrDbContext dbContext, IDirectory directory)
    {
        _dbContext = dbContext;
        _directory = directory;
    }

    public async Task<Result> ExecuteAsync(ValidateFolderPathsCommand command, CancellationToken cancellationToken)
    {
        List<FolderPath> folderPaths;

        if (command.MediaType is PlexMediaType.None or PlexMediaType.Unknown)
            folderPaths = await _dbContext.FolderPaths.ToListAsync(cancellationToken);
        else
        {
            folderPaths = await _dbContext
                .FolderPaths.Where(x => x.MediaType == command.MediaType)
                .ToListAsync(cancellationToken);
        }

        var errors = new List<IError>();
        foreach (var folderPath in folderPaths)
        {
            if (_directory.Exists(folderPath.DirectoryPath))
                continue;

            errors.Add(new Error($"The {folderPath.DisplayName} is not a valid or existing directory"));
        }

        return errors.Count > 0 ? new Result().WithErrors(errors).LogError() : Result.Ok();
    }
}
