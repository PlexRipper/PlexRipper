using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FolderPaths;

public class DeleteFolderPathValidator : AbstractValidator<DeleteFolderPathCommand>
{
    public DeleteFolderPathValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteFolderPathHandler : BaseHandler, IRequestHandler<DeleteFolderPathCommand, Result>
{
    public DeleteFolderPathHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(DeleteFolderPathCommand command, CancellationToken cancellationToken)
    {
        var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (folderPath == null)
            return ResultExtensions.EntityNotFound(nameof(FolderPath), command.Id);

        _dbContext.FolderPaths.Remove(folderPath);
        await _dbContext.SaveChangesAsync(cancellationToken);
        Log.Debug($"Deleted {nameof(FolderPath)} with Id: {command.Id} from the database");

        return Result.Ok();
    }
}