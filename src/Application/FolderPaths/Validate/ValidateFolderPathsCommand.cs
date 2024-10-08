using Data.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record ValidateFolderPathsCommand(PlexMediaType MediaType = PlexMediaType.None) : IRequest<Result>;

public class ValidateFolderPathsValidator : AbstractValidator<ValidateFolderPathsCommand>
{
    public ValidateFolderPathsValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class ValidateFolderPathsHandler : IRequestHandler<ValidateFolderPathsCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDirectorySystem _directorySystem;

    public ValidateFolderPathsHandler(IPlexRipperDbContext dbContext, IDirectorySystem directorySystem)
    {
        _dbContext = dbContext;
        _directorySystem = directorySystem;
    }

    public async Task<Result> Handle(ValidateFolderPathsCommand command, CancellationToken cancellationToken)
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
            var folderPathExitsResult = _directorySystem.Exists(folderPath.DirectoryPath);
            if (folderPathExitsResult.IsFailed)
            {
                errors.AddRange(folderPathExitsResult.Errors);
                continue;
            }

            if (folderPath.MediaType == command.MediaType && !folderPathExitsResult.Value)
                errors.Add(new Error($"The {folderPath.DisplayName} is not a valid or existing directory"));
        }

        return errors.Count > 0 ? new Result().WithErrors(errors).LogError() : Result.Ok();
    }
}