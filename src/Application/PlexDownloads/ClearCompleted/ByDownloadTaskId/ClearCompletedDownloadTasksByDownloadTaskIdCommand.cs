using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class ClearCompletedDownloadTasksByDownloadTaskIdCommandValidator
    : Validator<ClearCompletedDownloadTasksByDownloadTaskIdCommand>
{
    public ClearCompletedDownloadTasksByDownloadTaskIdCommandValidator()
    {
        RuleFor(x => x.DownloadTaskIds).NotEmpty();
    }
}

public class ClearCompletedDownloadTasksByDownloadTaskIdCommandHandler
    : ICommandHandler<ClearCompletedDownloadTasksByDownloadTaskIdCommand, Result<int>>
{
    private readonly IReaparrDbContext _dbContext;

    public ClearCompletedDownloadTasksByDownloadTaskIdCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> ExecuteAsync(
        ClearCompletedDownloadTasksByDownloadTaskIdCommand request,
        CancellationToken ct
    )
    {
        var totalRowsDeleted = 0;
        var affectedRootIds = await _dbContext.GetAffectedRootDownloadTaskIdsAsync(request.DownloadTaskIds, ct);

        foreach (var downloadTaskId in request.DownloadTaskIds)
        {
            var rowsDeleted = await _dbContext
                .DownloadTaskMovie.Where(x => x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed)
                .ExecuteDeleteAsync(ct);

            if (rowsDeleted > 0)
            {
                totalRowsDeleted += rowsDeleted;
                continue;
            }

            rowsDeleted = await _dbContext
                .DownloadTaskMovieFile.Where(x =>
                    x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed
                )
                .ExecuteDeleteAsync(ct);

            if (rowsDeleted > 0)
            {
                totalRowsDeleted += rowsDeleted;
                continue;
            }

            rowsDeleted = await _dbContext
                .DownloadTaskTvShow.Where(x => x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed)
                .ExecuteDeleteAsync(ct);

            if (rowsDeleted > 0)
            {
                totalRowsDeleted += rowsDeleted;
                continue;
            }

            rowsDeleted = await _dbContext
                .DownloadTaskTvShowSeason.Where(x =>
                    x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed
                )
                .ExecuteDeleteAsync(ct);

            if (rowsDeleted > 0)
            {
                totalRowsDeleted += rowsDeleted;
                continue;
            }

            rowsDeleted = await _dbContext
                .DownloadTaskTvShowEpisode.Where(x =>
                    x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed
                )
                .ExecuteDeleteAsync(ct);

            if (rowsDeleted > 0)
            {
                totalRowsDeleted += rowsDeleted;
                continue;
            }

            rowsDeleted = await _dbContext
                .DownloadTaskTvShowEpisodeFile.Where(x =>
                    x.Id == downloadTaskId && x.DownloadStatus == DownloadStatus.Completed
                )
                .ExecuteDeleteAsync(ct);

            totalRowsDeleted += rowsDeleted;
        }

        totalRowsDeleted += await _dbContext.DeleteOrphanedParentTasksByRootIdsAsync(affectedRootIds, ct);

        return Result.Ok(totalRowsDeleted);
    }
}
