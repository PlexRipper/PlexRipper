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

    public static async Task<List<DownloadTaskKey>> GetDownloadTaskKeysAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyList<Guid> guids,
        CancellationToken cancellationToken = default
    )
    {
        if (guids.Count == 0)
            return [];

        var filtered = guids.Where(g => g != Guid.Empty).ToList();
        if (filtered.Count == 0)
            return [];

        // Filter BEFORE projecting so EF Core can translate the UNION across different entity
        // types. Applying .Where() after .ProjectToKey() (which uses Select) would place the
        // predicate after a client projection and cause a translation exception.
        var queries = new IQueryable<DownloadTaskKey>[]
        {
            dbContext.DownloadTaskTvShow.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
            dbContext.DownloadTaskTvShowSeason.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisode.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
            dbContext.DownloadTaskTvShowEpisodeFile.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
            dbContext.DownloadTaskMovie.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
            dbContext.DownloadTaskMovieFile.Where(x => filtered.Contains(x.Id)).ProjectToKey(),
        };

        var keys = new List<DownloadTaskKey>();

        foreach (var query in queries)
            keys.AddRange(await query.ToListAsync(cancellationToken));

        return keys;
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
        var isFilteredByServer = plexServerId > 0;

        var rows = await dbContext
            .DownloadTaskMovie.AsNoTracking()
            .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
            .Select(x => new DownloadProgressRow
            {
                Id = x.Id,
                ParentId = null,
                PlexApiRatingKey = x.PlexApiRatingKey,
                Title = x.Title,
                FullTitle = x.FullTitle,
                MediaType = PlexMediaType.Movie,
                DownloadTaskType = DownloadTaskType.Movie,
                DownloadStatus = x.DownloadStatus,
                CreatedAt = x.CreatedAt,
                PlexServerId = x.PlexServerId,
                PlexLibraryId = x.PlexLibraryId,
                DataReceived = 0,
                DataTotal = 0,
                Percentage = 0,
                DownloadSpeed = 0,
                TimeRemaining = 0,
                FileTransferSpeed = 0,
                FileDataTransferred = 0,
            })
            .Concat(
                dbContext
                    .DownloadTaskMovieFile.AsNoTracking()
                    .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
                    .Select(x => new DownloadProgressRow
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        PlexApiRatingKey = x.PlexApiRatingKey,
                        Title = x.Title,
                        FullTitle = x.FullTitle,
                        MediaType = PlexMediaType.Movie,
                        DownloadTaskType = DownloadTaskType.MovieData,
                        DownloadStatus = x.DownloadStatus,
                        CreatedAt = x.CreatedAt,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        DataReceived = x.DataReceived,
                        DataTotal = x.DataTotal,
                        Percentage = x.Percentage,
                        DownloadSpeed = x.DownloadSpeed,
                        TimeRemaining = x.TimeRemaining,
                        FileTransferSpeed = x.FileTransferSpeed,
                        FileDataTransferred = x.FileDataTransferred,
                    })
            )
            .Concat(
                dbContext
                    .DownloadTaskTvShow.AsNoTracking()
                    .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
                    .Select(x => new DownloadProgressRow
                    {
                        Id = x.Id,
                        ParentId = null,
                        PlexApiRatingKey = x.PlexApiRatingKey,
                        Title = x.Title,
                        FullTitle = x.FullTitle,
                        MediaType = PlexMediaType.TvShow,
                        DownloadTaskType = DownloadTaskType.TvShow,
                        DownloadStatus = x.DownloadStatus,
                        CreatedAt = x.CreatedAt,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        DataReceived = 0,
                        DataTotal = 0,
                        Percentage = 0,
                        DownloadSpeed = 0,
                        TimeRemaining = 0,
                        FileTransferSpeed = 0,
                        FileDataTransferred = 0,
                    })
            )
            .Concat(
                dbContext
                    .DownloadTaskTvShowSeason.AsNoTracking()
                    .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
                    .Select(x => new DownloadProgressRow
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        PlexApiRatingKey = x.PlexApiRatingKey,
                        Title = x.Title,
                        FullTitle = x.FullTitle,
                        MediaType = PlexMediaType.Season,
                        DownloadTaskType = DownloadTaskType.Season,
                        DownloadStatus = x.DownloadStatus,
                        CreatedAt = x.CreatedAt,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        DataReceived = 0,
                        DataTotal = 0,
                        Percentage = 0,
                        DownloadSpeed = 0,
                        TimeRemaining = 0,
                        FileTransferSpeed = 0,
                        FileDataTransferred = 0,
                    })
            )
            .Concat(
                dbContext
                    .DownloadTaskTvShowEpisode.AsNoTracking()
                    .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
                    .Select(x => new DownloadProgressRow
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        PlexApiRatingKey = x.PlexApiRatingKey,
                        Title = x.Title,
                        FullTitle = x.FullTitle,
                        MediaType = PlexMediaType.Episode,
                        DownloadTaskType = DownloadTaskType.Episode,
                        DownloadStatus = x.DownloadStatus,
                        CreatedAt = x.CreatedAt,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        DataReceived = 0,
                        DataTotal = 0,
                        Percentage = 0,
                        DownloadSpeed = 0,
                        TimeRemaining = 0,
                        FileTransferSpeed = 0,
                        FileDataTransferred = 0,
                    })
            )
            .Concat(
                dbContext
                    .DownloadTaskTvShowEpisodeFile.AsNoTracking()
                    .Where(x => !isFilteredByServer || x.PlexServerId == plexServerId)
                    .Select(x => new DownloadProgressRow
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        PlexApiRatingKey = x.PlexApiRatingKey,
                        Title = x.Title,
                        FullTitle = x.FullTitle,
                        MediaType = PlexMediaType.Episode,
                        DownloadTaskType = DownloadTaskType.EpisodeData,
                        DownloadStatus = x.DownloadStatus,
                        CreatedAt = x.CreatedAt,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        DataReceived = x.DataReceived,
                        DataTotal = x.DataTotal,
                        Percentage = x.Percentage,
                        DownloadSpeed = x.DownloadSpeed,
                        TimeRemaining = x.TimeRemaining,
                        FileTransferSpeed = x.FileTransferSpeed,
                        FileDataTransferred = x.FileDataTransferred,
                    })
            )
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return [];

        var byId = rows.ToDictionary(x => x.Id, CreateNode);
        foreach (var row in rows.Where(x => x.ParentId.HasValue))
        {
            if (!byId.TryGetValue(row.ParentId!.Value, out var parent))
                continue;

            parent.Children.Add(byId[row.Id]);
        }

        var roots = rows.Where(x => !x.ParentId.HasValue).Select(x => byId[x.Id]).OrderBy(x => x.CreatedAt).ToList();

        foreach (var root in roots)
            root.Calculate();

        return roots;

        static DownloadTaskGeneric CreateNode(DownloadProgressRow row) =>
            new()
            {
                Id = row.Id,
                RatingKey = row.PlexApiRatingKey,
                Title = row.Title,
                FullTitle = row.FullTitle,
                MediaType = row.MediaType,
                DownloadTaskType = row.DownloadTaskType,
                DownloadStatus = row.DownloadStatus,
                CreatedAt = row.CreatedAt,
                FileName = string.Empty,
                IsDownloadable =
                    row.ParentId.HasValue
                    && (
                        row.DownloadTaskType == DownloadTaskType.MovieData
                        || row.DownloadTaskType == DownloadTaskType.EpisodeData
                    ),
                DownloadDirectory = string.Empty,
                Quality = VideoQuality.None,
                DestinationDirectory = string.Empty,
                FileLocationUrl = string.Empty,
                DataReceived = row.DataReceived,
                DataTotal = row.DataTotal,
                Percentage = row.Percentage,
                DownloadSpeed = row.DownloadSpeed,
                TimeRemaining = row.TimeRemaining,
                FileTransferSpeed = row.FileTransferSpeed,
                FileDataTransferred = row.FileDataTransferred,
                CurrentFileTransferBytesOffset = 0,
                Children = [],
                ParentId = row.ParentId ?? Guid.Empty,
                PlexServer = null,
                PlexServerId = row.PlexServerId,
                PlexLibrary = null,
                PlexLibraryId = row.PlexLibraryId,
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
                                .SetProperty(x => x.Percentage, progress.Percentage)
                                .SetProperty(x => x.TimeRemaining, progress.TimeRemaining)
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
                                .SetProperty(x => x.Percentage, progress.Percentage)
                                .SetProperty(x => x.TimeRemaining, progress.TimeRemaining)
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
                                .SetProperty(x => x.Percentage, 0)
                                .SetProperty(x => x.TimeRemaining, 0)
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
                                .SetProperty(x => x.Percentage, 0)
                                .SetProperty(x => x.TimeRemaining, 0)
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

    public static async Task ClearDownloadSpeed(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
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
                        p => p.SetProperty(x => x.DownloadSpeed, 0).SetProperty(x => x.TimeRemaining, 0),
                        cancellationToken
                    );
                break;
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
                await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .ExecuteUpdateAsync(
                        p => p.SetProperty(x => x.DownloadSpeed, 0).SetProperty(x => x.TimeRemaining, 0),
                        cancellationToken
                    );
                break;
        }
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
                                )
                                .SetProperty(
                                    x => x.Percentage,
                                    x =>
                                        x.DataTotal > 0
                                            ? progress.CurrentFileTransferBytesOffset * 100m / x.DataTotal
                                            : 0m
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
                                )
                                .SetProperty(
                                    x => x.Percentage,
                                    x =>
                                        x.DataTotal > 0
                                            ? progress.CurrentFileTransferBytesOffset * 100m / x.DataTotal
                                            : 0m
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

    public static async Task<int> DeleteOrphanedParentTasksByServerIdAsync(
        this IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken ct
    )
    {
        var totalRowsDeleted = 0;

        totalRowsDeleted += await dbContext
            .DownloadTaskMovie.Where(x =>
                x.PlexServerId == plexServerId && !dbContext.DownloadTaskMovieFile.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowEpisode.Where(x =>
                x.PlexServerId == plexServerId && !dbContext.DownloadTaskTvShowEpisodeFile.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowSeason.Where(x =>
                x.PlexServerId == plexServerId && !dbContext.DownloadTaskTvShowEpisode.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShow.Where(x =>
                x.PlexServerId == plexServerId && !dbContext.DownloadTaskTvShowSeason.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        return totalRowsDeleted;
    }

    public static async Task<int> DeleteOrphanedParentTasksByRootIdsAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<Guid> rootIds,
        CancellationToken ct
    )
    {
        if (rootIds.Count == 0)
            return 0;

        var totalRowsDeleted = 0;

        totalRowsDeleted += await dbContext
            .DownloadTaskMovie.Where(x =>
                rootIds.Contains(x.Id) && !dbContext.DownloadTaskMovieFile.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowEpisode.Where(x =>
                rootIds.Contains(x.Parent!.ParentId)
                && !dbContext.DownloadTaskTvShowEpisodeFile.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShowSeason.Where(x =>
                rootIds.Contains(x.ParentId) && !dbContext.DownloadTaskTvShowEpisode.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        totalRowsDeleted += await dbContext
            .DownloadTaskTvShow.Where(x =>
                rootIds.Contains(x.Id) && !dbContext.DownloadTaskTvShowSeason.Any(y => y.ParentId == x.Id)
            )
            .ExecuteDeleteAsync(ct);

        return totalRowsDeleted;
    }

    public static async Task<HashSet<Guid>> GetAffectedRootDownloadTaskIdsAsync(
        this IReaparrDbContext dbContext,
        IReadOnlyCollection<Guid> downloadTaskIds,
        CancellationToken ct
    )
    {
        if (downloadTaskIds.Count == 0)
            return [];

        var movieRootIdsTask = dbContext
            .DownloadTaskMovie.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        var movieFileRootIdsTask = dbContext
            .DownloadTaskMovieFile.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.ParentId)
            .ToListAsync(ct);

        var tvShowRootIdsTask = dbContext
            .DownloadTaskTvShow.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        var seasonRootIdsTask = dbContext
            .DownloadTaskTvShowSeason.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.ParentId)
            .ToListAsync(ct);

        var episodeSeasonIdsTask = dbContext
            .DownloadTaskTvShowEpisode.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.ParentId)
            .ToListAsync(ct);

        var episodeFileEpisodeIdsTask = dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => downloadTaskIds.Contains(x.Id))
            .Select(x => x.ParentId)
            .ToListAsync(ct);

        await Task.WhenAll(
            movieRootIdsTask,
            movieFileRootIdsTask,
            tvShowRootIdsTask,
            seasonRootIdsTask,
            episodeSeasonIdsTask,
            episodeFileEpisodeIdsTask
        );

        var rootIds = new HashSet<Guid>(
            movieRootIdsTask
                .Result.Concat(movieFileRootIdsTask.Result)
                .Concat(tvShowRootIdsTask.Result)
                .Concat(seasonRootIdsTask.Result)
        );

        var seasonIds = episodeSeasonIdsTask.Result;
        if (episodeFileEpisodeIdsTask.Result.Count > 0)
        {
            var episodeDerivedSeasonIds = await dbContext
                .DownloadTaskTvShowEpisode.Where(x => episodeFileEpisodeIdsTask.Result.Contains(x.Id))
                .Select(x => x.ParentId)
                .ToListAsync(ct);
            seasonIds = seasonIds.Concat(episodeDerivedSeasonIds).Distinct().ToList();
        }

        if (seasonIds.Count > 0)
        {
            var seasonRootIds = await dbContext
                .DownloadTaskTvShowSeason.Where(x => seasonIds.Contains(x.Id))
                .Select(x => x.ParentId)
                .ToListAsync(ct);
            rootIds.UnionWith(seasonRootIds);
        }

        return rootIds;
    }

    public static async Task<DownloadTaskKey?> GetRootDownloadTaskKeyAsync(
        this IReaparrDbContext dbContext,
        DownloadTaskKey key,
        CancellationToken cancellationToken = default
    )
    {
        switch (key.Type)
        {
            case DownloadTaskType.Movie:
            case DownloadTaskType.TvShow:
                return key;
            case DownloadTaskType.MovieData:
            case DownloadTaskType.MoviePart:
                return await dbContext
                    .DownloadTaskMovieFile.Where(x => x.Id == key.Id)
                    .Select(x => new DownloadTaskKey
                    {
                        Id = x.ParentId,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        Type = DownloadTaskType.Movie,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.Season:
                return await dbContext
                    .DownloadTaskTvShowSeason.Where(x => x.Id == key.Id)
                    .Select(x => new DownloadTaskKey
                    {
                        Id = x.ParentId,
                        PlexServerId = x.PlexServerId,
                        PlexLibraryId = x.PlexLibraryId,
                        Type = DownloadTaskType.TvShow,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            case DownloadTaskType.Episode:
            {
                var season = await dbContext
                    .DownloadTaskTvShowEpisode.Where(x => x.Id == key.Id)
                    .Select(x => new
                    {
                        x.ParentId,
                        x.PlexServerId,
                        x.PlexLibraryId,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (season is null)
                    return null;

                return await dbContext
                    .DownloadTaskTvShowSeason.Where(x => x.Id == season.ParentId)
                    .Select(x => new DownloadTaskKey
                    {
                        Id = x.ParentId,
                        PlexServerId = season.PlexServerId,
                        PlexLibraryId = season.PlexLibraryId,
                        Type = DownloadTaskType.TvShow,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }
            case DownloadTaskType.EpisodeData:
            case DownloadTaskType.EpisodePart:
            {
                var episode = await dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == key.Id)
                    .Select(x => new
                    {
                        x.ParentId,
                        x.PlexServerId,
                        x.PlexLibraryId,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (episode is null)
                    return null;

                var season = await dbContext
                    .DownloadTaskTvShowEpisode.Where(x => x.Id == episode.ParentId)
                    .Select(x => x.ParentId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (season == Guid.Empty)
                    return null;

                return await dbContext
                    .DownloadTaskTvShowSeason.Where(x => x.Id == season)
                    .Select(x => new DownloadTaskKey
                    {
                        Id = x.ParentId,
                        PlexServerId = episode.PlexServerId,
                        PlexLibraryId = episode.PlexLibraryId,
                        Type = DownloadTaskType.TvShow,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }
            default:
                return null;
        }
    }
}
