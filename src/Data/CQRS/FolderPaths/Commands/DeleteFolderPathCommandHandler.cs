using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
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
    public DeleteFolderPathHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(DeleteFolderPathCommand command, CancellationToken cancellationToken)
    {
        var folderPath = await _dbContext.FolderPaths.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (folderPath == null)
            return ResultExtensions.EntityNotFound(nameof(FolderPath), command.Id);

        _dbContext.FolderPaths.Remove(folderPath);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _log.Debug("Deleted {FolderPathName} with Id: {CommandId} from the database", nameof(FolderPath), command.Id);

        return Result.Ok();
    }
}