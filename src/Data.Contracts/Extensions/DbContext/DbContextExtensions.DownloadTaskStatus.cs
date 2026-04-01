namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    /// <summary>
    /// This will determine the download status of the download task and it's children. It will start from the lower nested hierarchy and traverse up to the root to determine the <see cref="DownloadStatus"/>.
    /// </summary>
    /// <param name="dbContext">The <see cref="IReaparrDbContext"/> to extend from. </param>
    /// <param name="key">The <see cref="DownloadTaskKey"/> to traverse from. </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe. </param>
    public static async Task<List<DownloadTaskKey>> DetermineDownloadStatus(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        var changedKeys = new List<DownloadTaskKey>();
        var serverId = key.PlexServerId;
        var libraryId = key.PlexLibraryId;

        try
        {
            var parentKey = key;
            while (parentKey is not null)
            {
                var currentParentKey = parentKey;

                switch (currentParentKey.Type)
                {
                    case DownloadTaskType.Movie:
                    {
                        var currentParentId = currentParentKey.Id;

                        var childStatuses = await dbContext
                            .DownloadTaskMovieFile.Where(x => x.ParentId == currentParentId)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        var changedCount = await dbContext
                            .DownloadTaskMovie.Where(x => x.Id == currentParentId && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        if (changedCount > 0)
                            changedKeys.Add(currentParentKey);

                        parentKey = null;
                        break;
                    }
                    case DownloadTaskType.TvShow:
                    {
                        var currentParentId = currentParentKey.Id;

                        var childStatuses = await dbContext
                            .DownloadTaskTvShowSeason.Where(x => x.ParentId == currentParentId)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        var changedCount = await dbContext
                            .DownloadTaskTvShow.Where(x => x.Id == currentParentId && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        if (changedCount > 0)
                            changedKeys.Add(currentParentKey);

                        parentKey = null;
                        break;
                    }
                    case DownloadTaskType.Season:
                    {
                        var currentParentId = currentParentKey.Id;

                        var showId = await dbContext
                            .DownloadTaskTvShowSeason.Where(x => x.Id == currentParentId)
                            .Select(x => (Guid?)x.ParentId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (showId is null)
                        {
                            parentKey = null;
                            break;
                        }

                        var childStatuses = await dbContext
                            .DownloadTaskTvShowEpisode.Where(x => x.ParentId == currentParentId)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        var changedCount = await dbContext
                            .DownloadTaskTvShowSeason.Where(x =>
                                x.Id == currentParentId && x.DownloadStatus != newStatus
                            )
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        if (changedCount > 0)
                            changedKeys.Add(currentParentKey);

                        parentKey = new DownloadTaskKey
                        {
                            Type = DownloadTaskType.TvShow,
                            Id = showId.Value,
                            PlexServerId = serverId,
                            PlexLibraryId = libraryId,
                        };

                        break;
                    }
                    case DownloadTaskType.Episode:
                    {
                        var currentParentId = currentParentKey.Id;

                        var seasonId = await dbContext
                            .DownloadTaskTvShowEpisode.Where(x => x.Id == currentParentId)
                            .Select(x => (Guid?)x.ParentId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (seasonId is null)
                        {
                            parentKey = null;
                            break;
                        }

                        var childStatuses = await dbContext
                            .DownloadTaskTvShowEpisodeFile.Where(x => x.ParentId == currentParentId)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        var changedCount = await dbContext
                            .DownloadTaskTvShowEpisode.Where(x =>
                                x.Id == currentParentId && x.DownloadStatus != newStatus
                            )
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        if (changedCount > 0)
                            changedKeys.Add(currentParentKey);

                        parentKey = new DownloadTaskKey
                        {
                            Type = DownloadTaskType.Season,
                            Id = seasonId.Value,
                            PlexServerId = serverId,
                            PlexLibraryId = libraryId,
                        };

                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the MoveDownloadFileJob
                    case DownloadTaskType.MovieData:
                    case DownloadTaskType.MoviePart:
                    {
                        var movieDataParentId = currentParentKey.Id;

                        parentKey = await dbContext
                            .DownloadTaskMovieFile.Where(x => x.Id == movieDataParentId)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the MoveDownloadFileJob
                    case DownloadTaskType.EpisodeData:
                    case DownloadTaskType.EpisodePart:
                    {
                        var episodeDataParentId = currentParentKey.Id;

                        parentKey = await dbContext
                            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeDataParentId)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }
                    default:
                        _log.Here()
                            .Error(
                                "DownloadTaskType {DownloadTaskType} is not supported in {DetermineDownloadStatus}",
                                currentParentKey.Type,
                                nameof(DetermineDownloadStatus)
                            );
                        parentKey = null;
                        break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Here().ErrorResult(ex);
            throw;
        }

        return changedKeys;
    }

    /// <summary>
    /// Sets the <see cref="DownloadStatus"/> of the download task but not its children or parent and immediately saves the changes to the database.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IReaparrDbContext"/> to extend from. </param>
    /// <param name="key"> The <see cref="DownloadTaskKey"/> of the download task to update. </param>
    /// <param name="status"> The <see cref="DownloadStatus"/> to set. </param>
    public static async Task SetDownloadStatus(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        DownloadStatus status
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.Movie:
                await dbContext
                    .DownloadTaskMovie.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.MovieData:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.TvShow:
                await dbContext
                    .DownloadTaskTvShow.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Season:
                await dbContext
                    .DownloadTaskTvShowSeason.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.Episode:
                await dbContext
                    .DownloadTaskTvShowEpisode.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, status));
                break;
            default:
                throw new ArgumentOutOfRangeException($"{key.Type} is not supported in {nameof(SetDownloadStatus)}");
        }
    }

    public static async Task<DownloadStatus?> GetDownloadStatusAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.Movie:
                return await dbContext
                    .DownloadTaskMovie.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                return await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.TvShow:
                return await dbContext
                    .DownloadTaskTvShow.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.Season:
                return await dbContext
                    .DownloadTaskTvShowSeason.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.Episode:
                return await dbContext
                    .DownloadTaskTvShowEpisode.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                return await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .Select(x => (DownloadStatus?)x.DownloadStatus)
                    .FirstOrDefaultAsync(cancellationToken);
            default:
                return null;
        }
    }
}
