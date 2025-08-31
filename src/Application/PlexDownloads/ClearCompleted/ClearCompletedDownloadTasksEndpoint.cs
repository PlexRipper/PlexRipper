using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Will clear any completed <see cref="DownloadTaskGeneric"/> from the database.
/// </summary>
/// <returns>Is successful.</returns>
public class ClearCompletedDownloadTasksEndpoint : BaseEndpoint<List<Guid>, ResultDTO<CountResponseDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear";

    public ClearCompletedDownloadTasksEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Post(EndpointPath);

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<CountResponseDTO>)));
    }

    public override async Task HandleAsync(List<Guid> downloadTaskIds, CancellationToken ct)
    {
        var hasDownloadTaskIds = downloadTaskIds.Any();

        int totalRowsDeleted;
        if (hasDownloadTaskIds)
            totalRowsDeleted = await ClearByGuids(downloadTaskIds, ct);
        else
            totalRowsDeleted = await ClearAllCompleted(ct);

        await SendFluentResult(Result.Ok(new CountResponseDTO(totalRowsDeleted)), ct);
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

    public async Task<int> ClearAllCompleted(CancellationToken ct)
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
