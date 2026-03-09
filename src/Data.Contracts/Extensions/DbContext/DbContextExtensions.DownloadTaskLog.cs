using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<List<DownloadTaskLogBase>>> GetDownloadTaskLogsAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey downloadTaskKey,
        int? sinceId,
        int? take,
        CancellationToken ct
    ) =>
        downloadTaskKey.Type switch
        {
            DownloadTaskType.Movie => await Result.Try(async Task<List<DownloadTaskLogBase>> () =>
                await dbContext
                    .DownloadTaskMovieFileLogs.Where(x => x.DownloadTaskMovieId == downloadTaskKey.Id)
                    .ApplyWhere(sinceId != null, x => x.Id > sinceId)
                    .OrderBy(x => x.Id)
                    .Select(x => (DownloadTaskLogBase)x)
                    .ApplyTake(take ?? 0)
                    .ToListAsync(ct)
            ),
            DownloadTaskType.MoviePart or DownloadTaskType.MovieData => await Result.Try(
                async Task<List<DownloadTaskLogBase>> () =>
                    await dbContext
                        .DownloadTaskMovieFileLogs.Where(x => x.DownloadTaskFileId == downloadTaskKey.Id)
                        .Where(x => sinceId == null || x.Id > sinceId)
                        .OrderBy(x => x.Id)
                        .Select(x => (DownloadTaskLogBase)x)
                        .ApplyTake(take ?? 0)
                        .ToListAsync(ct)
            ),
            DownloadTaskType.TvShow => await Result.Try(async Task<List<DownloadTaskLogBase>> () =>
                await dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowId == downloadTaskKey.Id)
                    .ApplyWhere(sinceId != null, x => x.Id > sinceId)
                    .OrderBy(x => x.Id)
                    .Select(x => (DownloadTaskLogBase)x)
                    .ApplyTake(take ?? 0)
                    .ToListAsync(ct)
            ),
            DownloadTaskType.Season => await Result.Try(async Task<List<DownloadTaskLogBase>> () =>
                await dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowSeasonId == downloadTaskKey.Id)
                    .ApplyWhere(sinceId != null, x => x.Id > sinceId)
                    .OrderBy(x => x.Id)
                    .Select(x => (DownloadTaskLogBase)x)
                    .ApplyTake(take ?? 0)
                    .ToListAsync(ct)
            ),
            DownloadTaskType.Episode => await Result.Try(async Task<List<DownloadTaskLogBase>> () =>
                await dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowEpisodeId == downloadTaskKey.Id)
                    .ApplyWhere(sinceId != null, x => x.Id > sinceId)
                    .OrderBy(x => x.Id)
                    .Select(x => (DownloadTaskLogBase)x)
                    .ApplyTake(take ?? 0)
                    .ToListAsync(ct)
            ),
            DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart => await Result.Try(
                async Task<List<DownloadTaskLogBase>> () =>
                    await dbContext
                        .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskFileId == downloadTaskKey.Id)
                        .ApplyWhere(sinceId != null, x => x.Id > sinceId)
                        .OrderBy(x => x.Id)
                        .Select(x => (DownloadTaskLogBase)x)
                        .ApplyTake(take ?? 0)
                        .ToListAsync(ct)
            ),
            _ => Result.Fail($"DownloadTaskLog of type {downloadTaskKey.Type} not implemented").LogError(),
        };

    public static async Task<Result<int>> DeleteDownloadTaskLogsAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey downloadTaskKey,
        CancellationToken ct
    ) =>
        downloadTaskKey.Type switch
        {
            DownloadTaskType.Movie => await Result.Try(() =>
                dbContext
                    .DownloadTaskMovieFileLogs.Where(x => x.DownloadTaskMovieId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            DownloadTaskType.MoviePart or DownloadTaskType.MovieData => await Result.Try(() =>
                dbContext
                    .DownloadTaskMovieFileLogs.Where(x => x.DownloadTaskFileId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            DownloadTaskType.TvShow => await Result.Try(() =>
                dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            DownloadTaskType.Season => await Result.Try(() =>
                dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowSeasonId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            DownloadTaskType.Episode => await Result.Try(() =>
                dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskTvShowEpisodeId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart => await Result.Try(() =>
                dbContext
                    .DownloadTaskTvShowEpisodeFileLogs.Where(x => x.DownloadTaskFileId == downloadTaskKey.Id)
                    .ExecuteDeleteAsync(ct)
            ),
            _ => Result.Fail($"DownloadTaskLog of type {downloadTaskKey.Type} not implemented").LogError(),
        };

    public static async Task CreateDownloadClientLog(
        this IReaparrDbContext dbContext,
        DownloadTaskKey downloadTaskKey,
        NotificationLevel logLevel,
        DownloadStatus status,
        string message
    )
    {
        if (downloadTaskKey.Type is DownloadTaskType.MovieData or DownloadTaskType.MoviePart)
        {
            var parentId = await dbContext
                .DownloadTaskMovieFile.Where(x => x.Id == downloadTaskKey.Id)
                .Select(x => x.ParentId)
                .FirstOrDefaultAsync();
            dbContext.DownloadTaskMovieFileLogs.Add(
                new DownloadTaskMovieFileLog
                {
                    Message = message,
                    LogLevel = logLevel,
                    Status = status,
                    DownloadTaskFileId = downloadTaskKey.Id,
                    DownloadTaskMovieId = parentId,
                    CreatedAt = DateTime.UtcNow,
                }
            );
        }

        if (downloadTaskKey.Type is DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart)
        {
            var ids = await dbContext
                .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadTaskKey.Id)
                .Select(x => new
                {
                    EpisodeId = x.ParentId,
                    SeasonId = x.Parent!.ParentId,
                    TvShowId = x.Parent.Parent!.ParentId,
                })
                .FirstOrDefaultAsync();

            dbContext.DownloadTaskTvShowEpisodeFileLogs.Add(
                new DownloadTaskTvShowEpisodeFileLog
                {
                    Message = message,
                    LogLevel = logLevel,
                    Status = status,
                    DownloadTaskFileId = downloadTaskKey.Id,
                    DownloadTaskTvShowEpisodeId = ids?.EpisodeId ?? Guid.Empty,
                    DownloadTaskTvShowSeasonId = ids?.SeasonId ?? Guid.Empty,
                    DownloadTaskTvShowId = ids?.TvShowId ?? Guid.Empty,
                    CreatedAt = DateTime.UtcNow,
                }
            );
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public static async Task CreateDownloadClientLogs(
        this IReaparrDbContext dbContext,
        List<DownloadTaskMovieFileLog> logs
    )
    {
        dbContext.DownloadTaskMovieFileLogs.AddRange(logs);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public static async Task CreateDownloadClientLogs(
        this IReaparrDbContext dbContext,
        List<DownloadTaskTvShowEpisodeFileLog> logs
    )
    {
        dbContext.DownloadTaskTvShowEpisodeFileLogs.AddRange(logs);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
