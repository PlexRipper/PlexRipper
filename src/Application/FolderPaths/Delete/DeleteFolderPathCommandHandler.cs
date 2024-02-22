using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record DeleteFolderPathCommand(int Id) : IRequest<Result>;

public class DeleteFolderPathValidator : AbstractValidator<DeleteFolderPathCommand>
{
    public DeleteFolderPathValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteFolderPathHandler : IRequestHandler<DeleteFolderPathCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public DeleteFolderPathHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

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