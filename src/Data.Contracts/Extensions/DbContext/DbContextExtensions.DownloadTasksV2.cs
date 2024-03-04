using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
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

    public static async Task<DownloadTaskGeneric?> GetDownloadTaskByKeyQuery(
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
                return downloadTaskMovie.ToGeneric();

            // DownloadTaskType.MovieData
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                var downloadTaskMovieFile = await dbContext.DownloadTaskMovieFile
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DestinationFolder)
                    .Include(x => x.DownloadFolder)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskMovieFile.ToGeneric();

            // DownloadTaskType.TvShow
            case DownloadTaskType.TvShow:
                var downloadTaskTvShow = await dbContext.DownloadTaskTvShow
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShow.ToGeneric();

            // DownloadTaskType.TvShowSeason
            case DownloadTaskType.Season:
                var downloadTaskTvShowSeason = await dbContext.DownloadTaskTvShowSeason
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .ThenInclude(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowSeason.ToGeneric();

            // DownloadTaskType.Episode
            case DownloadTaskType.Episode:
                var downloadTaskTvShowEpisode = await dbContext.DownloadTaskTvShowEpisode
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.Children)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowEpisode.ToGeneric();

            // DownloadTaskType.EpisodeData
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                var downloadTaskTvShowEpisodeFile = await dbContext.DownloadTaskTvShowEpisodeFile
                    .Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .Include(x => x.DestinationFolder)
                    .Include(x => x.DownloadFolder)
                    .Include(x => x.DownloadWorkerTasks)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return downloadTaskTvShowEpisodeFile.ToGeneric();

            default:
                return null;
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetAllDownloadTasksAsync(
        this IPlexRipperDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var downloadTasks = new List<DownloadTaskGeneric>();

        var downloadTasksMovies = await dbContext.DownloadTaskMovie
            .IncludeAll()
            .ToListAsync(cancellationToken);

        var downloadTasksTvShows = await dbContext.DownloadTaskTvShow.IncludeAll()
            .ToListAsync(cancellationToken);

        downloadTasks.AddRange(downloadTasksMovies.Select(x => x.ToGeneric()));
        downloadTasks.AddRange(downloadTasksTvShows.Select(x => x.ToGeneric()));

        return downloadTasks;
    }
}