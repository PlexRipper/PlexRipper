using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Will clear any completed <see cref="DownloadTask"/> from the database.
/// </summary>
/// <param name="DownloadTaskIds"> The list of <see cref="DownloadTask"/> id's to delete.</param>
/// <returns>Is successful.</returns>
public record ClearCompletedDownloadTasksCommand(List<int> DownloadTaskIds) : IRequest<Result>;

public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand> { }

public class ClearCompletedDownloadTasksHandler :
    IRequestHandler<ClearCompletedDownloadTasksCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;

    public ClearCompletedDownloadTasksHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ClearCompletedDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        if (command.DownloadTaskIds != null && command.DownloadTaskIds.Any())
        {
            var downloadTasks = await _dbContext.DownloadTasks
                .Where(x => command.DownloadTaskIds.Contains(x.Id) && x.DownloadStatus == DownloadStatus.Completed)
                .ToListAsync(cancellationToken);
            _dbContext.DownloadTasks.RemoveRange(downloadTasks);
        }
        else
        {
            var downloadTasks = await _dbContext.DownloadTasks.AsTracking()
                .Where(x => x.DownloadStatus == DownloadStatus.Completed)
                .ToListAsync(cancellationToken);
            _dbContext.DownloadTasks.RemoveRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}