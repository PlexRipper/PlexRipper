namespace Reaparr.Application;

public class GetMediaComparisonDetailsEndpointRequest
{
    [SetsRequiredMembers]
    public GetMediaComparisonDetailsEndpointRequest(int plexMediaId, PlexMediaType type)
    {
        PlexMediaId = plexMediaId;
        Type = type;
    }

    public required int PlexMediaId { get; init; }

    [QueryParam, BindFrom("type")]
    public required PlexMediaType Type { get; init; }
}

public class GetMediaComparisonDetailsEndpointRequestValidator : Validator<GetMediaComparisonDetailsEndpointRequest>
{
    public GetMediaComparisonDetailsEndpointRequestValidator()
    {
        RuleFor(x => x.PlexMediaId).GreaterThan(0);
        RuleFor(x => x.Type).Must(x => x is PlexMediaType.Movie or PlexMediaType.TvShow);
    }
}

public record PlexMediaComparisonDetailsDTO
{
    public required int PlexMediaId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required PlexMediaComparisonState State { get; init; }

    public required List<PlexMediaComparisonDetailsRowDTO> Rows { get; init; }
}

public record PlexMediaComparisonDetailsRowDTO
{
    public required int Id { get; init; }

    public int? ParentId { get; init; }

    public required int Level { get; init; }

    public required int PlexMediaId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required string Title { get; init; }

    public required int ComparisonId { get; init; }

    public required bool IsActionable { get; init; }

    public VideoQuality? RemoteQuality { get; init; }

    public VideoQuality? OwnedQuality { get; init; }

    public string RemoteLocation { get; init; } = string.Empty;

    public string OwnedLocation { get; init; } = string.Empty;

    public string RemoteLibraryTitle { get; init; } = string.Empty;

    public string OwnedLibraryTitle { get; init; } = string.Empty;
}

