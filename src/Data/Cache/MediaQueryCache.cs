using System.Collections.Concurrent;
using FlexQuery.NET.Models;
using Reaparr.Application.Contracts;

namespace Reaparr.Data;

/// <summary>
/// Caches media overview query snapshots so page changes and sort direction changes do not repeatedly execute the full media query.
/// </summary>
public sealed class MediaQueryCache : IMediaQueryCache
{
    private const string CacheWarmingUpMessage = "Media query cache is warming up. The media overview will appear once the cache is built on the next request.";
    private static readonly string[] _warmupSortFields =
    [
        nameof(BasePlexMedia.SearchTitle),
        nameof(BasePlexMedia.Year),
        nameof(BasePlexMedia.AddedAt),
        nameof(BasePlexMedia.UpdatedAt),
        nameof(BasePlexMedia.Duration),
        nameof(BasePlexMedia.MediaSize),
        "quality",
    ];

    private static readonly PlexMediaType[] _warmupMediaTypes =
    [
        PlexMediaType.Movie,
        PlexMediaType.TvShow,
    ];

    private readonly ConcurrentDictionary<MediaQueryMetadataKey, MediaQueryMetadataSnapshot> _metadataSnapshots = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, MediaQuerySortedListSnapshot> _sortedListSnapshots = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, Lazy<Task<Result<MediaQueryBuildResult>>>> _builds = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, long> _buildVersions = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, bool> _dirtyKeys = new();

    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IGeneralSettings _generalSettings;
    private readonly ILogger _log;

    public MediaQueryCache(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContextFactory dbContextFactory, IGeneralSettings generalSettings)
    {
        _log = log.ForContext<MediaQueryCache>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
        _generalSettings = generalSettings;
    }

    /// <inheritdoc />
    public bool SuppressInvalidation { get; set; }

    /// <inheritdoc />
    public async Task<Result<PagedMediaQueryResult>> GetMediaAsync(
        MediaQueryFilter filter,
        CancellationToken cancellationToken)
    {
        // Single-library queries are small — bypass the cache and hit the DB directly.
        if (filter.PlexLibraryId > 0)
            return await BypassCacheAsync(filter, cancellationToken, "specific library scope");

        if (!string.IsNullOrWhiteSpace(filter.Parameters.Query))
            return await BypassCacheAsync(filter, cancellationToken, "query parameter is set");

        if (!string.IsNullOrWhiteSpace(filter.Parameters.Filter))
            return await BypassCacheAsync(filter, cancellationToken, "filter parameter is set");

        var libraryIds = await ResolveLibraryIdsAsync(filter, cancellationToken);
        var sort = filter.Parameters.Sort.Normalize(libraryIds.Count);
        if (sort is null)
        {
            _log.Here().Debug("Bypassing media query cache: unsupported sort {Sort}", filter.Parameters.Sort);
            return await BypassCacheAsync(filter, cancellationToken, null);
        }

        var metadataKey = new MediaQueryMetadataKey(
            filter.MediaType,
            libraryIds,
            filter.FilterOfflineMedia,
            filter.FilterOwnedMedia);

        var sortedListKey = new MediaQuerySortedListKey(metadataKey, sort.Field);

        if (_metadataSnapshots.TryGetValue(metadataKey, out var metadataSnapshot)
            && _sortedListSnapshots.TryGetValue(sortedListKey, out var sortedListSnapshot))
        {
            if (_dirtyKeys.ContainsKey(sortedListKey))
                QueueSnapshotRefresh(sortedListKey);

            _log.Here().Debug("Media query cache hit for {MediaType} sorted by {SortField}", filter.MediaType, sortedListKey.NormalizedAscendingSortField);
            return Result.Ok(CreatePage(filter, metadataSnapshot, sortedListSnapshot, sort.Descending));
        }

        _log.Here().Debug("Media query cache miss for {MediaType} sorted by {SortField}", filter.MediaType, sortedListKey.NormalizedAscendingSortField);
        QueueSnapshotRefresh(sortedListKey);
        return Result.Fail(CacheWarmingUpMessage).Add503ServiceUnavailableError();
    }

