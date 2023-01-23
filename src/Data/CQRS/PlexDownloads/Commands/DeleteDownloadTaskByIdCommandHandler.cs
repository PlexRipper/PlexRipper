using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class DeleteDownloadTaskByIdCommandValidator : AbstractValidator<DeleteDownloadTasksByIdCommand>
{
    public DeleteDownloadTaskByIdCommandValidator()
    {
        RuleForEach(x => x.DownloadTaskIds).GreaterThan(0);
    }
}

public class DeleteDownloadTaskByIDHandler : BaseHandler, IRequestHandler<DeleteDownloadTasksByIdCommand, Result<bool>>
{
    public DeleteDownloadTaskByIDHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<bool>> Handle(DeleteDownloadTasksByIdCommand command, CancellationToken cancellationToken)
    {
        var entities = await _dbContext.DownloadTasks.AsTracking().Where(x => command.DownloadTaskIds.Contains(x.Id)).ToListAsync(cancellationToken);
        if (entities == null)
        {
            Log.Warning($"No downloadTasks could be found with ids from [{command.DownloadTaskIds}]");
            return Result.Ok(false);
        }

        _dbContext.DownloadTasks.RemoveRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(true);
    }
}