public class GetMediaComparisonDetailsEndpoint
    : Endpoint<GetMediaComparisonDetailsEndpointRequest, ResultDTO<PlexMediaComparisonDetailsDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private int _rowId;

    public GetMediaComparisonDetailsEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetMediaComparisonDetailsEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexMediaController + "/comparison-details/{PlexMediaId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaComparisonDetailsDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetMediaComparisonDetailsEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var result = req.Type switch
        {
            PlexMediaType.Movie => await GetMovieDetailsAsync(req.PlexMediaId, ct),
            PlexMediaType.TvShow => await GetTvShowDetailsAsync(req.PlexMediaId, ct),
            _ => Result.Fail<PlexMediaComparisonDetailsDTO>("Unsupported media type"),
        };

        await Send.FluentResult(result, ct);
    }

    private async Task<Result<PlexMediaComparisonDetailsDTO>> GetMovieDetailsAsync(int movieId, CancellationToken ct)
    {
        var movie = await _dbContext.PlexMovies.Include(x => x.MediaDataList).SingleOrDefaultAsync(x => x.Id == movieId, ct);
        if (movie is null)
            return ResultExtensions.EntityNotFound(nameof(PlexMovie), movieId).LogError();

        var isOwned = await _dbContext.PlexLibraries.WhereIsOwned().AnyAsync(x => x.Id == movie.PlexLibraryId, ct);
        var rows = isOwned
            ? await GetOwnedMovieRowsAsync(movie, ct)
            : await GetRemoteMovieRowsAsync(movie, ct);

        return Result.Ok(new PlexMediaComparisonDetailsDTO
        {
            PlexMediaId = movie.Id,
            Type = PlexMediaType.Movie,
            State = ToParentState(rows),
            Rows = rows,
        });
    }

    private async Task<List<PlexMediaComparisonDetailsRowDTO>> GetRemoteMovieRowsAsync(PlexMovie movie, CancellationToken ct)
    {
        var currentOwnedLibraryIds = await GetCurrentOwnedLibraryIdsAsync(movie.PlexLibraryId, PlexMediaType.Movie, ct);
        if (currentOwnedLibraryIds.Count == 0)
            return [];

        var hits = await _dbContext.PlexMovieComparisons
            .Where(x =>
                x.RemotePlexLibraryId == movie.PlexLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && x.RemotePlexMediaId == movie.Id)
            .ToListAsync(ct);

        if (hits.Count == 0)
            return
            [
                CreateRow(
                    parentId: null,
                    level: 0,
                    plexMediaId: movie.Id,
                    type: PlexMediaType.Movie,
                    title: movie.Title,
                    state: PlexMediaComparisonState.Missing,
                    remoteQuality: movie.Quality,
                    ownedQuality: null,
                    remoteLocation: GetLocation(movie.MediaDataList),
                    ownedLocation: string.Empty,
                    remoteLibraryTitle: await GetLibraryTitleAsync(movie.PlexLibraryId, ct),
                    ownedLibraryTitle: string.Empty)
            ];

        return await CreateRemoteMovieUpgradeRowsAsync(movie, hits, ct);
    }

    private async Task<List<PlexMediaComparisonDetailsRowDTO>> CreateRemoteMovieUpgradeRowsAsync(
        PlexMovie movie,
        List<PlexMovieComparison> hits,
        CancellationToken ct)
    {
        var upgradeHits = hits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality).ToList();
        if (upgradeHits.Count == 0)
            return [];

        var ownedMovieIds = upgradeHits.Select(x => x.OwnedPlexMediaId).ToHashSet();
        var ownedMovies = await _dbContext.PlexMovies
            .Include(x => x.MediaDataList)
            .Where(x => ownedMovieIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var libraryTitles = await GetLibraryTitlesAsync(upgradeHits.SelectMany(x => new[] { x.RemotePlexLibraryId, x.OwnedPlexLibraryId }), ct);

        return upgradeHits
            .Select(hit =>
            {
                ownedMovies.TryGetValue(hit.OwnedPlexMediaId, out var ownedMovie);
                return CreateRow(
                    parentId: null,
                    level: 0,
                    plexMediaId: movie.Id,
                    type: PlexMediaType.Movie,
                    title: movie.Title,
                    state: PlexMediaComparisonState.HigherQuality,
                    remoteQuality: hit.RemoteQuality,
                    ownedQuality: hit.OwnedQuality,
                    remoteLocation: GetLocation(movie.MediaDataList),
                    ownedLocation: ownedMovie is null ? string.Empty : GetLocation(ownedMovie.MediaDataList),
                    remoteLibraryTitle: GetDictionaryValue(libraryTitles, hit.RemotePlexLibraryId),
                    ownedLibraryTitle: GetDictionaryValue(libraryTitles, hit.OwnedPlexLibraryId));
            })
            .ToList();
    }

    private async Task<List<PlexMediaComparisonDetailsRowDTO>> GetOwnedMovieRowsAsync(PlexMovie movie, CancellationToken ct)
    {
        var currentRemoteLibraryIds = await GetCurrentRemoteLibraryIdsAsync(movie.PlexLibraryId, PlexMediaType.Movie, ct);
        if (currentRemoteLibraryIds.Count == 0)
            return [];

        var upgradeHits = await _dbContext.PlexMovieComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == movie.PlexLibraryId
                && x.OwnedPlexMediaId == movie.Id
                && x.HitState == PlexMediaComparisonHitState.HigherQuality)
            .ToListAsync(ct);

        if (upgradeHits.Count == 0)
            return [];

        var remoteMovieIds = upgradeHits.Select(x => x.RemotePlexMediaId).ToHashSet();
        var remoteMovies = await _dbContext.PlexMovies
            .Include(x => x.MediaDataList)
            .Where(x => remoteMovieIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var libraryTitles = await GetLibraryTitlesAsync(upgradeHits.SelectMany(x => new[] { x.RemotePlexLibraryId, x.OwnedPlexLibraryId }), ct);

        return upgradeHits
            .Select(hit =>
            {
                remoteMovies.TryGetValue(hit.RemotePlexMediaId, out var remoteMovie);
                return CreateRow(
                    parentId: null,
                    level: 0,
                    plexMediaId: remoteMovie?.Id ?? hit.RemotePlexMediaId,
                    type: PlexMediaType.Movie,
                    title: remoteMovie?.Title ?? movie.Title,
                    state: PlexMediaComparisonState.HigherQuality,
                    remoteQuality: hit.RemoteQuality,
                    ownedQuality: hit.OwnedQuality,
                    remoteLocation: remoteMovie is null ? string.Empty : GetLocation(remoteMovie.MediaDataList),
                    ownedLocation: GetLocation(movie.MediaDataList),
                    remoteLibraryTitle: GetDictionaryValue(libraryTitles, hit.RemotePlexLibraryId),
                    ownedLibraryTitle: GetDictionaryValue(libraryTitles, hit.OwnedPlexLibraryId));
            })
            .ToList();
    }

    private async Task<Result<PlexMediaComparisonDetailsDTO>> GetTvShowDetailsAsync(int tvShowId, CancellationToken ct)
    {
        var tvShow = await _dbContext.PlexTvShows.SingleOrDefaultAsync(x => x.Id == tvShowId, ct);
        if (tvShow is null)
            return ResultExtensions.EntityNotFound(nameof(PlexTvShow), tvShowId).LogError();

        var isOwned = await _dbContext.PlexLibraries.WhereIsOwned().AnyAsync(x => x.Id == tvShow.PlexLibraryId, ct);
        var rows = isOwned
            ? await GetOwnedTvShowRowsAsync(tvShow, ct)
            : await GetRemoteTvShowRowsAsync(tvShow, ct);

        return Result.Ok(new PlexMediaComparisonDetailsDTO
        {
            PlexMediaId = tvShow.Id,
            Type = PlexMediaType.TvShow,
            State = ToParentState(rows),
            Rows = rows,
        });
    }

    private async Task<List<PlexMediaComparisonDetailsRowDTO>> GetRemoteTvShowRowsAsync(PlexTvShow tvShow, CancellationToken ct)
    {
        var currentOwnedLibraryIds = await GetCurrentOwnedLibraryIdsAsync(tvShow.PlexLibraryId, PlexMediaType.TvShow, ct);
        if (currentOwnedLibraryIds.Count == 0)
            return [];

        var episodes = await _dbContext.PlexTvShowEpisodes
            .Include(x => x.MediaDataList)
            .Include(x => x.TvShowSeason)
            .Where(x => x.PlexLibraryId == tvShow.PlexLibraryId && x.TvShowId == tvShow.Id)
            .OrderBy(x => x.TvShowSeason == null ? 0 : x.TvShowSeason.SeasonNumber)
            .ThenBy(x => x.EpisodeNumber)
            .ToListAsync(ct);

        var episodeIds = episodes.Select(x => x.Id).ToHashSet();
        var hits = await _dbContext.PlexEpisodeComparisons
            .Where(x =>
                x.RemotePlexLibraryId == tvShow.PlexLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && episodeIds.Contains(x.RemotePlexMediaId))
            .ToListAsync(ct);

        var ownedEpisodeIds = hits.Select(x => x.OwnedPlexMediaId).ToHashSet();
        var ownedEpisodes = await _dbContext.PlexTvShowEpisodes
            .Include(x => x.MediaDataList)
            .Where(x => ownedEpisodeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var libraryTitles = await GetLibraryTitlesAsync(hits.SelectMany(x => new[] { x.RemotePlexLibraryId, x.OwnedPlexLibraryId }).Append(tvShow.PlexLibraryId), ct);

        return BuildRemoteTvRows(episodes, hits, ownedEpisodes, libraryTitles);
    }

    private async Task<List<PlexMediaComparisonDetailsRowDTO>> GetOwnedTvShowRowsAsync(PlexTvShow tvShow, CancellationToken ct)
    {
        var currentRemoteLibraryIds = await GetCurrentRemoteLibraryIdsAsync(tvShow.PlexLibraryId, PlexMediaType.TvShow, ct);
        if (currentRemoteLibraryIds.Count == 0)
            return [];

        var showHits = await _dbContext.PlexTvShowComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == tvShow.PlexLibraryId
                && x.OwnedPlexMediaId == tvShow.Id)
            .ToListAsync(ct);

        var remoteShowIds = showHits.Select(x => x.RemotePlexMediaId).ToHashSet();
        if (remoteShowIds.Count == 0)
            return [];

        var remoteEpisodes = await _dbContext.PlexTvShowEpisodes
            .Include(x => x.MediaDataList)
            .Include(x => x.TvShowSeason)
            .Where(x => remoteShowIds.Contains(x.TvShowId))
            .OrderBy(x => x.TvShowSeason == null ? 0 : x.TvShowSeason.SeasonNumber)
            .ThenBy(x => x.EpisodeNumber)
            .ToListAsync(ct);

        var remoteEpisodeIds = remoteEpisodes.Select(x => x.Id).ToHashSet();
        var hits = await _dbContext.PlexEpisodeComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == tvShow.PlexLibraryId
                && remoteEpisodeIds.Contains(x.RemotePlexMediaId))
            .ToListAsync(ct);

        var ownedEpisodeIds = hits.Select(x => x.OwnedPlexMediaId).ToHashSet();
        var ownedEpisodes = await _dbContext.PlexTvShowEpisodes
            .Include(x => x.MediaDataList)
            .Where(x => ownedEpisodeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var libraryTitles = await GetLibraryTitlesAsync(hits.SelectMany(x => new[] { x.RemotePlexLibraryId, x.OwnedPlexLibraryId }).Concat(showHits.SelectMany(x => new[] { x.RemotePlexLibraryId, x.OwnedPlexLibraryId })), ct);

        return BuildRemoteTvRows(remoteEpisodes, hits, ownedEpisodes, libraryTitles);
    }

    private List<PlexMediaComparisonDetailsRowDTO> BuildRemoteTvRows(
        List<PlexTvShowEpisode> episodes,
        List<PlexEpisodeComparison> hits,
        Dictionary<int, PlexTvShowEpisode> ownedEpisodes,
        Dictionary<int, string> libraryTitles)
    {
        var hitLookup = hits.GroupBy(x => x.RemotePlexMediaId).ToDictionary(x => x.Key, x => x.ToList());
        var childRows = new List<PlexMediaComparisonDetailsRowDTO>();

        foreach (var episode in episodes)
        {
            if (!hitLookup.TryGetValue(episode.Id, out var episodeHits))
            {
                childRows.Add(CreateRow(
                    parentId: null,
                    level: 1,
                    plexMediaId: episode.Id,
                    type: PlexMediaType.Episode,
                    title: episode.Title,
                    state: PlexMediaComparisonState.Missing,
                    remoteQuality: GetQuality(episode),
                    ownedQuality: null,
                    remoteLocation: GetLocation(episode.MediaDataList),
                    ownedLocation: string.Empty,
                    remoteLibraryTitle: GetDictionaryValue(libraryTitles, episode.PlexLibraryId),
                    ownedLibraryTitle: string.Empty));
                continue;
            }

            foreach (var hit in episodeHits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality))
            {
                ownedEpisodes.TryGetValue(hit.OwnedPlexMediaId, out var ownedEpisode);
                childRows.Add(CreateRow(
                    parentId: null,
                    level: 1,
                    plexMediaId: episode.Id,
                    type: PlexMediaType.Episode,
                    title: episode.Title,
                    state: PlexMediaComparisonState.HigherQuality,
                    remoteQuality: GetQuality(hit.RemoteQuality, episode),
                    ownedQuality: ownedEpisode is null ? hit.OwnedQuality : GetQuality(hit.OwnedQuality, ownedEpisode),
                    remoteLocation: GetLocation(episode.MediaDataList),
                    ownedLocation: ownedEpisode is null ? string.Empty : GetLocation(ownedEpisode.MediaDataList),
                    remoteLibraryTitle: GetDictionaryValue(libraryTitles, hit.RemotePlexLibraryId),
                    ownedLibraryTitle: GetDictionaryValue(libraryTitles, hit.OwnedPlexLibraryId)));
            }
        }

        return AddSeasonParents(episodes, childRows);
    }

    private List<PlexMediaComparisonDetailsRowDTO> AddSeasonParents(
        List<PlexTvShowEpisode> episodes,
        List<PlexMediaComparisonDetailsRowDTO> childRows)
    {
        if (childRows.Count == 0)
            return [];

        var episodeLookup = episodes.ToDictionary(x => x.Id);
        var result = new List<PlexMediaComparisonDetailsRowDTO>();
        foreach (var group in childRows.GroupBy(x => episodeLookup.TryGetValue(x.PlexMediaId, out var episode) ? episode.TvShowSeason?.SeasonNumber ?? 0 : 0).OrderBy(x => x.Key))
        {
            var parentId = ++_rowId;
            var hasMissing = group.Any(x => x.ComparisonId == PlexMediaComparisonState.Missing.ToComparisonId());
            var hasHigherQuality = group.Any(x => x.ComparisonId == PlexMediaComparisonState.HigherQuality.ToComparisonId());
            result.Add(new PlexMediaComparisonDetailsRowDTO
            {
                Id = parentId,
                ParentId = null,
                Level = 0,
                PlexMediaId = 0,
                Type = PlexMediaType.Season,
                Title = $"Season {group.Key}",
                ComparisonId = hasMissing && hasHigherQuality
                    ? PlexMediaComparisonState.PartialAndHigherQuality.ToComparisonId()
                    : hasMissing ? PlexMediaComparisonState.Partial.ToComparisonId() : PlexMediaComparisonState.HigherQuality.ToComparisonId(),
                IsActionable = true,
            });

            result.AddRange(group.Select(x => x with { ParentId = parentId }));
        }

        return result;
    }

    private async Task<HashSet<int>> GetCurrentOwnedLibraryIdsAsync(int remoteLibraryId, PlexMediaType mediaType, CancellationToken ct)
    {
        var remoteUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == remoteLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);
        if (remoteUpdatedAt is null)
            return [];

        var ownedLibraries = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .Where(x => x.Type == mediaType)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.MediaType == mediaType && ownedLibraries.Keys.Contains(x.OwnedPlexLibraryId))
            .ToListAsync(ct);

        return scopeRows
            .Where(x =>
                x.RemoteLibraryUpdatedAt == remoteUpdatedAt
                && ownedLibraries.TryGetValue(x.OwnedPlexLibraryId, out var ownedUpdatedAt)
                && x.OwnedLibraryUpdatedAt == ownedUpdatedAt)
            .Select(x => x.OwnedPlexLibraryId)
            .ToHashSet();
    }

    private async Task<HashSet<int>> GetCurrentRemoteLibraryIdsAsync(int ownedLibraryId, PlexMediaType mediaType, CancellationToken ct)
    {
        var ownedUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == ownedLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);
        if (ownedUpdatedAt is null)
            return [];

        var remoteLibraries = await _dbContext.PlexLibraries
            .WhereIsNotOwned()
            .Where(x => x.Type == mediaType)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x => x.OwnedPlexLibraryId == ownedLibraryId && x.MediaType == mediaType && remoteLibraries.Keys.Contains(x.RemotePlexLibraryId))
            .ToListAsync(ct);

        return scopeRows
            .Where(x =>
                x.OwnedLibraryUpdatedAt == ownedUpdatedAt
                && remoteLibraries.TryGetValue(x.RemotePlexLibraryId, out var remoteUpdatedAt)
                && x.RemoteLibraryUpdatedAt == remoteUpdatedAt)
            .Select(x => x.RemotePlexLibraryId)
            .ToHashSet();
    }

    private PlexMediaComparisonDetailsRowDTO CreateRow(
        int? parentId,
        int level,
        int plexMediaId,
        PlexMediaType type,
        string title,
        PlexMediaComparisonState state,
        VideoQuality? remoteQuality,
        VideoQuality? ownedQuality,
        string remoteLocation,
        string ownedLocation,
        string remoteLibraryTitle,
        string ownedLibraryTitle) =>
        new()
        {
            Id = ++_rowId,
            ParentId = parentId,
            Level = level,
            PlexMediaId = plexMediaId,
            Type = type,
            Title = title,
            ComparisonId = state.ToComparisonId(),
            IsActionable = true,
            RemoteQuality = remoteQuality,
            OwnedQuality = ownedQuality,
            RemoteLocation = remoteLocation,
            OwnedLocation = ownedLocation,
            RemoteLibraryTitle = remoteLibraryTitle,
            OwnedLibraryTitle = ownedLibraryTitle,
        };

    private static PlexMediaComparisonState ToParentState(List<PlexMediaComparisonDetailsRowDTO> rows)
    {
        var hasMissing = rows.Any(x => x.ComparisonId == PlexMediaComparisonState.Missing.ToComparisonId() || x.ComparisonId == PlexMediaComparisonState.Partial.ToComparisonId());
        var hasHigherQuality = rows.Any(x => x.ComparisonId == PlexMediaComparisonState.HigherQuality.ToComparisonId());
        return (hasMissing, hasHigherQuality) switch
        {
            (true, true) => PlexMediaComparisonState.PartialAndHigherQuality,
            (true, false) => PlexMediaComparisonState.Partial,
            (false, true) => PlexMediaComparisonState.HigherQuality,
            _ => PlexMediaComparisonState.Owned,
        };
    }

    private async Task<string> GetLibraryTitleAsync(int libraryId, CancellationToken ct) =>
        await _dbContext.PlexLibraries.Where(x => x.Id == libraryId).Select(x => x.Title).SingleOrDefaultAsync(ct) ?? string.Empty;

    private async Task<Dictionary<int, string>> GetLibraryTitlesAsync(IEnumerable<int> libraryIds, CancellationToken ct)
    {
        var ids = libraryIds.ToHashSet();
        return await _dbContext.PlexLibraries.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title, ct);
    }

    private static string GetDictionaryValue(Dictionary<int, string> source, int key) =>
        source.GetValueOrDefault(key, string.Empty);

    private static string GetLocation(IEnumerable<PlexMovieMediaData> mediaData) =>
        mediaData.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;

    private static string GetLocation(IEnumerable<PlexTvShowEpisodeMediaData> mediaData) =>
        mediaData.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;

    private static VideoQuality GetQuality(PlexTvShowEpisode episode) =>
        GetQuality(episode.Quality, episode);

    private static VideoQuality GetQuality(VideoQuality quality, PlexTvShowEpisode episode)
    {
        if (quality != VideoQuality.Unknown)
            return quality;

        return episode.MediaDataList
            .Select(x => x.VideoResolution)
            .OrderByDescending(x => x.ToId())
            .FirstOrDefault();
    }
}