    /// <inheritdoc />
    public async Task BuildCache()
    {
        // Resolve library IDs once for Movie and TvShow warmup keys
        var movieLibraryIds = await ResolveLibraryIdsAsync(CreateWarmupFilter(PlexMediaType.Movie, _warmupSortFields[0]), CancellationToken.None);
        var tvShowLibraryIds = await ResolveLibraryIdsAsync(CreateWarmupFilter(PlexMediaType.TvShow, _warmupSortFields[0]), CancellationToken.None);

        var warmupFilters = _warmupMediaTypes
            .SelectMany(mediaType => _warmupSortFields
                .Select(sortField =>
                {
                    var filter = CreateWarmupFilter(mediaType, sortField);
                    var libraryIds = mediaType == PlexMediaType.Movie ? movieLibraryIds : tvShowLibraryIds;
                    var normalizedSort = MediaSortNormalizer.Normalize(filter.Parameters.Sort, libraryIds.Count)!;
                    var key = new MediaQuerySortedListKey(
                        new MediaQueryMetadataKey(mediaType, libraryIds, filter.FilterOfflineMedia, filter.FilterOwnedMedia),
                        normalizedSort.Field);
                    return (Filter: filter, Key: key);
                }))
            .ToList();

        var tasks = warmupFilters.Select(x => BuildAndStoreSnapshotAsync(x.Filter, x.Key, CancellationToken.None)).ToList();
        var results = await Task.WhenAll(tasks);
        var failures = results.Where(x => x.IsFailed).SelectMany(x => x.Errors).ToList();
        if (failures.Count > 0)
        {
            _log.Here().Warning("Media query cache warmup completed with {FailureCount} failed snapshot builds", failures.Count);
            return;
        }

        _log.Here().Information("Media query cache warmup completed with {SnapshotCount} all-library sorted snapshots", results.Length);
    }

    /// <inheritdoc />
    public void InvalidateLibrary(int plexLibraryId, string reason) => InvalidateLibraries([plexLibraryId], reason);

    /// <inheritdoc />
    public void InvalidateLibraries(IReadOnlyCollection<int> plexLibraryIds, string reason)
    {
        if (SuppressInvalidation)
        {
            _log.Here().Debug("Skipping media query cache invalidation for libraries {PlexLibraryIds}: suppression active. Reason: {Reason}", plexLibraryIds, reason);
            return;
        }
        var affectedLibraryIds = plexLibraryIds.Where(x => x > 0).ToHashSet();
        if (affectedLibraryIds.Count == 0)
            return;

        var dirtyMetadataCount = MarkKeysContainingLibraryAsDirty(_metadataSnapshots, k => k.ContainsAnyLibrary(affectedLibraryIds));
        var dirtySortedListCount = MarkKeysContainingLibraryAsDirty(_sortedListSnapshots, k => k.ContainsAnyLibrary(affectedLibraryIds));
        var inFlightCount = CountKeysContainingLibrary(_builds, k => k.ContainsAnyLibrary(affectedLibraryIds));

        _log.Here().Information(
            "Invalidated media query cache for libraries {PlexLibraryIds}: {Reason}. " +
            "Marked {MetadataCount} metadata, {SortedListCount} sorted-lists as dirty, {InFlightCount} in-flight builds.",
            affectedLibraryIds, reason, dirtyMetadataCount, dirtySortedListCount, inFlightCount);
    }

    // ── Background refresh helpers ────────────────────────────────

    private void QueueSnapshotRefresh(MediaQuerySortedListKey sortedListKey)
    {
        if (_builds.ContainsKey(sortedListKey))
            return;

        var startVersion = _buildVersions.TryGetValue(sortedListKey, out var v) ? v : 0;

        var lazyBuild = _builds.GetOrAdd(
            sortedListKey,
            _ => new Lazy<Task<Result<MediaQueryBuildResult>>>(
                async () =>
                {
                    _dirtyKeys.TryRemove(sortedListKey, out bool _);
                    var result = await BuildAndStoreSnapshotAsync(sortedListKey, CancellationToken.None);
                    // If version was bumped during the build, the result may be stale.
                    // Re-mark dirty so the next read queues a fresh build.
                    if (result.IsSuccess
                        && _buildVersions.TryGetValue(sortedListKey, out var currentVersion)
                        && currentVersion > startVersion)
                    {
                        _dirtyKeys.TryAdd(sortedListKey, true);
                        _log.Here().Debug(
                            "Snapshot build for {SortedListKey} was stale (version {StartVersion} → {CurrentVersion}), re-marking dirty",
                            sortedListKey, startVersion, currentVersion);
                    }

                    return result;
                },
                LazyThreadSafetyMode.ExecutionAndPublication));

        _ = ObserveBuildAsync(lazyBuild, sortedListKey);
    }

