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
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    private PagedMediaQueryResult _response = new();

    public GetMediaByTypeCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetMediaByTypeCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<PagedMediaQueryResult>> ExecuteAsync(
        GetMediaByTypeCommand command,
        CancellationToken ct)
    {
        var filter = command.Filter;
        var plexLibraryId = filter.PlexLibraryId;

        var allowedPlexLibraryIds = new List<int>();

        if (plexLibraryId > 0)
        {
            allowedPlexLibraryIds.Add(plexLibraryId);
        }
        else
        {
            // Get only enabled servers
            var serverList = await _dbContext.PlexServers
                .Select(server => new { server.Id, PlexLibraryIds = server.PlexLibraries.Select(x => x.Id).ToList() })
                .ToListAsync(ct);

            allowedPlexLibraryIds = serverList.SelectMany(x => x.PlexLibraryIds).ToList();
            if (filter.FilterOwnedMedia)
            {
                var ownedPlexLibraries = await _dbContext
                    .PlexAccountLibraries.Where(x => x.IsLibraryOwned)
                    .Select(x => x.PlexLibraryId)
                    .ToListAsync(ct);

                allowedPlexLibraryIds.RemoveAll(x => ownedPlexLibraries.Contains(x));
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

        if (!allowedPlexLibraryIds.Any())
            return Result.Ok(_response);

        var options = QueryOptionsParser.Parse(filter.Parameters);
        options = WithServerLibraryScope(options, allowedPlexLibraryIds, plexLibraryId);

        ApplyDefaultMediaSort(options, plexLibraryId);

        switch (filter.MediaType)
        {
            case PlexMediaType.Movie:
            {
                _response.Items = await _dbContext.PlexMovies
                    .IncludeMediaData()
                    .ApplyFilter(options)
                    .ApplySort(options)
                    .ApplyPaging(options)
                    .Select(x => x.ToSlimDTO())
                    .ToListAsync(ct);

                break;
            }
            case PlexMediaType.TvShow:
            {
                _response.Items = await _dbContext.PlexTvShows
                    .Include(x => x.Qualities)
                    .ApplyFilter(options)
                    .ApplySort(options)
                    .ApplyPaging(options)
                    .Select(x => x.ToSlimDTOMapper())
                    .ToListAsync(ct);

                break;
            }
            default:
                return Result.Fail(
                    $"Type {filter.MediaType} is not supported for retrieving the PlexMedia data by library id"
                );
        }

        for (var i = 0; i < _response.Items.Count; i++)
        {
            var slimDTO = _response.Items[i];

            slimDTO.SortIndex = i + 1;
        }

        await SetCounts(options, allowedPlexLibraryIds);

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

    private async Task SetCounts(QueryOptions options, List<int> allowedPlexLibraryIds)
    {
        if (options.HasFiltersApplied())
        {
            _response.MediaCount = _response.Items.Count;
            _response.MovieCount = _response.Items.Count(x => x.Type == PlexMediaType.Movie);
            _response.TvShowCount = _response.Items.Count(x => x.Type == PlexMediaType.TvShow);
            _response.SeasonCount = _response.Items.Where(x => x.Type == PlexMediaType.TvShow).Sum(x => x.ChildCount);
            _response.EpisodeCount =
                _response.Items.Where(x => x.Type == PlexMediaType.TvShow).Sum(x => x.GrandChildCount);
            _response.MediaSize = _response.Items.Sum(x => x.MediaSize);
            _response.TotalCount = _response.MediaCount;
        }
        else
        {
            var plexLibraries = await _dbContext.PlexLibraries.Where(x => allowedPlexLibraryIds.Contains(x.Id))
                .ToListAsync();

            foreach (var plexLibrary in plexLibraries)
            {
                _response.MovieCount += plexLibrary.MovieCount;
                _response.TvShowCount += plexLibrary.TvShowCount;
                _response.SeasonCount += plexLibrary.SeasonCount;
                _response.EpisodeCount += plexLibrary.EpisodeCount;
                _response.MediaSize += plexLibrary.MediaSize;
                _response.MediaCount += plexLibrary.MediaCount;
                _response.TotalCount += plexLibrary.MediaCount;
            }
        }
    }
}