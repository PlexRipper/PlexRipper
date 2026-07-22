using FlexQuery.NET;
using FlexQuery.NET.Constants;
using FlexQuery.NET.Models;
using FlexQuery.NET.Parsers;
using Reaparr.Application.Contracts;

namespace Reaparr.Data;

public class GetMediaByTypeCommandValidator : AbstractValidator<GetMediaByTypeCommand>
{
    public GetMediaByTypeCommandValidator()
    {
        RuleFor(x => x).NotNull();

        RuleFor(x => x.Filter).NotNull();

        RuleFor(x => x.Filter.MediaType)
            .Must(mediaType => mediaType is not PlexMediaType.None and not PlexMediaType.Unknown)
            .When(x => x.Filter.PlexLibraryId == 0)
            .WithMessage("MediaType is required when PlexLibraryId is 0.");
    }
}

public class GetMediaByTypeCommandHandler : ICommandHandler<GetMediaByTypeCommand, Result<PagedMediaQueryResult>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    private readonly PagedMediaQueryResult _response = new();

    public GetMediaByTypeCommandHandler(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<PagedMediaQueryResult>> ExecuteAsync(
        GetMediaByTypeCommand command,
        CancellationToken ct)
    {
        var filter = command.Filter;
        _response.QueryHash = filter.QueryHash;
        var plexLibraryId = filter.PlexLibraryId;

        // For a specific library request, verify the library is enabled.
        if (plexLibraryId > 0)
        {
            var requestedLibrary = await _dbContext.PlexLibraries
                .IgnoreIsEnabledFilter()
                .Select(x => new { x.Id, x.IsEnabled })
                .FirstOrDefaultAsync(x => x.Id == plexLibraryId, ct);

            if (requestedLibrary is null || !requestedLibrary.IsEnabled)
                return Result.Ok(_response);
        }

        var allowedPlexLibraryIds = new List<int>();

        if (plexLibraryId > 0)
        {
            allowedPlexLibraryIds.Add(plexLibraryId);
        }
        else
        {
            var hasPlexAccounts = await _dbContext.PlexAccounts.AnyAsync(ct);

            // Get only enabled servers and libraries that still have access through at least one Plex account.
            var serverList = await _dbContext.PlexServers
                .Where(server => !hasPlexAccounts || server.PlexAccountServers.Any())
                .Select(server => new
                {
                    server.Id,
                    PlexLibraryIds = server.PlexLibraries
                        .Where(library => !hasPlexAccounts || library.PlexAccountLibraries.Any())
                        .Select(library => library.Id)
                        .ToList(),
                })
                .ToListAsync(ct);

            allowedPlexLibraryIds = serverList.SelectMany(x => x.PlexLibraryIds).ToList();
            if (filter.FilterOwnedMedia)
            {
                var ownedPlexLibraryIds = await _dbContext.PlexLibraries
                    .WhereIsOwned()
                    .Select(x => x.Id)
                    .ToListAsync(ct);

                allowedPlexLibraryIds.RemoveAll(ownedPlexLibraryIds.Contains);
            }

            if (filter.FilterOfflineMedia)
            {
                foreach (var server in serverList)
                {
                    var isServerOnline = await _dbContext.IsServerOnline(server.Id, ct);
                    if (!isServerOnline)
                    {
                        allowedPlexLibraryIds.RemoveAll(x => server.PlexLibraryIds.Contains(x));
                    }
                }
            }
        }

        var options = QueryOptionsParser.Parse(filter.Parameters);
        var page = Math.Max(filter.Parameters.Page ?? 1, 1);
        var pageSize = Math.Max(filter.Parameters.PageSize ?? 0, 0);
        options.Paging.Disabled = pageSize == 0;

        _response.Page = page;
        _response.PageSize = pageSize;

        if (!allowedPlexLibraryIds.Any())
            return Result.Ok(_response);

        var hasUserFilters = options.HasFiltersApplied() || filter.ComparisonState.HasValue;
        options = WithServerLibraryScope(options, allowedPlexLibraryIds, plexLibraryId);

        ApplyDefaultMediaSort(options, plexLibraryId);
        NormalizeAllLibrarySort(options, plexLibraryId);

        // TODO Deduplicate
        switch (filter.MediaType)
        {
            case PlexMediaType.Movie:
            {
                var movieQuery = _dbContext.PlexMovies
                    .IncludeMediaData()
                    .Include(x => x.Actors)
                    .Include(x => x.Countries)
                    .Include(x => x.Genres)
                    .ApplyFilter(options)
                    .ApplySort(options);

                if (filter.ComparisonState.HasValue)
                {
                    var movies = await movieQuery.ToListAsync(ct);
                    var movieDtos = movies.Select(x => x.ToSlimDTO()).ToList();
                    await ApplyComparisonStateAsync(movieDtos, plexLibraryId, PlexMediaType.Movie, ct);

                    var filteredDtos = movieDtos
                        .Where(x => x.ComparisonState == filter.ComparisonState.Value)
                        .ToList();
                    var filteredIds = filteredDtos.Select(x => x.Id).ToHashSet();
                    var filteredMovies = movies.Where(x => filteredIds.Contains(x.Id)).ToList();

                    SetNavigationIndexes(filteredMovies.Select(x => new MediaNavigationIndexRow(
                        x.SearchTitle,
                        x.Year,
                        (int?)x.Quality,
                        x.Duration,
                        x.AddedAt,
                        x.UpdatedAt,
                        x.MediaSize
                    )), options);

                    _response.TotalCount = filteredDtos.Count;
                    _response.MediaSize = filteredDtos.Sum(x => x.MediaSize);
                    _response.TotalMediaSize = _response.MediaSize;

                    var pagedDtos = ApplyDtoPaging(filteredDtos, page, pageSize);
                    var pagedIds = pagedDtos.Select(x => x.Id).ToHashSet();
                    var pagedMovies = filteredMovies.Where(x => pagedIds.Contains(x.Id)).ToList();

                    _response.Items = pagedDtos;
                    _response.Roles.AddRange(pagedMovies.SelectMany(x => x.Actors)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Countries.AddRange(pagedMovies.SelectMany(x => x.Countries)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Genres.AddRange(pagedMovies.SelectMany(x => x.Genres)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Qualities.AddRange(
                        pagedMovies.SelectMany(x => x.MediaDataList)
                            .Select(x => x.Quality.ToId())
                            .Distinct()
                            .OrderBy(x => x)
                    );
                }
                else
                {
                    await SetNavigationIndexes(movieQuery.Select(x => new MediaNavigationIndexRow(
                        x.SearchTitle,
                        x.Year,
                        (int?)x.Quality,
                        x.Duration,
                        x.AddedAt,
                        x.UpdatedAt,
                        x.MediaSize
                    )), options, ct);

                    _response.TotalCount = await movieQuery.CountAsync(ct);
                    _response.MediaSize = await movieQuery.SumAsync(x => x.MediaSize, ct);
                    _response.TotalMediaSize = _response.MediaSize;

                    var movies = await movieQuery
                        .ApplyPaging(options)
                        .ToListAsync(ct);

                    var movieDtos = movies.Select(x => x.ToSlimDTO()).ToList();
                    await _commandExecutor.Send(
                        new ApplyComparisonStateCommand(movieDtos, plexLibraryId, PlexMediaType.Movie), ct);

                    _response.Items = movieDtos;
                    _response.Roles.AddRange(movies.SelectMany(x => x.Actors)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Countries.AddRange(movies.SelectMany(x => x.Countries)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Genres.AddRange(movies.SelectMany(x => x.Genres)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Qualities.AddRange(
                        movies.SelectMany(x => x.MediaDataList).Select(x => x.Quality.ToId()).Distinct().OrderBy(x => x)
                    );
                }

                break;
            }
            case PlexMediaType.TvShow:
            {
                var tvShowQuery = _dbContext.PlexTvShows
                    .Include(x => x.Qualities)
                    .Include(x => x.Actors)
                    .Include(x => x.Countries)
                    .Include(x => x.Genres)
                    .ApplyFilter(options)
                    .ApplySort(options);

                if (filter.ComparisonState.HasValue)
                {
                    var tvShows = await tvShowQuery.ToListAsync(ct);
                    var tvShowDtos = tvShows.Select(x => x.ToSlimDTOMapper()).ToList();
                    await ApplyComparisonStateAsync(tvShowDtos, plexLibraryId, PlexMediaType.TvShow, ct);

                    var filteredDtos = tvShowDtos
                        .Where(x => x.ComparisonState == filter.ComparisonState.Value)
                        .ToList();
                    var filteredIds = filteredDtos.Select(x => x.Id).ToHashSet();
                    var filteredTvShows = tvShows.Where(x => filteredIds.Contains(x.Id)).ToList();

                    SetNavigationIndexes(filteredTvShows.Select(x => new MediaNavigationIndexRow(
                        x.SearchTitle,
                        x.Year,
                        (int?)x.Quality,
                        x.Duration,
                        x.AddedAt,
                        x.UpdatedAt,
                        x.MediaSize
                    )), options);

                    _response.TotalCount = filteredDtos.Count;
                    _response.MediaSize = filteredDtos.Sum(x => x.MediaSize);
                    _response.TotalMediaSize = _response.MediaSize;

                    var pagedDtos = ApplyDtoPaging(filteredDtos, page, pageSize);
                    var pagedIds = pagedDtos.Select(x => x.Id).ToHashSet();
                    var pagedTvShows = filteredTvShows.Where(x => pagedIds.Contains(x.Id)).ToList();

                    _response.Items = pagedDtos;
                    _response.Roles.AddRange(pagedTvShows.SelectMany(x => x.Actors)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Countries.AddRange(pagedTvShows.SelectMany(x => x.Countries)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Genres.AddRange(pagedTvShows.SelectMany(x => x.Genres)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Qualities.AddRange(
                        pagedTvShows.SelectMany(x => x.Qualities)
                            .Select(x => x.Quality.ToId())
                            .Distinct()
                            .OrderBy(x => x)
                    );
                }
                else
                {
                    await SetNavigationIndexes(tvShowQuery.Select(x => new MediaNavigationIndexRow(
                        x.SearchTitle,
                        x.Year,
                        (int?)x.Quality,
                        x.Duration,
                        x.AddedAt,
                        x.UpdatedAt,
                        x.MediaSize
                    )), options, ct);

                    _response.TotalCount = await tvShowQuery.CountAsync(ct);
                    _response.MediaSize = await tvShowQuery.SumAsync(x => x.MediaSize, ct);
                    _response.TotalMediaSize = _response.MediaSize;

                    var tvShows = await tvShowQuery
                        .ApplyPaging(options)
                        .ToListAsync(ct);

                    var tvShowDtos = tvShows.Select(x => x.ToSlimDTOMapper()).ToList();
                    await _commandExecutor.Send(
                        new ApplyComparisonStateCommand(tvShowDtos, plexLibraryId, PlexMediaType.TvShow), ct);

                    _response.Items = tvShowDtos;
                    _response.Roles.AddRange(tvShows.SelectMany(x => x.Actors)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Countries.AddRange(tvShows.SelectMany(x => x.Countries)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Genres.AddRange(tvShows.SelectMany(x => x.Genres)
                        .Select(x => x.Id)
                        .Distinct()
                        .OrderBy(x => x));
                    _response.Qualities.AddRange(
                        tvShows.SelectMany(x => x.Qualities).Select(x => x.Quality.ToId()).Distinct().OrderBy(x => x)
                    );
                }

                break;
            }
            default:
                return Result.Fail(
                    $"Type {filter.MediaType} is not supported for retrieving the PlexMedia data by library id"
                );
        }

        var effectivePageSize = _response.PageSize > 0 ? _response.PageSize : _response.Items.Count;
        var offset = (_response.Page - 1) * effectivePageSize;

        for (var i = 0; i < _response.Items.Count; i++)
        {
            var slimDTO = _response.Items[i];

            slimDTO.SortIndex = offset + i + 1;
        }

        await SetCounts(hasUserFilters, allowedPlexLibraryIds, filter.MediaType, ct);

        return Result.Ok(_response);
    }

    /// <summary>
    /// Filter by allowed <see cref="PlexLibrary"/> based on previous filtering
    /// </summary>
    private QueryOptions WithServerLibraryScope(
        QueryOptions options,
        List<int> allowedPlexLibraryIds,
        int requestedPlexLibraryId
    )
    {
        var scopedLibraryIds = requestedPlexLibraryId > 0
            ? allowedPlexLibraryIds.Where(x => x == requestedPlexLibraryId).ToList()
            : allowedPlexLibraryIds;

        if (scopedLibraryIds.Count == 0)
        {
            options.Filter = new FilterGroup
            {
                Logic = LogicOperator.And,
                Filters =
                [
                    new FilterCondition
                    {
                        Field = nameof(BasePlexMedia.PlexLibraryId),
                        Operator = FilterOperators.Equal,
                        Value = int.MinValue.ToString(),
                    },
                ],
            };

            return options;
        }

        var serverFilter = new FilterGroup
        {
            Logic = LogicOperator.And,
            Filters =
            [
                new FilterCondition
                {
                    Field = nameof(BasePlexMedia.PlexLibraryId),
                    Operator = scopedLibraryIds.Count == 1 ? FilterOperators.Equal : FilterOperators.In,
                    Value = scopedLibraryIds.Count == 1
                        ? scopedLibraryIds.First().ToString()
                        : string.Join(',', scopedLibraryIds),
                },
            ],
        };

        if (options.Filter is null)
        {
            options.Filter = serverFilter;
            return options;
        }

        options.Filter = new FilterGroup
        {
            Logic = LogicOperator.And,
            Groups = [options.Filter, serverFilter],
        };

        return options;
    }

    private async Task SetNavigationIndexes(
        IQueryable<MediaNavigationIndexRow> rows,
        QueryOptions options,
        CancellationToken ct)
    {
        SetNavigationIndexes(await rows.ToListAsync(ct), options);
    }

    private async Task ApplyComparisonStateAsync(
        List<PlexMediaSlimDTO> items,
        int plexLibraryId,
        PlexMediaType mediaType,
        CancellationToken ct)
    {
        if (items.Count == 0)
            return;

        if (plexLibraryId > 0)
        {
            await _commandExecutor.Send(new ApplyComparisonStateCommand(items, plexLibraryId, mediaType), ct);
            return;
        }

        var groupedItemIndexes = items
            .Select((item, index) => new { item.PlexLibraryId, Index = index })
            .Where(x => x.PlexLibraryId > 0)
            .GroupBy(x => x.PlexLibraryId);

        foreach (var group in groupedItemIndexes)
        {
            var indexes = group.Select(x => x.Index).ToList();
            var libraryItems = indexes.Select(index => items[index]).ToList();
            await _commandExecutor.Send(new ApplyComparisonStateCommand(libraryItems, group.Key, mediaType), ct);

            for (var i = 0; i < indexes.Count; i++)
                items[indexes[i]] = libraryItems[i];
        }
    }

    private void SetNavigationIndexes(IEnumerable<MediaNavigationIndexRow> rows, QueryOptions options)
    {
        _response.NavigationIndexes = MediaNavigationIndexBuilder.Build(
            rows,
            options.Sort.FirstOrDefault()?.Field
        );
    }

    private static List<PlexMediaSlimDTO> ApplyDtoPaging(List<PlexMediaSlimDTO> items, int page, int pageSize)
    {
        if (pageSize == 0)
            return items;

        return items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private void NormalizeAllLibrarySort(QueryOptions options, int plexLibraryId)
    {
        if (plexLibraryId > 0 || !options.Sort.Any())
            return;

        foreach (var sort in options.Sort.Where(x => IsSortIndexField(x.Field)))
            sort.Field = nameof(BasePlexMedia.SearchTitle);
    }

    private static bool IsSortIndexField(string? field) => field is nameof(BasePlexMedia.SortIndex) or "sortIndex";

    private void ApplyDefaultMediaSort(QueryOptions options, int plexLibraryId)
    {
        if (options.Sort.Count > 0 || plexLibraryId == 0)
            return;

        options.Sort.Add(
            new SortNode
            {
                Field = nameof(BasePlexMedia.SortIndex),
                Descending = false,
            }
        );
    }

    private async Task SetCounts(
        bool hasUserFilters,
        List<int> allowedPlexLibraryIds,
        PlexMediaType mediaType,
        CancellationToken ct)
    {
        var allScopedLibraries = await _dbContext.PlexLibraries
            .Where(x => allowedPlexLibraryIds.Contains(x.Id))
            .ToListAsync(ct);

        _response.TotalMovieCount = mediaType == PlexMediaType.Movie
            ? _response.TotalCount
            : allScopedLibraries.Sum(x => x.MovieCount);
        _response.TotalTvShowCount = mediaType == PlexMediaType.TvShow
            ? _response.TotalCount
            : allScopedLibraries.Sum(x => x.TvShowCount);
        _response.TotalSeasonCount = allScopedLibraries.Sum(x => x.SeasonCount);
        _response.TotalEpisodeCount = allScopedLibraries.Sum(x => x.EpisodeCount);

        if (hasUserFilters)
        {
            _response.MediaCount = _response.TotalCount;
            _response.MovieCount = mediaType == PlexMediaType.Movie ? _response.TotalCount : 0;
            _response.TvShowCount = mediaType == PlexMediaType.TvShow ? _response.TotalCount : 0;
            _response.SeasonCount = _response.Items.Where(x => x.Type == PlexMediaType.TvShow).Sum(x => x.ChildCount);
            _response.EpisodeCount =
                _response.Items.Where(x => x.Type == PlexMediaType.TvShow).Sum(x => x.GrandChildCount);
            return;
        }

        _response.MovieCount = mediaType == PlexMediaType.Movie ? _response.TotalCount : 0;
        _response.TvShowCount = mediaType == PlexMediaType.TvShow ? _response.TotalCount : 0;
        _response.SeasonCount = mediaType == PlexMediaType.TvShow ? _response.TotalSeasonCount : 0;
        _response.EpisodeCount = mediaType == PlexMediaType.TvShow ? _response.TotalEpisodeCount : 0;
        _response.MediaCount = _response.TotalCount;
    }
}