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

    public GetMediaByTypeCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedMediaQueryResult>> ExecuteAsync(
        GetMediaByTypeCommand command,
        CancellationToken ct)
    {
        var filter = command.Filter;
        List<PlexMediaSlimDTO> plexMediaSlimDtos;
        var plexLibraryId = filter.PlexLibraryId;

        // Get only enabled servers
        var serverList = await _dbContext.PlexServers
            .Select(server => new { server.Id, PlexLibraryIds = server.PlexLibraries.Select(x => x.Id).ToList() })
            .ToListAsync(ct);

        var allowedPlexLibraryIds = serverList.SelectMany(x => x.PlexLibraryIds).ToList();
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

        if (plexLibraryId == 0 && !allowedPlexLibraryIds.Any())
            return Result.Ok(new PagedMediaQueryResult
            {
                TotalCount = 0,
                Items = [],
            });

        var options = QueryOptionsParser.Parse(filter.Parameters);

        if (plexLibraryId == 0)
        {
            options = WithServerLibraryScope(options, allowedPlexLibraryIds, plexLibraryId);
        }
        else
        {
            // Specific library requests should ignore owned/offline visibility filters,
            // but still be scoped to the requested library id.
            options = WithServerLibraryScope(options, [plexLibraryId], plexLibraryId);
        }

        ApplyDefaultMediaSort(options, plexLibraryId);

        switch (filter.MediaType)
        {
            case PlexMediaType.Movie:
            {
                var movies = await _dbContext.PlexMovies.IncludeMediaData()
                    .Include(x => x.MediaDataList)
                    .ApplyFilter(options)
                    .ApplySort(options)
                    .ApplyPaging(options)
                    .ToListAsync(ct);

                plexMediaSlimDtos = movies.Select(x => x.ToSlimDTO()).ToList();

                break;
            }
            case PlexMediaType.TvShow:
            {
                var tvShows = await _dbContext.PlexTvShows
                    .Include(x => x.Qualities)
                    .ApplyFilter(options)
                    .ApplySort(options)
                    .ApplyPaging(options)
                    .ToListAsync(ct);

                plexMediaSlimDtos = tvShows.Select(x => x.ToSlimDTOMapper()).ToList();
                break;
            }
            default:
                return Result.Fail(
                    $"Type {filter.MediaType} is not supported for retrieving the PlexMedia data by library id"
                );
        }

        // if (plexLibraryId == 0)
        //     plexMediaSlimDtos = plexMediaSlimDtos.OrderByNatural(x => x.SearchTitle).ToList();

        for (var i = 0; i < plexMediaSlimDtos.Count; i++)
        {
            var slimDTO = plexMediaSlimDtos[i];

            slimDTO.SortIndex = i + 1;
        }

        // If the plexLibraryId is set, we don't need to sort the list again
        return Result.Ok(new PagedMediaQueryResult
        {
            TotalCount = plexMediaSlimDtos.Count,
            Items = plexMediaSlimDtos,
        });
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

    private static QueryExecutionOptions CreateMovieFlexQueryExecutionOptions()
    {
        var options = CreateBaseFlexQueryExecutionOptions(
            filterableFields:
            [
                nameof(BasePlexMedia.Title),
                nameof(BasePlexMedia.SearchTitle),
                nameof(BasePlexMedia.Year),
                nameof(BasePlexMedia.Rating),
                nameof(BasePlexMedia.Studio),
                nameof(BasePlexMedia.ContentRating),
                nameof(BasePlexMedia.PlexLibraryId),
                nameof(BasePlexMedia.PlexServerId),
                "Countries.Id",
                "Genres.Id",
                "Actors.Id",
                "MediaDataList.Quality",
            ]
        );

        options.AllowOperators("MediaDataList.Quality", FilterOperators.Equal, FilterOperators.In);
        return options;
    }

    private static QueryExecutionOptions CreateTvShowFlexQueryExecutionOptions()
    {
        var options = CreateBaseFlexQueryExecutionOptions(
            filterableFields:
            [
                nameof(BasePlexMedia.Title),
                nameof(BasePlexMedia.SearchTitle),
                nameof(BasePlexMedia.Year),
                nameof(BasePlexMedia.Rating),
                nameof(BasePlexMedia.Studio),
                nameof(BasePlexMedia.ContentRating),
                nameof(BasePlexMedia.PlexLibraryId),
                nameof(BasePlexMedia.PlexServerId),
                "Countries.Id",
                "Genres.Id",
                "Actors.Id",
                "Qualities.Quality",
            ]
        );

        options.AllowOperators("Qualities.Quality", FilterOperators.Equal, FilterOperators.In);
        return options;
    }

    private static QueryExecutionOptions CreateBaseFlexQueryExecutionOptions(HashSet<string> filterableFields)
    {
        var sortableFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(BasePlexMedia.Title),
            nameof(BasePlexMedia.SearchTitle),
            nameof(BasePlexMedia.Year),
            nameof(BasePlexMedia.Rating),
            nameof(BasePlexMedia.SortIndex),
            nameof(BasePlexMedia.AddedAt),
            nameof(BasePlexMedia.OriginallyAvailableAt),
        };

        var selectableFields = new HashSet<string>(sortableFields, StringComparer.OrdinalIgnoreCase)
        {
            nameof(BaseEntity.Id),
            nameof(BasePlexMedia.Duration),
            nameof(BasePlexMedia.MediaSize),
            nameof(BasePlexMedia.ChildCount),
            nameof(BasePlexMedia.FullTitle),
        };

        var allowedFields = new HashSet<string>(filterableFields, StringComparer.OrdinalIgnoreCase);
        allowedFields.UnionWith(sortableFields);
        allowedFields.UnionWith(selectableFields);

        var options = new QueryExecutionOptions
        {
            AllowedFields = allowedFields,
            FilterableFields = filterableFields,
            SortableFields = sortableFields,
            SelectableFields = selectableFields,
            MaxFieldDepth = 2,
            StrictFieldValidation = true,
            MaxPageSize = 1000,
            UseNoTracking = true,
        };

        foreach (var field in new[] { "Countries.Id", "Genres.Id", "Actors.Id" })
            options.AllowOperators(field, FilterOperators.Equal, FilterOperators.In);

        foreach (var field in new[] { nameof(BasePlexMedia.PlexLibraryId), nameof(BasePlexMedia.PlexServerId) })
            options.AllowOperators(field, FilterOperators.Equal, FilterOperators.In);

        foreach (var field in new[]
                     { nameof(BasePlexMedia.Title), nameof(BasePlexMedia.SearchTitle), nameof(BasePlexMedia.Studio) })
            options.AllowOperators(field, FilterOperators.Equal, FilterOperators.Contains, FilterOperators.StartsWith);

        return options;
    }

    private static Error CreateFlexQueryValidationError(FlexQuery.NET.Validation.ValidationResult validation) => new(
        "FlexQuery validation failed: "
        + string.Join(
            "; ",
            validation.Errors.Select(error => $"{error.Field} [{error.Code}]: {error.Message}")
        )
    );
}