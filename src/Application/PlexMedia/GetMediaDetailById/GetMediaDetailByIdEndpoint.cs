namespace Reaparr.Application;

/// <summary>
/// Gets the <see cref="PlexMediaDTO"/> with all children
/// </summary>
public class GetMediaDetailByIdEndpointRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    [SetsRequiredMembers]
    public GetMediaDetailByIdEndpointRequest(int plexMediaId, PlexMediaType type)
    {
        PlexMediaId = plexMediaId;
        Type = type;
    }

    /// <summary>The id of the <see cref="BasePlexMedia"/>.</summary>
    public required int PlexMediaId { get; init; }

    /// <summary> The <see cref="PlexMediaType">Type</see> of the PlexMedia.</summary>
    [QueryParam, BindFrom("type")]
    public required PlexMediaType Type { get; init; }
}

public class GetMediaDetailByIdEndpointRequestValidator : Validator<GetMediaDetailByIdEndpointRequest>
{
    public GetMediaDetailByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexMediaId).GreaterThan(0);
        RuleFor(x => x.Type).Must(x => x is PlexMediaType.Movie or PlexMediaType.TvShow);
    }
}

public class GetMediaDetailByIdEndpoint : Endpoint<GetMediaDetailByIdEndpointRequest, ResultDTO<PlexMediaDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public GetMediaDetailByIdEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<GetMediaDetailByIdEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexMediaController + "/detail/{PlexMediaId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetMediaDetailByIdEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        if (req.Type == PlexMediaType.Movie)
        {
            var plexMovie = await _dbContext.PlexMovies.IncludeAll().FirstOrDefaultAsync(x => x.Id == req.PlexMediaId, ct);
            if (plexMovie is null)
            {
                await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(req.Type.GetType), req.PlexMediaId), ct);
                return;
            }

            await SetNestedMovieProperties(plexMovie, ct);
            await ApplyMovieDetailComparisonStateAsync(plexMovie, ct);

            await Send.FluentResult(Result.Ok(plexMovie), x => x.ToDTO(), ct);
        }
        else if (req.Type == PlexMediaType.TvShow)
        {
            var plexTvShowResult = await GetPlexTvShow(req.PlexMediaId, ct);
            if (plexTvShowResult.IsFailed)
            {
                await Send.FluentResult(plexTvShowResult, ct);
                return;
            }

            await ApplyTvShowDetailComparisonStateAsync(plexTvShowResult.Value, ct);

            await Send.FluentResult(plexTvShowResult, x => x.ToDTO(), ct);
        }
        else
            await Send.FluentResult(ResultExtensions.Create400BadRequestResult($"Type {req.Type} is not allowed"), ct);
    }

    private async Task<Result<PlexTvShow>> GetPlexTvShow(int plexTvShowId, CancellationToken ct)
    {
        var plexTvShow = _dbContext.PlexTvShows.FirstOrDefault(x => x.Id == plexTvShowId);

        if (plexTvShow is null)
            return ResultExtensions.EntityNotFound(nameof(PlexTvShow), plexTvShowId).LogError();

        plexTvShow.Seasons = _dbContext
            .PlexTvShowSeason.Where(x => x.TvShowId == plexTvShowId)
            .Take(plexTvShow.ChildCount)
            .ToList();

        plexTvShow.Seasons = plexTvShow.Seasons.OrderBy(x => x.SortIndex).ToList();

        foreach (var season in plexTvShow.Seasons)
            season.Episodes = _dbContext
                .PlexTvShowEpisodes.Include(x => x.MediaDataList)
                .Where(x => x.TvShowSeasonId == season.Id)
                .Take(season.ChildCount)
                .ToList();

        await SetNestedTvShowProperties(plexTvShow, ct);

        return Result.Ok(plexTvShow);
    }

    private async Task ApplyTvShowDetailComparisonStateAsync(PlexTvShow plexTvShow, CancellationToken ct)
    {
        var isOwned = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .AnyAsync(x => x.Id == plexTvShow.PlexLibraryId, ct);

        if (isOwned)
            await ApplyOwnedTvShowDetailComparisonStateAsync(plexTvShow, ct);
        else
            await ApplyRemoteTvShowDetailComparisonStateAsync(plexTvShow, ct);
    }

    private async Task ApplyRemoteTvShowDetailComparisonStateAsync(PlexTvShow plexTvShow, CancellationToken ct)
    {
        var remoteUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == plexTvShow.PlexLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);

        if (remoteUpdatedAt is null)
            return;

        var ownedLibraries = await _dbContext.PlexLibraries
            .WhereIsOwned()
            .Where(x => x.Type == PlexMediaType.TvShow)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (ownedLibraries.Count == 0)
            return;

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.RemotePlexLibraryId == plexTvShow.PlexLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && ownedLibraries.Keys.Contains(x.OwnedPlexLibraryId))
            .ToListAsync(ct);

        var currentOwnedLibraryIds = scopeRows
            .Where(x =>
                x.RemoteLibraryUpdatedAt == remoteUpdatedAt
                && ownedLibraries.TryGetValue(x.OwnedPlexLibraryId, out var ownedUpdatedAt)
                && x.OwnedLibraryUpdatedAt == ownedUpdatedAt)
            .Select(x => x.OwnedPlexLibraryId)
            .ToHashSet();

        if (currentOwnedLibraryIds.Count == 0)
        {
            if (await HasPendingRemoteTvShowComparisonAsync(plexTvShow.PlexLibraryId, ownedLibraries.Keys.ToHashSet(), ct))
                SetEpisodeComparisonState(plexTvShow, PlexMediaComparisonState.Pending);

            return;
        }

        var episodeIds = plexTvShow.Seasons
            .SelectMany(x => x.Episodes)
            .Select(x => x.Id)
            .ToHashSet();

        var episodeHits = await _dbContext.PlexEpisodeComparisons
            .Where(x =>
                x.RemotePlexLibraryId == plexTvShow.PlexLibraryId
                && currentOwnedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && episodeIds.Contains(x.RemotePlexMediaId))
            .Select(x => new { x.RemotePlexMediaId, x.HitState })
            .ToListAsync(ct);

        var episodeHitLookup = episodeHits
            .GroupBy(x => x.RemotePlexMediaId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.HitState).ToList());

        foreach (var episode in plexTvShow.Seasons.SelectMany(x => x.Episodes))
        {
            if (!episodeHitLookup.TryGetValue(episode.Id, out var hits))
            {
                episode.ComparisonState = PlexMediaComparisonState.Missing;
                continue;
            }

            episode.ComparisonState = hits.Contains(PlexMediaComparisonHitState.HigherQuality)
                ? PlexMediaComparisonState.HigherQuality
                : PlexMediaComparisonState.Owned;
        }
    }

    private async Task ApplyOwnedTvShowDetailComparisonStateAsync(PlexTvShow plexTvShow, CancellationToken ct)
    {
        var ownedUpdatedAt = await _dbContext.PlexLibraries
            .Where(x => x.Id == plexTvShow.PlexLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(ct);

        if (ownedUpdatedAt is null)
            return;

        var remoteLibraries = await _dbContext.PlexLibraries
            .WhereIsNotOwned()
            .Where(x => x.Type == PlexMediaType.TvShow)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, ct);

        if (remoteLibraries.Count == 0)
            return;

        var scopeRows = await _dbContext.PlexComparisonScopes
            .Where(x =>
                x.OwnedPlexLibraryId == plexTvShow.PlexLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && remoteLibraries.Keys.Contains(x.RemotePlexLibraryId))
            .ToListAsync(ct);

        var currentRemoteLibraryIds = scopeRows
            .Where(x =>
                x.OwnedLibraryUpdatedAt == ownedUpdatedAt
                && remoteLibraries.TryGetValue(x.RemotePlexLibraryId, out var remoteUpdatedAt)
                && x.RemoteLibraryUpdatedAt == remoteUpdatedAt)
            .Select(x => x.RemotePlexLibraryId)
            .ToHashSet();

        if (currentRemoteLibraryIds.Count == 0)
        {
            if (await HasPendingOwnedTvShowComparisonAsync(plexTvShow.PlexLibraryId, remoteLibraries.Keys.ToHashSet(), ct))
                SetEpisodeComparisonState(plexTvShow, PlexMediaComparisonState.Pending);

            return;
        }

        var episodeIds = plexTvShow.Seasons
            .SelectMany(x => x.Episodes)
            .Select(x => x.Id)
            .ToHashSet();

        var upgradeIds = await _dbContext.PlexEpisodeComparisons
            .Where(x =>
                currentRemoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && x.OwnedPlexLibraryId == plexTvShow.PlexLibraryId
                && episodeIds.Contains(x.OwnedPlexMediaId)
                && x.HitState == PlexMediaComparisonHitState.HigherQuality)
            .Select(x => x.OwnedPlexMediaId)
            .Distinct()
            .ToListAsync(ct);

        var upgradeIdSet = upgradeIds.ToHashSet();

        foreach (var episode in plexTvShow.Seasons.SelectMany(x => x.Episodes))
        {
            episode.ComparisonState = upgradeIdSet.Contains(episode.Id)
                ? PlexMediaComparisonState.HigherQuality
                : PlexMediaComparisonState.Owned;
        }
    }

    private async Task<bool> HasPendingRemoteTvShowComparisonAsync(
        int remoteLibraryId,
        HashSet<int> ownedLibraryIds,
        CancellationToken ct) =>
        await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x =>
                x.RemotePlexLibraryId == remoteLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && ownedLibraryIds.Contains(x.OwnedPlexLibraryId)
                && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing), ct);

    private async Task<bool> HasPendingOwnedTvShowComparisonAsync(
        int ownedLibraryId,
        HashSet<int> remoteLibraryIds,
        CancellationToken ct) =>
        await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x =>
                x.OwnedPlexLibraryId == ownedLibraryId
                && x.MediaType == PlexMediaType.TvShow
                && remoteLibraryIds.Contains(x.RemotePlexLibraryId)
                && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing), ct);

    private static void SetEpisodeComparisonState(PlexTvShow plexTvShow, PlexMediaComparisonState state)
    {
        foreach (var episode in plexTvShow.Seasons.SelectMany(x => x.Episodes))
            episode.ComparisonState = state;
    }

    private async Task ApplyMovieDetailComparisonStateAsync(PlexMovie plexMovie, CancellationToken ct)
    {
        var items = new List<PlexMediaSlimDTO> { plexMovie.ToSlimDTO() };
        await _commandExecutor.Send(new ApplyComparisonStateCommand(items, plexMovie.PlexLibraryId, PlexMediaType.Movie), ct);

        plexMovie.ComparisonState = items[0].ComparisonId.ToComparisonState();
    }

    private async Task SetNestedMovieProperties(PlexMovie plexMovie, CancellationToken ct = default)
    {
        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexMovie.PlexServerId, ct);
        if (plexServerConnection.IsFailed)
        {
            plexServerConnection.ToResult().LogError();
            return;
        }

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexMovie.PlexServerId, ct);
        if (plexServerToken.IsFailed)
        {
            plexServerToken.ToResult().LogError();
        }
    }

    private async Task SetNestedTvShowProperties(PlexTvShow plexTvShow, CancellationToken ct = default)
    {
        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexTvShow.PlexServerId, ct);
        if (plexServerConnection.IsFailed)
        {
            plexServerConnection.ToResult().LogError();
            return;
        }

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexTvShow.PlexServerId, ct);
        if (plexServerToken.IsFailed)
        {
            plexServerToken.ToResult().LogError();
        }
    }
}
