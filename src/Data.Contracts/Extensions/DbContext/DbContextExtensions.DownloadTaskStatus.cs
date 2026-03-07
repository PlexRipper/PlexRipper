using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;
using Reaparr.Logging;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    /// <summary>
    /// This will determine the download status of the download task and it's children. It will start from the lower nested hierarchy and traverse up to the root to determine the <see cref="DownloadStatus"/>.
    /// </summary>
    /// <param name="dbContext">The <see cref="IReaparrDbContext"/> to extend from. </param>
    /// <param name="key">The <see cref="DownloadTaskKey"/> to traverse from. </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe. </param>
    public static async Task DetermineDownloadStatus(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var parentKey = key;
            while (parentKey is not null)
            {
                switch (parentKey.Type)
                {
                    case DownloadTaskType.Movie:
                    {
                        var childStatuses = await dbContext
                            .DownloadTaskMovieFile.Where(x => x.ParentId == parentKey.Id)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        await dbContext
                            .DownloadTaskMovie.Where(x => x.Id == parentKey.Id && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        parentKey = null;
                        break;
                    }
                    case DownloadTaskType.TvShow:
                    {
                        var childStatuses = await dbContext
                            .DownloadTaskTvShowSeason.Where(x => x.ParentId == parentKey.Id)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        await dbContext
                            .DownloadTaskTvShow.Where(x => x.Id == parentKey.Id && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        parentKey = null;
                        break;
                    }
                    case DownloadTaskType.Season:
                    {
                        var season = await dbContext
                            .DownloadTaskTvShowSeason.Where(x => x.Id == parentKey.Id)
                            .Select(x => new
                            {
                                x.Id,
                                x.ParentId,
                                x.PlexServerId,
                                x.PlexLibraryId,
                            })
                            .FirstOrDefaultAsync(cancellationToken);

                        if (season is null)
                        {
                            parentKey = null;
                            break;
                        }

                        var childStatuses = await dbContext
                            .DownloadTaskTvShowEpisode.Where(x => x.ParentId == season.Id)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        await dbContext
                            .DownloadTaskTvShowSeason.Where(x => x.Id == season.Id && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        parentKey = new DownloadTaskKey
                        {
                            Type = DownloadTaskType.TvShow,
                            Id = season.ParentId,
                            PlexServerId = season.PlexServerId,
                            PlexLibraryId = season.PlexLibraryId,
                        };

                        break;
                    }
                    case DownloadTaskType.Episode:
                    {
                        var episode = await dbContext
                            .DownloadTaskTvShowEpisode.Where(x => x.Id == parentKey.Id)
                            .Select(x => new
                            {
                                x.Id,
                                x.ParentId,
                                x.PlexServerId,
                                x.PlexLibraryId,
                            })
                            .FirstOrDefaultAsync(cancellationToken);

                        if (episode is null)
                        {
                            parentKey = null;
                            break;
                        }

                        var childStatuses = await dbContext
                            .DownloadTaskTvShowEpisodeFile.Where(x => x.ParentId == episode.Id)
                            .Select(x => x.DownloadStatus)
                            .ToListAsync(cancellationToken);
                        var newStatus = DownloadTaskActions.Aggregate(childStatuses);

                        await dbContext
                            .DownloadTaskTvShowEpisode.Where(x => x.Id == episode.Id && x.DownloadStatus != newStatus)
                            .ExecuteUpdateAsync(
                                p => p.SetProperty(x => x.DownloadStatus, newStatus),
                                cancellationToken
                            );

                        parentKey = new DownloadTaskKey
                        {
                            Type = DownloadTaskType.Season,
                            Id = episode.ParentId,
                            PlexServerId = episode.PlexServerId,
                            PlexLibraryId = episode.PlexLibraryId,
                        };

                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the MoveDownloadFileJob
                    case DownloadTaskType.MovieData:
                    case DownloadTaskType.MoviePart:
                    {
                        parentKey = await dbContext
                            // ReSharper disable once AccessToModifiedClosure
                            .DownloadTaskMovieFile.Where(x => x.Id == parentKey.Id)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }

                    // The DownloadStatus here is determined by PlexDownloadClient and the MoveDownloadFileJob
                    case DownloadTaskType.EpisodeData:
                    case DownloadTaskType.EpisodePart:
                    {
                        parentKey = await dbContext
                            // ReSharper disable once AccessToModifiedClosure
                            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == parentKey.Id)
                            .ProjectToParentKey()
                            .FirstOrDefaultAsync(cancellationToken);
                        break;
                    }
                    default:
                        _log.Here()
                            .Error(
                                "DownloadTaskType {DownloadTaskType} is not supported in {DetermineDownloadStatus}",
                                parentKey.Type,
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
}
