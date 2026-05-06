namespace Reaparr.Application;

public record GetLibraryMediaMetadataRequest
{
    public int PlexLibraryId { get; init; }

    [QueryParam, BindFrom("mediaType")]
    [DefaultValue(PlexMediaType.None)]
    public PlexMediaType MediaType { get; init; }

    [QueryParam, BindFrom("countryId")]
    [DefaultValue(0)]
    public int CountryId { get; init; }

    [QueryParam, BindFrom("genreId")]
    [DefaultValue(0)]
    public int GenreId { get; init; }

    [QueryParam, BindFrom("roleId")]
    [DefaultValue(0)]
    public int ActorId { get; init; }

    [QueryParam, BindFrom("quality")]
    [DefaultValue(VideoQuality.None)]
    public VideoQuality Quality { get; init; }

    [QueryParam, BindFrom("filterOfflineMedia")]
    [DefaultValue(false)]
    public bool FilterOfflineMedia { get; init; }

    [QueryParam, BindFrom("filterOwnedMedia")]
    [DefaultValue(false)]
    public bool FilterOwnedMedia { get; init; }

    [QueryParam, BindFrom("search")]
    [DefaultValue("")]
    public string Search { get; init; } = string.Empty;

    [QueryParam, BindFrom("sortField")]
    [DefaultValue("sortIndex")]
    public string SortField { get; init; } = "sortIndex";

    [QueryParam, BindFrom("sortDirection")]
    [DefaultValue("asc")]
    public string SortDirection { get; init; } = "asc";
}

public class GetLibraryMediaMetadataRequestValidator : Validator<GetLibraryMediaMetadataRequest>
{
    public GetLibraryMediaMetadataRequestValidator()
    {
        RuleFor(x => x.MediaType)
            .NotEqual(PlexMediaType.None)
            .When(x => x.PlexLibraryId == 0)
            .WithMessage("MediaType must not be 'None' when PlexLibraryId is 0.");

        RuleFor(x => x.SortDirection).Must(x => x is "asc" or "desc");
        RuleFor(x => x.SortField).Must(x => x is "sortIndex" or "year" or "addedAt" or "updatedAt" or "duration" or "mediaSize" or "quality");
    }
}

public class GetLibraryMediaMetadata : BaseEndpoint<GetLibraryMediaMetadataRequest, PlexMediaMetadataDTO>
{
    private const long Gigabyte = 1_000_000_000;

    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/metadata";

    public GetLibraryMediaMetadata(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetLibraryMediaMetadata>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaMetadataDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetLibraryMediaMetadataRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        if (req.PlexLibraryId > 0)
        {
            var plexLibrary = await _dbContext.PlexLibraries.GetAsync(req.PlexLibraryId, ct);
            if (plexLibrary is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
                return;
            }

            var roles = await (
                from la in _dbContext.PlexLibraryActors
                join a in _dbContext.PlexActors on la.PlexActorId equals a.Id
                where la.PlexLibraryId == req.PlexLibraryId
                select new PlexRoleDTO { Id = a.Id, Name = a.Name }
            ).ToListAsync(ct);

            var countries = await (
                from lc in _dbContext.PlexLibraryCountries
                join c in _dbContext.PlexCountries on lc.PlexCountryId equals c.Id
                where lc.PlexLibraryId == req.PlexLibraryId
                select new PlexCountryDTO { Id = c.Id, Name = c.Name }
            ).ToListAsync(ct);

            var genres = await (
                from lg in _dbContext.PlexLibraryGenres
                join g in _dbContext.PlexGenres on lg.PlexGenreId equals g.Id
                where lg.PlexLibraryId == req.PlexLibraryId
                select new PlexGenreDTO { Id = g.Id, Name = g.Name }
            ).ToListAsync(ct);

            var uniqueQualities2 = await GetQualitiesForMediaType(req.MediaType, req.PlexLibraryId, ct);
            var navigationIndexes = await GetNavigationIndexes(req, ct);

            await SendFluentResult(
                Result.Ok(
                    new PlexMediaMetadataDTO
                    {
                        MediaCount = plexLibrary.MediaCount,
                        Roles = roles,
                        Countries = countries,
                        Genres = genres,
                        Qualities = uniqueQualities2,
                        RoleCount = plexLibrary.ActorsCount,
                        CountryCount = plexLibrary.CountriesCount,
                        GenreCount = plexLibrary.GenresCount,
                        QualityCount = uniqueQualities2.Count,
                        NavigationIndexes = navigationIndexes,
                    }
                ),
                ct
            );

            return;
        }

        var mediaCount = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SumAsync(pl => pl.MediaCount, ct);

        var roleCount = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Actors)
            .Select(a => a.Id)
            .Distinct()
            .CountAsync(ct);

