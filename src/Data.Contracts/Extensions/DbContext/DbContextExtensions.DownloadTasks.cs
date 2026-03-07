using FluentResults;
using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;
using Reaparr.Logging;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<string>> GetDownloadUrl(
        this IReaparrDbContext dbContext,
        int plexServerId,
        string fileLocationUrl,
        CancellationToken cancellationToken = default
    )
    {
        var plexServerConnectionResult = await dbContext.ChoosePlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult().LogError();

        var plexServerConnection = plexServerConnectionResult.Value;
        var plexServer = plexServerConnection.PlexServer;

        var tokenResult = await dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _log.Here().Error("Could not find a valid token for server {ServerName}", plexServer?.Name ?? "Unknown");
            return tokenResult.ToResult();
        }

        var downloadUrl = plexServerConnection.GetDownloadUrl(fileLocationUrl, tokenResult.Value);
        return Result.Ok(downloadUrl);
    }

    public static async Task<DownloadTaskKey?> GetDownloadTaskKeyAsync(
        this IReaparrDbContext dbContext,
        Guid guid,
        CancellationToken cancellationToken = default
    )
    {
        if (guid == Guid.Empty)
            return null;

        var queries = new List<IQueryable<DownloadTaskKey>>
        {
            dbContext.DownloadTaskTvShow.ProjectToKey(),
            dbContext.DownloadTaskTvShowSeason.ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisode.ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisodeFile.ProjectToKey(),
            dbContext.DownloadTaskMovie.ProjectToKey(),
            dbContext.DownloadTaskMovieFile.ProjectToKey(),
        };

        foreach (var query in queries)
        {
            var downloadTaskKey = await query.FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
            if (downloadTaskKey is not null)
                return downloadTaskKey;
        }

        return null;
    }

    public static async Task<DownloadTaskType> GetDownloadTaskTypeAsync(
        this IReaparrDbContext dbContext,
        Guid guid,
        CancellationToken cancellationToken = default
    )
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

    /// <summary>
    /// Retrieves a <see cref="DownloadTaskGeneric"/> from the database based on the <paramref name="key"/> with all its children and related entities.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IReaparrDbContext"/> to query. </param>
    /// <param name="key"> The <see cref="DownloadTaskKey"/> to retrieve the <see cref="DownloadTaskGeneric"/> by. </param>
    /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
    /// <returns> The <see cref="DownloadTaskGeneric"/> if found, otherwise null. </returns>
    public static Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    ) => dbContext.GetDownloadTaskAsync(key.Id, key.Type, cancellationToken);

    /// <summary>
    /// Retrieves a <see cref="DownloadTaskGeneric"/> from the database based on the <paramref name="id"/> and <paramref name="type"/> with all its children and related entities.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IReaparrDbContext"/> to query. </param>
    /// <param name="id"> The id of the <see cref="DownloadTaskGeneric"/> to retrieve. </param>
    /// <param name="type"> The type of the root <see cref="DownloadTaskGeneric"/> to retrieve. </param>
    /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
    /// <returns> The <see cref="DownloadTaskGeneric"/> if found, otherwise null. </returns>
    public static async Task<DownloadTaskGeneric?> GetDownloadTaskAsync(
        this IReaparrDbContext dbContext,
        Guid id,
        DownloadTaskType type = DownloadTaskType.None,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (id == Guid.Empty)
                return null;

            if (type == DownloadTaskType.None)
                type = await dbContext.GetDownloadTaskTypeAsync(id, cancellationToken);

            switch (type)
            {
                // DownloadTaskType.Movie
                case DownloadTaskType.Movie:
                    var downloadTaskMovie = await dbContext
                        .DownloadTaskMovie.IncludeAll()
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskMovie?.ToGeneric() ?? null;

                // DownloadTaskType.MovieData
                case DownloadTaskType.MovieData:
                case DownloadTaskType.MoviePart:
                    var downloadTaskMovieFile = await dbContext
                        .DownloadTaskMovieFile.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskMovieFile?.ToGeneric() ?? null;

                // DownloadTaskType.TvShow
                case DownloadTaskType.TvShow:
                    var downloadTaskTvShow = await dbContext
                        .DownloadTaskTvShow.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                            .ThenInclude(x => x.Children)
                                .ThenInclude(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShow?.ToGeneric() ?? null;

                // DownloadTaskType.TvShowSeason
                case DownloadTaskType.Season:
                    var downloadTaskTvShowSeason = await dbContext
                        .DownloadTaskTvShowSeason.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                            .ThenInclude(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowSeason?.ToGeneric() ?? null;

                // DownloadTaskType.Episode
                case DownloadTaskType.Episode:
                    var downloadTaskTvShowEpisode = await dbContext
                        .DownloadTaskTvShowEpisode.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .Include(x => x.Children)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowEpisode?.ToGeneric() ?? null;

                // DownloadTaskType.EpisodeData
                case DownloadTaskType.EpisodeData:
                case DownloadTaskType.EpisodePart:
                    var downloadTaskTvShowEpisodeFile = await dbContext
                        .DownloadTaskTvShowEpisodeFile.Include(x => x.PlexServer)
                        .Include(x => x.PlexLibrary)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                    return downloadTaskTvShowEpisodeFile?.ToGeneric() ?? null;

                default:
                    return null;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Here().ErrorResult(ex);
            throw;
        }
    }

    public static async Task<DownloadStatus> GetDownloadTaskStatusAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (!key.IsValid)
            {
                _log.Here()
                    .Error(
                        "Invalid {Name} {Key} in {GetDownloadTaskStatus}",
                        nameof(DownloadTaskKey),
                        key.ToString(),
                        nameof(GetDownloadTaskStatusAsync)
                    );
                return DownloadStatus.Unknown;
            }

            switch (key.Type)
            {
                // DownloadTaskType.Movie
                case DownloadTaskType.Movie:
                    return await dbContext
                        .DownloadTaskMovie.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                // DownloadTaskType.MovieData
                case DownloadTaskType.MovieData:
                case DownloadTaskType.MoviePart:
                    return await dbContext
                        .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                // DownloadTaskType.TvShow
                case DownloadTaskType.TvShow:
                    return await dbContext
                        .DownloadTaskTvShow.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                // DownloadTaskType.TvShowSeason
                case DownloadTaskType.Season:
                    return await dbContext
                        .DownloadTaskTvShowSeason.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                // DownloadTaskType.Episode
                case DownloadTaskType.Episode:
                    return await dbContext
                        .DownloadTaskTvShowEpisode.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                // DownloadTaskType.EpisodeData
                case DownloadTaskType.EpisodeData:
                case DownloadTaskType.EpisodePart:
                    return await dbContext
                        .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                        .Take(1)
                        .Select(x => x.DownloadStatus)
                        .FirstOrDefaultAsync(cancellationToken);

                default:
                    _log.Here()
                        .Error(
                            "Unsupported {Name} {Type} in {GetDownloadTaskStatus}",
                            nameof(DownloadTaskType),
                            key.Type,
                            nameof(GetDownloadTaskStatusAsync)
                        );
                    return DownloadStatus.Unknown;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.Here().ErrorResult(ex);
        }

        return DownloadStatus.Unknown;
    }

    public static async Task<DownloadTaskFileBase?> GetDownloadTaskFileAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            // DownloadTaskType.MovieData
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                return await dbContext
                    .DownloadTaskMovieFile.Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .FirstOrDefaultAsync(x => x.Id == key.Id, cancellationToken);

            // DownloadTaskType.EpisodeData
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                return await dbContext
                    .DownloadTaskTvShowEpisodeFile.Include(x => x.PlexServer)
                    .Include(x => x.PlexLibrary)
                    .FirstOrDefaultAsync(x => x.Id == key.Id, cancellationToken);
            default:
                return null;
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetAllDownloadTasksByServerAsync(
        this IReaparrDbContext dbContext,
        int plexServerId = 0,
        bool asTracking = false,
        CancellationToken cancellationToken = default
    )
    {
        var downloadTasks = new List<DownloadTaskGeneric>();

        var downloadTasksMovies = await dbContext
            .DownloadTaskMovie.AsTracking(
                asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        var downloadTasksTvShows = await dbContext
            .DownloadTaskTvShow.AsTracking(
                asTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
            .IncludeAll()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        downloadTasks.AddRange(downloadTasksMovies.Select(x => x.ToGeneric()));
        downloadTasks.AddRange(downloadTasksTvShows.Select(x => x.ToGeneric()));

        // Sort by CreatedAt
        downloadTasks.Sort((x, y) => DateTime.Compare(x.CreatedAt, y.CreatedAt));

        return downloadTasks;
    }

    public static async Task<List<DownloadTaskGeneric>> GetDownloadProgressTasksByServerAsync(
        this IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        var movies = await dbContext
            .DownloadTaskMovie.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
            })
            .ToListAsync(cancellationToken);

        var movieFiles = await dbContext
            .DownloadTaskMovieFile.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
                x.DataReceived,
                x.DataTotal,
                x.DownloadSpeed,
                x.FileTransferSpeed,
                x.FileDataTransferred,
            })
            .ToListAsync(cancellationToken);

        var tvShows = await dbContext
            .DownloadTaskTvShow.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
            })
            .ToListAsync(cancellationToken);

        var seasons = await dbContext
            .DownloadTaskTvShowSeason.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
            })
            .ToListAsync(cancellationToken);

        var episodes = await dbContext
            .DownloadTaskTvShowEpisode.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
            })
            .ToListAsync(cancellationToken);

        var episodeFiles = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .Where(x => plexServerId <= 0 || x.PlexServerId == plexServerId)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.PlexApiRatingKey,
                x.Title,
                x.DownloadStatus,
                x.CreatedAt,
                x.PlexServerId,
                x.PlexLibraryId,
                x.DataReceived,
                x.DataTotal,
                x.DownloadSpeed,
                x.FileTransferSpeed,
                x.FileDataTransferred,
            })
            .ToListAsync(cancellationToken);

        var movieFilesByParent = movieFiles.ToLookup(x => x.ParentId);
        var seasonsByParent = seasons.ToLookup(x => x.ParentId);
        var episodesByParent = episodes.ToLookup(x => x.ParentId);
        var episodeFilesByParent = episodeFiles.ToLookup(x => x.ParentId);

        var downloadTasks = new List<DownloadTaskGeneric>();

        foreach (var movie in movies.OrderBy(x => x.CreatedAt))
        {
            var children = movieFilesByParent[movie.Id]
                .OrderBy(x => x.CreatedAt)
                .Select(file =>
                    CreateFileNode(
                        file.Id,
                        file.ParentId,
                        file.PlexApiRatingKey,
                        file.Title,
                        PlexMediaType.Movie,
                        DownloadTaskType.MovieData,
                        file.DownloadStatus,
                        file.CreatedAt,
                        file.PlexServerId,
                        file.PlexLibraryId,
                        file.DataReceived,
                        file.DataTotal,
                        file.DownloadSpeed,
                        file.FileTransferSpeed,
                        file.FileDataTransferred
                    )
                )
                .ToList();

            var movieTask = CreateParentNode(
                movie.Id,
                movie.PlexApiRatingKey,
                movie.Title,
                PlexMediaType.Movie,
                DownloadTaskType.Movie,
                movie.DownloadStatus,
                movie.CreatedAt,
                movie.PlexServerId,
                movie.PlexLibraryId,
                children
            );

            movieTask.Calculate();
            downloadTasks.Add(movieTask);
        }

        foreach (var tvShow in tvShows.OrderBy(x => x.CreatedAt))
        {
            var tvShowSeasons = seasonsByParent[tvShow.Id]
                .OrderBy(x => x.CreatedAt)
                .Select(season =>
                {
                    var seasonEpisodes = episodesByParent[season.Id]
                        .OrderBy(x => x.CreatedAt)
                        .Select(episode =>
                        {
                            var files = episodeFilesByParent[episode.Id]
                                .OrderBy(x => x.CreatedAt)
                                .Select(file =>
                                    CreateFileNode(
                                        file.Id,
                                        file.ParentId,
                                        file.PlexApiRatingKey,
                                        file.Title,
                                        PlexMediaType.Episode,
                                        DownloadTaskType.EpisodeData,
                                        file.DownloadStatus,
                                        file.CreatedAt,
                                        file.PlexServerId,
                                        file.PlexLibraryId,
                                        file.DataReceived,
                                        file.DataTotal,
                                        file.DownloadSpeed,
                                        file.FileTransferSpeed,
                                        file.FileDataTransferred
                                    )
                                )
                                .ToList();

                            return CreateParentNode(
                                episode.Id,
                                episode.PlexApiRatingKey,
                                episode.Title,
                                PlexMediaType.Episode,
                                DownloadTaskType.Episode,
                                episode.DownloadStatus,
                                episode.CreatedAt,
                                episode.PlexServerId,
                                episode.PlexLibraryId,
                                files,
                                episode.ParentId
                            );
                        })
                        .ToList();

                    var seasonTask = CreateParentNode(
                        season.Id,
                        season.PlexApiRatingKey,
                        season.Title,
                        PlexMediaType.Season,
                        DownloadTaskType.Season,
                        season.DownloadStatus,
                        season.CreatedAt,
                        season.PlexServerId,
                        season.PlexLibraryId,
                        seasonEpisodes,
                        season.ParentId
                    );

                    seasonTask.Calculate();
                    return seasonTask;
                })
                .ToList();

            var tvShowTask = CreateParentNode(
                tvShow.Id,
                tvShow.PlexApiRatingKey,
                tvShow.Title,
                PlexMediaType.TvShow,
                DownloadTaskType.TvShow,
                tvShow.DownloadStatus,
                tvShow.CreatedAt,
                tvShow.PlexServerId,
                tvShow.PlexLibraryId,
                tvShowSeasons
            );

            tvShowTask.Calculate();
            downloadTasks.Add(tvShowTask);
        }

        downloadTasks.Sort((x, y) => DateTime.Compare(x.CreatedAt, y.CreatedAt));
        return downloadTasks;

        static DownloadTaskGeneric CreateParentNode(
            Guid id,
            int ratingKey,
            string title,
            PlexMediaType mediaType,
            DownloadTaskType taskType,
            DownloadStatus status,
            DateTime createdAt,
            int plexServerId,
            int plexLibraryId,
            List<DownloadTaskGeneric> children,
            Guid parentId = default
        ) =>
            new()
            {
                Id = id,
                RatingKey = ratingKey,
                Title = title,
                FullTitle = string.Empty,
                MediaType = mediaType,
                DownloadTaskType = taskType,
                DownloadStatus = status,
                CreatedAt = createdAt,
                FileName = string.Empty,
                IsDownloadable = false,
                DownloadDirectory = string.Empty,
                Quality = VideoQuality.None,
                DestinationDirectory = string.Empty,
                FileLocationUrl = string.Empty,
                DataReceived = 0,
                DataTotal = 0,
                DownloadSpeed = 0,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
                CurrentFileTransferBytesOffset = 0,
                Children = children,
                ParentId = parentId,
                PlexServer = null,
                PlexServerId = plexServerId,
                PlexLibrary = null,
                PlexLibraryId = plexLibraryId,
            };

        static DownloadTaskGeneric CreateFileNode(
            Guid id,
            Guid parentId,
            int ratingKey,
            string title,
            PlexMediaType mediaType,
            DownloadTaskType taskType,
            DownloadStatus status,
            DateTime createdAt,
            int plexServerId,
            int plexLibraryId,
            long dataReceived,
            long dataTotal,
            long downloadSpeed,
            long fileTransferSpeed,
            long fileDataTransferred
        ) =>
            new()
            {
                Id = id,
                RatingKey = ratingKey,
                Title = title,
                FullTitle = string.Empty,
                MediaType = mediaType,
                DownloadTaskType = taskType,
                DownloadStatus = status,
                CreatedAt = createdAt,
                FileName = string.Empty,
                IsDownloadable = true,
                DownloadDirectory = string.Empty,
                Quality = VideoQuality.None,
                DestinationDirectory = string.Empty,
                FileLocationUrl = string.Empty,
                DataReceived = dataReceived,
                DataTotal = dataTotal,
                DownloadSpeed = downloadSpeed,
                FileTransferSpeed = fileTransferSpeed,
                FileDataTransferred = fileDataTransferred,
                CurrentFileTransferBytesOffset = 0,
                Children = [],
                ParentId = parentId,
                PlexServer = null,
                PlexServerId = plexServerId,
                PlexLibrary = null,
                PlexLibraryId = plexLibraryId,
            };
    }

    public static Task<DownloadTaskTvShow?> GetDownloadTaskTvShowByRatingKeyQuery(
        this IReaparrDbContext dbContext,
        int plexServerId,
        int ratingKey,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .DownloadTaskTvShow.AsTracking()
            .IncludeAll()
            .FirstOrDefaultAsync(
                x => x.PlexServerId == plexServerId && x.PlexApiRatingKey == ratingKey,
                cancellationToken
            );
    }

    public static async Task UpdateDownloadProgress(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        IDownloadTaskProgress progress,
        DirectDownloadSnapshot? snapshot = null,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                                .SetProperty(x => x.DataReceived, progress.DataReceived)
                                .SetProperty(x => x.DataTotal, progress.DataTotal)
                                .SetProperty(x => x.DirectDownloadSnapshot, snapshot),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, progress.DownloadSpeed)
                                .SetProperty(x => x.DataReceived, progress.DataReceived)
                                .SetProperty(x => x.DataTotal, progress.DataTotal)
                                .SetProperty(x => x.DirectDownloadSnapshot, snapshot),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
            case DownloadTaskType.Season:
            case DownloadTaskType.Episode:
                _log.Here()
                    .Error(
                        "{Name} of type {Type} is not supported in {MethodName}",
                        nameof(DownloadTaskType),
                        key.Type,
                        nameof(UpdateDownloadProgress)
                    );
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static async Task<Result> ResetDownloadTaskProgress(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        DownloadStatus downloadStatus,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, 0)
                                .SetProperty(x => x.DataReceived, 0)
                                .SetProperty(x => x.FileTransferSpeed, 0)
                                .SetProperty(x => x.FileDataTransferred, 0)
                                .SetProperty(x => x.CurrentFileTransferBytesOffset, 0)
                                .SetProperty(x => x.DirectDownloadSnapshot, (DirectDownloadSnapshot?)null)
                                .SetProperty(x => x.DownloadStatus, downloadStatus),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.DownloadSpeed, 0)
                                .SetProperty(x => x.DataReceived, 0)
                                .SetProperty(x => x.FileTransferSpeed, 0)
                                .SetProperty(x => x.FileDataTransferred, 0)
                                .SetProperty(x => x.CurrentFileTransferBytesOffset, 0)
                                .SetProperty(x => x.DirectDownloadSnapshot, (DirectDownloadSnapshot?)null)
                                .SetProperty(x => x.DownloadStatus, downloadStatus),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
            case DownloadTaskType.Season:
            case DownloadTaskType.Episode:
                return _log.Here()
                    .ErrorResult(
                        "{Name} of type {Type} is not supported in {MethodName}",
                        nameof(DownloadTaskType),
                        key.Type,
                        nameof(ResetDownloadTaskProgress)
                    );
            case DownloadTaskType.None:
            default:
                return Result.Fail($"Unsupported DownloadTaskType {key.Type}").LogError();
        }

        return Result.Ok();
    }

    public static async Task UpdateDownloadFileTransferProgress(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        IDownloadFileTransferProgress progress,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.MovieData:
                await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.FileTransferSpeed, progress.FileTransferSpeed)
                                .SetProperty(x => x.FileDataTransferred, progress.FileDataTransferred)
                                .SetProperty(
                                    x => x.CurrentFileTransferBytesOffset,
                                    progress.CurrentFileTransferBytesOffset
                                ),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p =>
                            p.SetProperty(x => x.FileTransferSpeed, progress.FileTransferSpeed)
                                .SetProperty(x => x.FileDataTransferred, progress.FileDataTransferred)
                                .SetProperty(
                                    x => x.CurrentFileTransferBytesOffset,
                                    progress.CurrentFileTransferBytesOffset
                                ),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
            case DownloadTaskType.Season:
            case DownloadTaskType.Episode:
                _log.Here()
                    .Error(
                        "{Name} of type {Type} is not supported in {MethodName}",
                        nameof(DownloadTaskType),
                        key.Type,
                        nameof(UpdateDownloadFileTransferProgress)
                    );
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static async Task<List<DownloadTaskKey>> GetDownloadableChildTaskKeys(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        var keys = new List<DownloadTaskKey>();
        var downloadTask = await dbContext.GetDownloadTaskAsync(key, cancellationToken);
        if (downloadTask is null)
            return keys;

        FindDownloadableTaskKeys([downloadTask]);

        return keys;

        void FindDownloadableTaskKeys(ICollection<DownloadTaskGeneric> tasks)
        {
            if (!tasks.Any())
                return;

            foreach (var task in tasks.OrderByNatural(x => x.FullTitle))
            {
                if (task.IsDownloadable)
                    keys.Add(task.ToKey());

                if (task.Children.Any())
                    FindDownloadableTaskKeys(task.Children);
            }
        }
    }

    public static async Task<List<DownloadTaskGeneric>> GetDownloadableChildTasks(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        var keys = await dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        var results = await Task.WhenAll(keys.Select(x => dbContext.GetDownloadTaskAsync(x, cancellationToken)));

        return results.Where(x => x != null).ToList()!;
    }

    public static async Task<int> DeleteOrphanedParentTasksAsync(this IReaparrDbContext dbContext, CancellationToken ct)
    {
        var totalRowsDeleted = 0;

        totalRowsDeleted += await dbContext
            .DownloadTaskMovie.Where(x => !dbContext.DownloadTaskMovieFile.Any(y => y.ParentId == x.Id))
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowEpisode.Where(x => !dbContext.DownloadTaskTvShowEpisodeFile.Any(y => y.ParentId == x.Id))
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowSeason.Where(x => !dbContext.DownloadTaskTvShowEpisode.Any(y => y.ParentId == x.Id))
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShow.Where(x => !dbContext.DownloadTaskTvShowSeason.Any(y => y.ParentId == x.Id))
            .ExecuteDeleteAsync(ct);

        return totalRowsDeleted;
    }
}
