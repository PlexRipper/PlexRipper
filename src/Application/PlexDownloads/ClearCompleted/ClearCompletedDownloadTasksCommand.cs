using FastEndpoints;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Domain;

namespace Reaparr.Application;

public class ClearCompletedDownloadTasksCommandHandler
    : ICommandHandler<ClearCompletedDownloadTasksCommand, Result<int>>
{
    private readonly IReaparrDbContext _dbContext;

    public ClearCompletedDownloadTasksCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> ExecuteAsync(ClearCompletedDownloadTasksCommand request, CancellationToken ct)
    {
        var downloadTaskIds = request.DownloadTaskIds ?? [];
        var hasDownloadTaskIds = downloadTaskIds.Count > 0;

        var totalRowsDeleted = hasDownloadTaskIds
            ? await ClearByGuids(downloadTaskIds, ct)
            : await ClearAllCompleted(ct);

        return Result.Ok(totalRowsDeleted);
    }

    private async Task<int> ClearByGuids(List<Guid> downloadTaskIds, CancellationToken ct)
    {
        var totalRowsDeleted = 0;

        foreach (var downloadTaskId in downloadTaskIds)
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

        return totalRowsDeleted;
    }

    private async Task<int> ClearAllCompleted(CancellationToken ct)
    {
        var totalRowsDeleted = 0;

        totalRowsDeleted += await _dbContext
            .DownloadTaskMovie.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await _dbContext
            .DownloadTaskMovieFile.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await _dbContext
            .DownloadTaskTvShow.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await _dbContext
            .DownloadTaskTvShowSeason.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await _dbContext
            .DownloadTaskTvShowEpisode.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.DownloadStatus == DownloadStatus.Completed)
            .ExecuteDeleteAsync(ct);

        return totalRowsDeleted;
    }
}