        var countryCount = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Countries)
            .Select(c => c.Id)
            .Distinct()
            .CountAsync(ct);

        var genreCount = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Genres)
            .Select(g => g.Id)
            .Distinct()
            .CountAsync(ct);

        var uniqueRoles = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Actors)
            .Select(a => new PlexRoleDTO { Id = a.Id, Name = a.Name })
            .Distinct()
            .ToListAsync(ct);

        var uniqueCountries = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Countries)
            .Select(c => new PlexCountryDTO { Id = c.Id, Name = c.Name })
            .Distinct()
            .ToListAsync(ct);

        var uniqueGenres = await _dbContext
            .PlexLibraries.Where(pl => pl.Type == req.MediaType)
            .SelectMany(pl => pl.Genres)
            .Select(g => new PlexGenreDTO { Id = g.Id, Name = g.Name })
            .Distinct()
            .ToListAsync(ct);

        var uniqueQualities = await GetQualitiesForMediaType(req.MediaType, ct: ct);
        var allNavigationIndexes = await GetNavigationIndexes(req, ct);

        await SendFluentResult(
            Result.Ok(
                new PlexMediaMetadataDTO
                {
                    MediaCount = mediaCount,
                    QualityCount = uniqueQualities.Count,
                    Roles = uniqueRoles,
                    Countries = uniqueCountries,
                    Genres = uniqueGenres,
                    RoleCount = roleCount,
                    CountryCount = countryCount,
                    GenreCount = genreCount,
                    Qualities = uniqueQualities,
                    NavigationIndexes = allNavigationIndexes,
                }
            ),
            ct
        );
    }

    private async Task<Dictionary<string, int>> GetNavigationIndexes(GetLibraryMediaMetadataRequest req, CancellationToken ct)
    {
        var filter = new MediaQueryFilter
        {
            MediaType = req.MediaType,
            PlexLibraryId = req.PlexLibraryId,
            Skip = 0,
            Take = 0,
            FilterOfflineMedia = req.FilterOfflineMedia,
            FilterOwnedMedia = req.FilterOwnedMedia,
            CountryId = req.CountryId,
            ActorId = req.ActorId,
            GenreId = req.GenreId,
            Quality = req.Quality,
            Search = req.Search,
            SortField = req.SortField,
            SortDirection = req.SortDirection,
        };

        var result = await _dbContext.GetMediaByType(filter, ct);
        if (result.IsFailed || result.Value.TotalCount == 0)
        {
            return [];
        }

        var items = result.Value.Items;
        var indexes = new Dictionary<string, int>();

        for (var i = 0; i < items.Count; i++)
        {
            var label = GetNavigationLabel(items[i], req.SortField);
            if (string.IsNullOrWhiteSpace(label) || indexes.ContainsKey(label))
            {
                continue;
            }

            indexes[label] = i;
        }

        return indexes;
    }

    private static string GetNavigationLabel(PlexMediaSlimDTO item, string sortField)
    {
        return sortField switch
        {
            "year" => item.Year > 0 ? item.Year.ToString() : "#",
            "quality" => ToQualityLabel(item.Qualities.MaxBy(x => (int)x.Quality)?.Quality ?? VideoQuality.Unknown),
            "duration" => ToDurationBucket(item.Duration),
            "addedAt" => item.AddedAt.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
            "updatedAt" => item.UpdatedAt?.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "#",
            "mediaSize" => ToMediaSizeBucket(item.MediaSize),
            _ => ToTitleBucket(item.Title),
        };
    }

    private static string ToTitleBucket(string title)
    {
        var first = title.Trim().FirstOrDefault();
        return char.IsLetter(first) ? char.ToUpperInvariant(first).ToString() : "#";
    }

    private static string ToDurationBucket(int seconds)
    {
        var start = Math.Max(0, seconds) / 600 * 10;
        return $"{start}–{start + 10} min";
    }

    private static string ToMediaSizeBucket(long bytes)
    {
        var start = (int)Math.Floor((double)Math.Max(0, bytes) / Gigabyte);
        return $"{start}–{start + 1} GB";
    }

    private static string ToQualityLabel(VideoQuality quality)
    {
        return quality switch
        {
            VideoQuality.SubSD_144p => "144p",
            VideoQuality.SubSD_CIF => "240p",
            VideoQuality.nHD => "360p",
            VideoQuality.SD => "480p",
            VideoQuality.DVD => "576p",
            VideoQuality.HD => "720p",
            VideoQuality.FullHD => "1080p",
            VideoQuality.QHD => "1440p",
            VideoQuality.UHD_4K => "4K",
            VideoQuality.UHD_8K => "8K",
            VideoQuality.Unknown => "Unknown",
            _ => "None",
        };
    }

    private async Task<List<PlexQualityDTO>> GetQualitiesForMediaType(
        PlexMediaType mediaType,
        int plexLibraryId = 0,
        CancellationToken ct = default
    )
    {
        var uniqueQualities = new List<PlexQualityDTO>();
        if (mediaType == PlexMediaType.Movie)
        {
            uniqueQualities = await _dbContext
                .PlexMovieData.ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                .GroupBy(x => x.Quality)
                .Select(g => new PlexQualityDTO
                {
                    Name = ((int)g.Key).ToString(),
                    Quality = g.Key,
                    Count = g.Count(),
                })
                .ToListAsync(ct);
        }

        if (mediaType == PlexMediaType.TvShow)
        {
            uniqueQualities = await _dbContext
                .PlexTvShowMediaQualities.ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                .GroupBy(x => x.Quality)
                .Select(g => new PlexQualityDTO
                {
                    Name = ((int)g.Key).ToString(),
                    Quality = g.Key,
                    Count = g.Count(),
                })
                .ToListAsync(ct);
        }

        return uniqueQualities;
    }
}