    private async Task ObserveBuildAsync(
        Lazy<Task<Result<MediaQueryBuildResult>>> lazyBuild,
        MediaQuerySortedListKey sortedListKey)
    {
        try
        {
            _ = await lazyBuild.Value;
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "Unhandled error in snapshot refresh background observer for {SortedListKey}", sortedListKey);
        }
        finally
        {
            if (_builds.TryGetValue(sortedListKey, out var currentBuild) && ReferenceEquals(currentBuild, lazyBuild))
                _builds.TryRemove(sortedListKey, out _);

            // If dirty marker survived the build (stale detection re-added it), queue another refresh.
            if (_dirtyKeys.ContainsKey(sortedListKey))
                QueueSnapshotRefresh(sortedListKey);
        }
    }

    private async Task<Result<MediaQueryBuildResult>> BuildAndStoreSnapshotAsync(
        MediaQuerySortedListKey sortedListKey,
        CancellationToken cancellationToken)
    {
        var libraryIds = sortedListKey.LibraryIds;
        var plexLibraryId = libraryIds.Count == 1 ? libraryIds[0] : 0;
        var filter = new MediaQueryFilter
        {
            MediaType = sortedListKey.MetadataKey.MediaType,
            PlexLibraryId = plexLibraryId,
            FilterOfflineMedia = sortedListKey.MetadataKey.FilterOfflineMedia,
            FilterOwnedMedia = sortedListKey.MetadataKey.FilterOwnedMedia,
            Parameters = new FlexQueryParameters { Sort = $"{sortedListKey.NormalizedAscendingSortField}:asc", Page = null, PageSize = null },
        };

        var sort = filter.Parameters.Sort.Normalize(libraryIds.Count);
        if (sort is null)
            return Result.Fail("Invalid sort parameter");

        return await BuildAndStoreSnapshotAsync(filter, sortedListKey, cancellationToken);
    }

    private async Task<Result<MediaQueryBuildResult>> BuildAndStoreSnapshotAsync(
        MediaQueryFilter filter,
        MediaQuerySortedListKey sortedListKey,
        CancellationToken cancellationToken)
    {
        var sort = filter.Parameters.Sort.Normalize(sortedListKey.LibraryIds.Count);
        if (sort is null)
            return Result.Fail("Invalid sort parameter");

        var buildResult = await BuildSnapshotAsync(filter, sortedListKey, sort, cancellationToken);

        if (buildResult.IsSuccess)
        {
            _metadataSnapshots[sortedListKey.MetadataKey] = buildResult.Value.Metadata;
            _sortedListSnapshots[sortedListKey] = buildResult.Value.SortedList;
            _dirtyKeys.TryRemove(sortedListKey, out _);
        }

        return buildResult;
    }



    /// <summary>
    /// Executes the media query with paging and user filters removed, producing both metadata
    /// and sorted-list snapshots.
    /// </summary>
    private async Task<Result<MediaQueryBuildResult>> BuildSnapshotAsync(
        MediaQueryFilter originalFilter,
        MediaQuerySortedListKey sortedListKey,
        MediaSortNormalizer.Result sort,
        CancellationToken cancellationToken)
    {
        var commandResult = await _commandExecutor.Send(
            new GetMediaByTypeCommand
            {
                Filter = originalFilter with
                {
                    Parameters = new FlexQueryParameters
                    {
                        Query = null,
                        Filter = null,
                        Sort = $"{sort.Field}:asc",
                        Page = null,
                        PageSize = null,
                        Select = originalFilter.Parameters.Select,
                        Includes = originalFilter.Parameters.Includes,
                        GroupBy = originalFilter.Parameters.GroupBy,
                        Having = originalFilter.Parameters.Having,
                        IncludeCount = originalFilter.Parameters.IncludeCount,
                        Distinct = originalFilter.Parameters.Distinct,
                        Mode = originalFilter.Parameters.Mode,
                    },
                },
            },
            cancellationToken);

        if (commandResult.IsFailed)
            return Result.Fail(commandResult.Errors);

        var result = commandResult.Value;

        return Result.Ok(new MediaQueryBuildResult(
            CreateMetadataSnapshot(sortedListKey.MetadataKey, result),
            new MediaQuerySortedListSnapshot
            {
                Key = sortedListKey,
                Items = result.Items.Select(x => x with { Qualities = x.Qualities.ToList() }).ToList(),
                NavigationIndexes = result.NavigationIndexes.Select(CopyNavigationIndex).ToList(),
                CreatedAt = DateTimeOffset.UtcNow,
            }));
    }

    // ── Library resolution ─────────────────────────────────────────

    /// <summary>
    /// Resolves the effective library id set for the request.
    /// </summary>
    private async Task<IReadOnlyList<int>> ResolveLibraryIdsAsync(MediaQueryFilter filter, CancellationToken cancellationToken)
    {
        if (filter.PlexLibraryId > 0)
            return [filter.PlexLibraryId];

        using var dbContext = await _dbContextFactory.CreateAsync();
        var hasPlexAccounts = await dbContext.PlexAccounts.AnyAsync(cancellationToken);
        var serverList = await dbContext.PlexServers
            .Where(server => !hasPlexAccounts || server.PlexAccountServers.Any())
            .Select(server => new
            {
                server.Id,
                PlexLibraryIds = server.PlexLibraries
                    .Where(library => !hasPlexAccounts || library.PlexAccountLibraries.Any())
                    .Select(library => library.Id)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var allowedPlexLibraryIds = serverList.SelectMany(x => x.PlexLibraryIds).Distinct().ToList();

        if (filter.FilterOwnedMedia)
        {
            var ownedLibraries = await dbContext.PlexAccountLibraries
                .Where(x => x.IsLibraryOwned)
                .Select(x => x.PlexLibraryId)
                .ToListAsync(cancellationToken);

            allowedPlexLibraryIds.RemoveAll(ownedLibraries.Contains);
        }

        if (filter.FilterOfflineMedia)
        {
            foreach (var server in serverList)
            {
                var isServerOnline = await dbContext.IsServerOnline(server.Id, cancellationToken);
                if (!isServerOnline)
                    allowedPlexLibraryIds.RemoveAll(x => server.PlexLibraryIds.Contains(x));
            }
        }

        return allowedPlexLibraryIds.Distinct().OrderBy(x => x).ToList();
    }

    // ── Page creation ──────────────────────────────────────────────

    private static PagedMediaQueryResult CreatePage(
        MediaQueryFilter filter,
        MediaQueryMetadataSnapshot metadataSnapshot,
        MediaQuerySortedListSnapshot sortedListSnapshot,
        bool descending)
    {
        var normalizedPage = Math.Max(filter.Parameters.Page ?? 1, 1);
        var itemCount = sortedListSnapshot.Items.Count;
        var effectivePageSize = filter.Parameters.PageSize is > 0 ? filter.Parameters.PageSize.Value : itemCount;
        var offset = effectivePageSize == 0 ? 0 : (normalizedPage - 1) * effectivePageSize;
        var pageItems = CreatePageItems(sortedListSnapshot.Items, descending, offset, effectivePageSize);
        var navigationIndexes = descending
            ? RebuildNavigationIndexes(sortedListSnapshot.Items, sortedListSnapshot.Key.NormalizedAscendingSortField)
            : sortedListSnapshot.NavigationIndexes.Select(CopyNavigationIndex).ToList();

        return new PagedMediaQueryResult
        {
            QueryHash = filter.QueryHash,
            Page = normalizedPage,
            PageSize = effectivePageSize,
            Items = pageItems,
            NavigationIndexes = navigationIndexes,
            Roles = [],
            Countries = [],
            Genres = [],
            Qualities = [],
            TotalCount = metadataSnapshot.TotalCount,
            MediaCount = metadataSnapshot.MediaCount,
            MovieCount = metadataSnapshot.MovieCount,
            TvShowCount = metadataSnapshot.TvShowCount,
            SeasonCount = metadataSnapshot.SeasonCount,
            EpisodeCount = metadataSnapshot.EpisodeCount,
            TotalMovieCount = metadataSnapshot.TotalMovieCount,
            TotalTvShowCount = metadataSnapshot.TotalTvShowCount,
            TotalSeasonCount = metadataSnapshot.TotalSeasonCount,
            TotalEpisodeCount = metadataSnapshot.TotalEpisodeCount,
            MediaSize = metadataSnapshot.MediaSize,
            TotalMediaSize = metadataSnapshot.TotalMediaSize,
        };
    }

    /// <summary>
    /// Copies only the requested page slice and rewrites sort indexes to match the requested sort direction.
    /// </summary>
    private static List<PlexMediaSlimDTO> CreatePageItems(
        IReadOnlyList<PlexMediaSlimDTO> items,
        bool descending,
        int offset,
        int pageSize)
    {
        var pageItems = new List<PlexMediaSlimDTO>(Math.Min(pageSize, Math.Max(items.Count - offset, 0)));
        var end = Math.Min(offset + pageSize, items.Count);

        for (var i = offset; i < end; i++)
        {
            var sourceIndex = descending ? items.Count - i - 1 : i;
            var item = items[sourceIndex];
            pageItems.Add(item with
            {
                SortIndex = i + 1,
                Qualities = item.Qualities.ToList(),
            });
        }

        return pageItems;
    }

    private static List<MediaNavigationIndexDTO> RebuildNavigationIndexes(
        IReadOnlyList<PlexMediaSlimDTO> sourceItems,
        string sortField)
    {
        var rows = new List<MediaNavigationIndexRow>(sourceItems.Count);
        for (var i = sourceItems.Count - 1; i >= 0; i--)
        {
            var x = sourceItems[i];
            rows.Add(new MediaNavigationIndexRow(
                x.SearchTitle,
                x.Year,
                x.Qualities.FirstOrDefault()?.Quality.ToId(),
                x.Duration,
                x.AddedAt,
                x.UpdatedAt,
                x.MediaSize));
        }

        return MediaNavigationIndexBuilder.Build(rows, sortField);
    }

    // ── Invalidation helpers ───────────────────────────────────────

    private int MarkKeysContainingLibraryAsDirty<TKey, TValue>(
        ConcurrentDictionary<TKey, TValue> dictionary,
        Func<TKey, bool> matches)
        where TKey : notnull
    {
        var matchingKeys = dictionary.Keys.Where(matches).ToList();
        foreach (var key in matchingKeys)
        {
            if (key is MediaQuerySortedListKey sortedListKey)
            {
                _buildVersions.AddOrUpdate(sortedListKey, 1, (_, v) => v + 1);
                _dirtyKeys.TryAdd(sortedListKey, true);
                QueueSnapshotRefresh(sortedListKey);
            }
        }

        return matchingKeys.Count;
    }

    private static int CountKeysContainingLibrary<TKey, TValue>(
        ConcurrentDictionary<TKey, TValue> dictionary,
        Func<TKey, bool> matches)
        where TKey : notnull => dictionary.Keys.Count(matches);

    // ── Factory helpers ────────────────────────────────────────────

    private MediaQueryFilter CreateWarmupFilter(PlexMediaType mediaType, string sortField) => new()
    {
        MediaType = mediaType,
        PlexLibraryId = 0,
        FilterOfflineMedia = _generalSettings.HideMediaFromOfflineServers,
        FilterOwnedMedia = _generalSettings.HideMediaFromOwnedServers,
        Parameters = new FlexQueryParameters { Query = null, Filter = null, Sort = $"{sortField}:asc", Page = null, PageSize = null },
    };

    private Task<Result<PagedMediaQueryResult>> BypassCacheAsync(MediaQueryFilter filter, CancellationToken cancellationToken, string? reason)
    {
        if (!string.IsNullOrWhiteSpace(reason))
            _log.Here().Debug("Bypassing media query cache: {Reason}", reason);

        return _commandExecutor.Send(new GetMediaByTypeCommand { Filter = filter }, cancellationToken);
    }

    private static MediaQueryMetadataSnapshot CreateMetadataSnapshot(MediaQueryMetadataKey key, PagedMediaQueryResult result) => new()
    {
        Key = key,
        Roles = result.Roles.ToList(),
        Countries = result.Countries.ToList(),
        Genres = result.Genres.ToList(),
        Qualities = result.Qualities.ToList(),
        TotalCount = result.TotalCount,
        MediaCount = result.MediaCount,
        MovieCount = result.MovieCount,
        TvShowCount = result.TvShowCount,
        SeasonCount = result.SeasonCount,
        EpisodeCount = result.EpisodeCount,
        TotalMovieCount = result.TotalMovieCount,
        TotalTvShowCount = result.TotalTvShowCount,
        TotalSeasonCount = result.TotalSeasonCount,
        TotalEpisodeCount = result.TotalEpisodeCount,
        MediaSize = result.MediaSize,
        TotalMediaSize = result.TotalMediaSize,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static MediaNavigationIndexDTO CopyNavigationIndex(MediaNavigationIndexDTO index) => new()
    {
        Label = index.Label,
        Index = index.Index,
    };

    // ── Nested types ───────────────────────────────────────────────

    /// <summary>
    /// Bundles metadata and sorted-list snapshots produced by a single build so they can be stored atomically.
    /// </summary>
    private sealed record MediaQueryBuildResult(
        MediaQueryMetadataSnapshot Metadata,
        MediaQuerySortedListSnapshot SortedList);
}
