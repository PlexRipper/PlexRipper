using System.Collections.Concurrent;
using FlexQuery.NET.Models;
using Reaparr.Application.Contracts;

namespace Reaparr.Data;

/// <summary>
/// Caches media overview query snapshots so page changes and sort direction changes do not repeatedly execute the full media query.
/// </summary>
public sealed class MediaQueryCache : IMediaQueryCache
{
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
    
    private static readonly HashSet<string> _supportedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "sortTitle",
        "title",
        "sortIndex",
        nameof(BasePlexMedia.SortIndex),
        nameof(BasePlexMedia.SearchTitle),
        "year",
        nameof(BasePlexMedia.Year),
        "addedAt",
        nameof(BasePlexMedia.AddedAt),
        "updatedAt",
        nameof(BasePlexMedia.UpdatedAt),
        "duration",
        nameof(BasePlexMedia.Duration),
        "mediaSize",
        nameof(BasePlexMedia.MediaSize),
        "quality",
    };

    private readonly ConcurrentDictionary<MediaQueryMetadataKey, MediaQueryMetadataSnapshot> _metadataSnapshots = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, MediaQuerySortedListSnapshot> _sortedListSnapshots = new();

    private readonly ConcurrentDictionary<MediaQuerySortedListKey, Lazy<Task<Result<MediaQuerySortedListSnapshot>>>> _builds = new();
    private readonly ConcurrentDictionary<MediaQuerySortedListKey, long> _buildVersions = new();

    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ILogger _log;

    /// <summary>
    /// Creates the cache with command execution for snapshot builds and database access for resolving library scopes.
    /// </summary>
    public MediaQueryCache(ILogger log, ICommandExecutor commandExecutor, IReaparrDbContextFactory dbContextFactory)
    {
        _log = log.ForContext<MediaQueryCache>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<Result<PagedMediaQueryResult>> GetMediaAsync(
        MediaQueryFilter filter,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(filter.Parameters.Query))
        {
            _log.Here().Debug("Bypassing media query cache: query parameter is set");
            return await _commandExecutor.Send(new GetMediaByTypeCommand { Filter = filter }, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(filter.Parameters.Filter))
        {
            _log.Here().Debug("Bypassing media query cache: filter parameter is set");
            return await _commandExecutor.Send(new GetMediaByTypeCommand { Filter = filter }, cancellationToken);
        }

        var libraryIds = await ResolveLibraryIdsAsync(filter, cancellationToken);
        var normalizedSortResult = NormalizeSort(filter.Parameters.Sort, libraryIds.Count);
        if (normalizedSortResult is null)
        {
            _log.Here().Debug("Bypassing media query cache: unsupported sort {Sort}", filter.Parameters.Sort);
            return await _commandExecutor.Send(new GetMediaByTypeCommand { Filter = filter }, cancellationToken);
        }

        var metadataKey = new MediaQueryMetadataKey(
            filter.MediaType,
            libraryIds,
            filter.FilterOfflineMedia,
            filter.FilterOwnedMedia
        );
        var sortedListKey = new MediaQuerySortedListKey(metadataKey, normalizedSortResult.StoredField);

        if (_metadataSnapshots.TryGetValue(metadataKey, out var metadataSnapshot)
            && _sortedListSnapshots.TryGetValue(sortedListKey, out var sortedListSnapshot))
        {
            _log.Here().Debug("Media query cache hit for {MediaType} sorted by {SortField}", filter.MediaType, sortedListKey.NormalizedAscendingSortField);
            return Result.Ok(CreatePage(filter, metadataSnapshot, sortedListSnapshot, normalizedSortResult.Descending));
        }

        _log.Here().Debug("Media query cache miss for {MediaType} sorted by {SortField}", filter.MediaType, sortedListKey.NormalizedAscendingSortField);
        var buildResult = await GetOrBuildSortedListSnapshotAsync(
            filter,
            sortedListKey,
            normalizedSortResult,
            cancellationToken
        );

        if (buildResult.IsFailed)
            return Result.Fail(buildResult.Errors);

        if (!_metadataSnapshots.TryGetValue(metadataKey, out metadataSnapshot))
            return Result.Fail("Media query cache build completed without metadata snapshot.");

        return Result.Ok(CreatePage(filter, metadataSnapshot, buildResult.Value, normalizedSortResult.Descending));
    }

    /// <inheritdoc />
    public async Task BuildCache()
    {
        var warmupTasks = new List<Task<Result<PagedMediaQueryResult>>>();

        foreach (var mediaType in new[] { PlexMediaType.Movie, PlexMediaType.TvShow })
        {
            foreach (var sortField in _warmupSortFields)
            {
                warmupTasks.Add(GetMediaAsync(
                    new MediaQueryFilter
                    {
                        MediaType = mediaType,
                        PlexLibraryId = 0,
                        FilterOfflineMedia = false,
                        FilterOwnedMedia = false,
                        Parameters = new FlexQueryParameters
                        {
                            Query = null,
                            Filter = null,
                            Sort = $"{sortField}:asc",
                            Page = null,
                            PageSize = null,
                        },
                    },
                    CancellationToken.None
                ));
            }
        }

        var results = await Task.WhenAll(warmupTasks);
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
        var affectedLibraryIds = plexLibraryIds.Where(x => x > 0).ToHashSet();
        if (affectedLibraryIds.Count == 0)
            return;

        var removedMetadataCount = RemoveKeysContainingLibrary(_metadataSnapshots, affectedLibraryIds);
        var removedSortedListCount = RemoveKeysContainingLibrary(_sortedListSnapshots, affectedLibraryIds);
        var removedBuildCount = RemoveKeysContainingLibrary(_builds, affectedLibraryIds);
        IncrementBuildVersions(affectedLibraryIds);

        _log.Here()
            .Information(
                "Invalidated media query cache for libraries {PlexLibraryIds}: {Reason}. Removed {MetadataCount} metadata snapshots, {SortedListCount} sorted lists, and {BuildCount} in-flight builds",
                affectedLibraryIds,
                reason,
                removedMetadataCount,
                removedSortedListCount,
                removedBuildCount
            );
    }

    /// <summary>
    /// Ensures only one build runs per sorted-list key and stores the completed snapshot if it was not invalidated mid-build.
    /// </summary>
    private async Task<Result<MediaQuerySortedListSnapshot>> GetOrBuildSortedListSnapshotAsync(
        MediaQueryFilter filter,
        MediaQuerySortedListKey sortedListKey,
        NormalizedMediaSort normalizedSort,
        CancellationToken cancellationToken)
    {
        var buildVersion = _buildVersions.GetOrAdd(sortedListKey, 0);
        var lazyBuild = _builds.GetOrAdd(
            sortedListKey,
            _ => new Lazy<Task<Result<MediaQuerySortedListSnapshot>>>(
                () => BuildSortedListSnapshotAsync(filter, sortedListKey, normalizedSort, cancellationToken),
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );

        try
        {
            var result = await lazyBuild.Value;
            if (_buildVersions.TryGetValue(sortedListKey, out var currentVersion) && currentVersion != buildVersion)
                return Result.Fail("Media query cache build was invalidated before completion.");

            if (result.IsSuccess)
                _sortedListSnapshots[sortedListKey] = result.Value;

            return result;
        }
        finally
        {
            if (_builds.TryGetValue(sortedListKey, out var currentBuild) && ReferenceEquals(currentBuild, lazyBuild))
                _builds.TryRemove(sortedListKey, out _);
        }
    }

    /// <summary>
    /// Builds the full ascending snapshot by executing the existing media query command with paging and user filters removed.
    /// </summary>
    private async Task<Result<MediaQuerySortedListSnapshot>> BuildSortedListSnapshotAsync(
        MediaQueryFilter originalFilter,
        MediaQuerySortedListKey sortedListKey,
        NormalizedMediaSort normalizedSort,
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
                        Sort = $"{normalizedSort.HandlerField}:asc",
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
            cancellationToken
        );

        if (commandResult.IsFailed)
            return Result.Fail(commandResult.Errors);

        var result = commandResult.Value;
        _metadataSnapshots[sortedListKey.MetadataKey] = new MediaQueryMetadataSnapshot
        {
            Key = sortedListKey.MetadataKey,
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

        return Result.Ok(new MediaQuerySortedListSnapshot
        {
            Key = sortedListKey,
            Items = result.Items.Select(CopyItem).ToList(),
            NavigationIndexes = result.NavigationIndexes.Select(CopyNavigationIndex).ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
        });
    }

    /// <summary>
    /// Resolves the effective library id set for the request, applying account access, owned-library, and offline-server filters for all-library requests.
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
            var ownedPlexLibraries = await dbContext.PlexAccountLibraries
                .Where(x => x.IsLibraryOwned)
                .Select(x => x.PlexLibraryId)
                .ToListAsync(cancellationToken);

            allowedPlexLibraryIds.RemoveAll(ownedPlexLibraries.Contains);
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

    /// <summary>
    /// Normalizes supported sort expressions into a cache key and indicates whether the request should be reversed at page creation.
    /// </summary>
    private static NormalizedMediaSort? NormalizeSort(string? sort, int libraryCount)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            var titleField = libraryCount == 1 ? "sortIndex" : nameof(BasePlexMedia.SearchTitle);
            return new NormalizedMediaSort(titleField, titleField, false);
        }

        var segments = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 1)
            return null;

        var parts = segments[0].Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2)
            return null;

        var requestedField = parts[0];
        if (!_supportedSortFields.Contains(requestedField))
            return null;

        var direction = parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "asc";
        var descending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        if (!descending && !direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
            return null;

        var normalizedField = NormalizeSortField(requestedField, libraryCount);
        return new NormalizedMediaSort(normalizedField, normalizedField, descending);
    }

    /// <summary>
    /// Maps supported sort aliases to the canonical field name used by the snapshot build.
    /// </summary>
    private static string NormalizeSortField(string requestedField, int libraryCount)
    {
        if (requestedField.Equals("sortTitle", StringComparison.OrdinalIgnoreCase)
            || requestedField.Equals("title", StringComparison.OrdinalIgnoreCase)
            || requestedField.Equals("sortIndex", StringComparison.OrdinalIgnoreCase)
            || requestedField.Equals(nameof(BasePlexMedia.SortIndex), StringComparison.OrdinalIgnoreCase)
            || requestedField.Equals(nameof(BasePlexMedia.SearchTitle), StringComparison.OrdinalIgnoreCase))
            return libraryCount == 1 ? "sortIndex" : nameof(BasePlexMedia.SearchTitle);

        return requestedField switch
        {
            nameof(BasePlexMedia.Year) => nameof(BasePlexMedia.Year),
            "year" => nameof(BasePlexMedia.Year),
            nameof(BasePlexMedia.AddedAt) => nameof(BasePlexMedia.AddedAt),
            "addedAt" => nameof(BasePlexMedia.AddedAt),
            nameof(BasePlexMedia.UpdatedAt) => nameof(BasePlexMedia.UpdatedAt),
            "updatedAt" => nameof(BasePlexMedia.UpdatedAt),
            nameof(BasePlexMedia.Duration) => nameof(BasePlexMedia.Duration),
            "duration" => nameof(BasePlexMedia.Duration),
            nameof(BasePlexMedia.MediaSize) => nameof(BasePlexMedia.MediaSize),
            "mediaSize" => nameof(BasePlexMedia.MediaSize),
            "quality" => "quality",
            _ => requestedField,
        };
    }

    /// <summary>
    /// Creates a page response by slicing the cached list, rebuilding descending views when necessary, and copying shared metadata.
    /// </summary>
    private static PagedMediaQueryResult CreatePage(
        MediaQueryFilter filter,
        MediaQueryMetadataSnapshot metadataSnapshot,
        MediaQuerySortedListSnapshot sortedListSnapshot,
        bool descending)
    {
        var normalizedPage = Math.Max(filter.Parameters.Page ?? 1, 1);
        var sourceItems = descending
            ? sortedListSnapshot.Items.Reverse().Select(CopyItem).ToList()
            : sortedListSnapshot.Items.Select(CopyItem).ToList();

        for (var i = 0; i < sourceItems.Count; i++)
            sourceItems[i].SortIndex = i + 1;

        var effectivePageSize = filter.Parameters.PageSize is > 0 ? filter.Parameters.PageSize.Value : sourceItems.Count;
        var offset = effectivePageSize == 0 ? 0 : (normalizedPage - 1) * effectivePageSize;
        var pageItems = sourceItems.Skip(offset).Take(effectivePageSize).ToList();
        var navigationIndexes = descending
            ? RebuildNavigationIndexes(sourceItems, sortedListSnapshot.Key.NormalizedAscendingSortField)
            : sortedListSnapshot.NavigationIndexes.Select(CopyNavigationIndex).ToList();

        return new PagedMediaQueryResult
        {
            QueryHash = filter.QueryHash,
            Page = normalizedPage,
            PageSize = effectivePageSize,
            Items = pageItems,
            NavigationIndexes = navigationIndexes,
            Roles = metadataSnapshot.Roles.ToList(),
            Countries = metadataSnapshot.Countries.ToList(),
            Genres = metadataSnapshot.Genres.ToList(),
            Qualities = metadataSnapshot.Qualities.ToList(),
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
    /// Rebuilds navigation indexes from a reversed source list so descending requests still expose correct anchors.
    /// </summary>
    private static List<MediaNavigationIndexDTO> RebuildNavigationIndexes(
        IReadOnlyCollection<PlexMediaSlimDTO> sourceItems,
        string sortField) => MediaNavigationIndexBuilder.Build(
        sourceItems.Select(x => new MediaNavigationIndexRow(
            x.SearchTitle,
            x.Year,
            x.Qualities.FirstOrDefault()?.Quality.ToId(),
            x.Duration,
            x.AddedAt,
            x.UpdatedAt,
            x.MediaSize
        )),
        sortField
    );

    /// <summary>
    /// Removes dictionary entries whose metadata keys depend on one of the invalidated libraries.
    /// </summary>
    private static int RemoveKeysContainingLibrary<TValue>(
        ConcurrentDictionary<MediaQueryMetadataKey, TValue> dictionary,
        IReadOnlySet<int> libraryIds)
    {
        var keysToRemove = dictionary.Keys.Where(key => key.ContainsAnyLibrary(libraryIds)).ToList();
        foreach (var key in keysToRemove)
            dictionary.TryRemove(key, out _);

        return keysToRemove.Count;
    }

    /// <summary>
    /// Removes dictionary entries whose sorted-list keys depend on one of the invalidated libraries.
    /// </summary>
    private static int RemoveKeysContainingLibrary<TValue>(
        ConcurrentDictionary<MediaQuerySortedListKey, TValue> dictionary,
        IReadOnlySet<int> libraryIds)
    {
        var keysToRemove = dictionary.Keys.Where(key => key.ContainsAnyLibrary(libraryIds)).ToList();
        foreach (var key in keysToRemove)
            dictionary.TryRemove(key, out _);

        return keysToRemove.Count;
    }

    /// <summary>
    /// Advances the invalidation version for in-flight builds whose keys include one of the invalidated libraries.
    /// </summary>
    private void IncrementBuildVersions(IReadOnlySet<int> libraryIds)
    {
        var keysToIncrement = _buildVersions.Keys.Where(key => key.ContainsAnyLibrary(libraryIds)).ToList();
        foreach (var key in keysToIncrement)
            _buildVersions.AddOrUpdate(key, 1, (_, version) => version + 1);
    }

    /// <summary>
    /// Copies a navigation index so cached snapshots do not expose mutable shared instances.
    /// </summary>
    private static MediaNavigationIndexDTO CopyNavigationIndex(MediaNavigationIndexDTO index) => new()
    {
        Label = index.Label,
        Index = index.Index,
    };

    /// <summary>
    /// Copies a media item and its qualities so cached snapshots remain detached from the original command result.
    /// </summary>
    private static PlexMediaSlimDTO CopyItem(PlexMediaSlimDTO item) => item with { Qualities = item.Qualities.ToList() };

    /// <summary>
    /// Stores the canonical sort field to cache, the field sent to the query handler, and whether the requested page should be reversed.
    /// </summary>
    private sealed record NormalizedMediaSort(string StoredField, string HandlerField, bool Descending);
}
