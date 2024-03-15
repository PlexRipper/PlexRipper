using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<string>> GetDownloadUrl(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        string fileLocationUrl,
        CancellationToken cancellationToken = default)
    {
        var plexServerConnectionResult = await dbContext.GetValidPlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Error("Could not find a valid token for server {ServerName}", plexServer.Name);
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }

    public static async Task<DownloadTaskType> GetDownloadTaskTypeAsync(
        this IPlexRipperDbContext dbContext,
        Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return DownloadTaskType.None;

        if (await dbContext.DownloadTaskTvShow.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.TvShow;

        if (await dbContext.DownloadTaskMovie.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Movie;

        if (await dbContext.DownloadTaskTvShowSeason.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Season;

        if (await dbContext.DownloadTaskTvShowEpisode.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.Episode;

        if (await dbContext.DownloadTaskTvShowEpisodeFile.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.EpisodeData;

        if (await dbContext.DownloadTaskMovieFile.AnyAsync(x => x.Id == guid, cancellationToken))
            return DownloadTaskType.MovieData;

        return DownloadTaskType.None;
    }

    public static Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default) => dbContext.GetDownloadTaskAsync(key.Id, key.Type, cancellationToken);

    public static async Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IPlexRipperDbContext dbContext,
        Guid id,
        DownloadTaskType type = DownloadTaskType.None,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        if (type == DownloadTaskType.None)
            type = await dbContext.GetDownloadTaskTypeAsync(id, cancellationToken);

        switch (type)
        {
            // DownloadTaskType.Movie
            case DownloadTaskType.Movie:
                var downloadTaskMovie = await dbContext.DownloadTaskMovie
                    .IncludeAll()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskMovie?.ToGeneric() ?? null;

            // DownloadTaskType.MovieData
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                var downloadTaskMovieFile = await dbContext.DownloadTaskMovieFile
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskMovieFile?.ToGeneric() ?? null;

            // DownloadTaskType.TvShow
            case DownloadTaskType.TvShow:
                var downloadTaskTvShow = await dbContext.DownloadTaskTvShow
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShow?.ToGeneric() ?? null;

            // DownloadTaskType.TvShowSeason
            case DownloadTaskType.Season:
                var downloadTaskTvShowSeason = await dbContext.DownloadTaskTvShowSeason
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowSeason?.ToGeneric() ?? null;

            // DownloadTaskType.Episode
            case DownloadTaskType.Episode:
                var downloadTaskTvShowEpisode = await dbContext.DownloadTaskTvShowEpisode
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowEpisode?.ToGeneric() ?? null;

            // DownloadTaskType.EpisodeData
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                var downloadTaskTvShowEpisodeFile = await dbContext.DownloadTaskTvShowEpisodeFile
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowEpisodeFile?.ToGeneric() ?? null;

            default:
                return null;
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetAllDownloadTasksAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId = 0,
        bool asTracking = false,
        CancellationToken cancellationToken = default)
    {
        var downloadTasks = new List<DownloadTaskGeneric>();

        var downloadTasksMovies = await dbContext.DownloadTaskMovie
            .AsTracking(asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        var downloadTasksTvShows = await dbContext.DownloadTaskTvShow
            .AsTracking(asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        downloadTasks.AddRange(downloadTasksMovies.Select(x => x.ToGeneric()));
        downloadTasks.AddRange(downloadTasksTvShows.Select(x => x.ToGeneric()));

        // Sort by CreatedAt
        downloadTasks.Sort((x, y) => DateTime.Compare(x.CreatedAt, y.CreatedAt));

        return downloadTasks;
    }

    public static Task<DownloadTaskTvShow?> GetDownloadTaskTvShowByMediaKeyQuery(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int mediaKey,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DownloadTaskTvShow
            .IncludeAll()
            .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);
    }

    public static Task<DownloadTaskTvShowSeason?> GetDownloadTaskTvShowSeasonByMediaKeyQuery(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int mediaKey,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DownloadTaskTvShowSeason
            .IncludeAll()
            .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);
    }

    public static Task<DownloadTaskTvShowEpisode?> GetDownloadTaskTvShowEpisodeByMediaKeyQuery(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int mediaKey,
        CancellationToken cancellationToken = default)
    {
        return dbContext.DownloadTaskTvShowEpisode
            .IncludeAll()
            .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && x.Key == mediaKey, cancellationToken);
    }

    public static async Task UpdateDownloadProgress(
        this IPlexRipperDbContext dbContext,
        DownloadTaskKey key,
        IDownloadTaskProgress progress,
        CancellationToken cancellationToken = default)
    {
        switch (key.Type)
        {
            case DownloadTaskType.Movie:
                await dbContext.DownloadTaskMovie.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            case DownloadTaskType.MovieData:
                await dbContext.DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            case DownloadTaskType.TvShow:
                await dbContext.DownloadTaskTvShow.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            case DownloadTaskType.Season:
                await dbContext.DownloadTaskTvShowSeason.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            case DownloadTaskType.Episode:
                await dbContext.DownloadTaskTvShowEpisode.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext.DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p
                        .SetProperty(x => x.Percentage, progress.Percentage)
                        .SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                        .SetProperty(x => x.DataReceived, progress.DataReceived)
                        .SetProperty(x => x.DataTotal, progress.DataTotal), cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}