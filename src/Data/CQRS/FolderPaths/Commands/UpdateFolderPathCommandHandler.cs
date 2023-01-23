using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FolderPaths;

public class UpdateFolderPathCommandValidator : AbstractValidator<UpdateFolderPathCommand>
{
    public UpdateFolderPathCommandValidator()
    {
        RuleFor(x => x.FolderPath).NotNull();
        RuleFor(x => x.FolderPath.Id).GreaterThan(0);
        RuleFor(x => x.FolderPath.DisplayName).NotEmpty();
        RuleFor(x => x.FolderPath.FolderType).NotEqual(FolderType.Unknown);
        RuleFor(x => x.FolderPath.MediaType).NotEqual(PlexMediaType.Unknown);
    }
}

public class UpdateFolderPathCommandHandler : BaseHandler, IRequestHandler<UpdateFolderPathCommand, Result<FolderPath>>
{
    public UpdateFolderPathCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<FolderPath>> Handle(UpdateFolderPathCommand command, CancellationToken cancellationToken)
    {
        var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.FolderPath.Id, cancellationToken);

        if (folderPath == null)
            return ResultExtensions.EntityNotFound(nameof(FolderPath), command.FolderPath.Id);

        _dbContext.Entry(folderPath).CurrentValues.SetValues(command.FolderPath);
        await SaveChangesAsync(cancellationToken);

        return Result.Ok(folderPath);
    }
}