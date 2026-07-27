namespace Reaparr.Application;

public class CompareTvShowPlexLibraryCommandValidator : AbstractValidator<CompareTvShowPlexLibraryCommand>
{
    public CompareTvShowPlexLibraryCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0).NotEqual(x => x.OwnedPlexLibraryId);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
    }
}

public class CompareTvShowPlexLibraryCommandHandler : ICommandHandler<CompareTvShowPlexLibraryCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public CompareTvShowPlexLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext
    )
    {
        _log = log.ForContext<CompareTvShowPlexLibraryCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        CompareTvShowPlexLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        return await Result.Try(new Func<Task<Result>>(async () =>
        {
            var (remoteLibraryId, ownedLibraryId) = command;

            _log.Here()
                .Information(
                    "Starting TV show comparison: remote library {RemoteLibId} vs owned library {OwnedLibId}",
                    remoteLibraryId,
                    ownedLibraryId
                );

            var validationResult = await ValidateLibrariesAsync(remoteLibraryId, ownedLibraryId, cancellationToken);

            if (validationResult.IsFailed)
                return validationResult;

            return await CompareTvShowsAsync(remoteLibraryId, ownedLibraryId, cancellationToken);
        }));
    }

    private async Task<Result> ValidateLibrariesAsync(
        int remoteLibraryId,
        int ownedLibraryId,
        CancellationToken cancellationToken
    )
    {
        var libraries = await _dbContext.PlexLibraries
            .Where(x => x.Id == remoteLibraryId || x.Id == ownedLibraryId)
            .SelectOwnership()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (!libraries.TryGetValue(remoteLibraryId, out var remoteLibrary))
            return Result.Fail($"Remote library {remoteLibraryId} was not found");

        if (!libraries.TryGetValue(ownedLibraryId, out var ownedLibrary))
            return Result.Fail($"Owned library {ownedLibraryId} was not found");

        if (remoteLibrary.IsOwned)
            return Result.Fail($"Library {remoteLibraryId} is owned and cannot be compared as a remote library");

        if (!ownedLibrary.IsOwned)
            return Result.Fail($"Library {ownedLibraryId} is not owned and cannot be compared as an owned library");

        if (remoteLibrary.Type != PlexMediaType.TvShow || ownedLibrary.Type != PlexMediaType.TvShow)
            return Result.Fail("Both libraries must be TV show libraries");

        return Result.Ok();
    }

    private async Task<Result> CompareTvShowsAsync(
        int remoteLibraryId,
        int ownedLibraryId,
        CancellationToken cancellationToken
    )
    {
        var now = DateTime.UtcNow;

        var remoteShows = await LoadShowsAsync(remoteLibraryId, cancellationToken);
        var ownedShows = await LoadShowsAsync(ownedLibraryId, cancellationToken);
        var remoteSeasons = await LoadSeasonsAsync(remoteLibraryId, cancellationToken);
        var ownedSeasons = await LoadSeasonsAsync(ownedLibraryId, cancellationToken);
        var remoteEpisodes = await LoadEpisodesAsync(remoteLibraryId, cancellationToken);
        var ownedEpisodes = await LoadEpisodesAsync(ownedLibraryId, cancellationToken);

        var ownedByTmdb = ownedShows
            .Where(x => x.Guid_TMDB.HasValue)
            .GroupBy(x => x.Guid_TMDB!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ownedByImdb = ownedShows
            .Where(x => !string.IsNullOrEmpty(x.Guid_IMDB))
            .GroupBy(x => x.Guid_IMDB!)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var ownedByTvdb = ownedShows
            .Where(x => x.Guid_TVDB.HasValue)
            .GroupBy(x => x.Guid_TVDB!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ownedByTitleYear = ownedShows
            .GroupBy(GetTitleYearKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var ownedSeasonsByShowAndNumber = ownedSeasons
            .GroupBy(x => (x.TvShowId, x.SeasonNumber))
            .ToDictionary(g => g.Key, g => g.ToList());

        var ownedEpisodesBySeasonAndNumber = ownedEpisodes
            .GroupBy(x => (x.TvShowSeasonId, x.EpisodeNumber))
            .ToDictionary(g => g.Key, g => g.ToList());

        var remoteSeasonsByShow = remoteSeasons
            .GroupBy(x => x.TvShowId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var remoteEpisodesBySeason = remoteEpisodes
            .GroupBy(x => x.TvShowSeasonId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var showRows = new List<PlexTvShowComparison>();
        var seasonRows = new List<PlexSeasonComparison>();
        var episodeRows = new List<PlexEpisodeComparison>();

        foreach (var remoteShow in remoteShows)
        {
            var showMatches = MatchShows(remoteShow, ownedByTmdb, ownedByImdb, ownedByTvdb, ownedByTitleYear);

            foreach (var (ownedShow, showMatchType) in showMatches)
            {
                showRows.Add(new PlexTvShowComparison
                {
                    Id = 0,
                    RemotePlexLibraryId = remoteLibraryId,
                    OwnedPlexLibraryId = ownedLibraryId,
                    RemotePlexMediaId = remoteShow.Id,
                    OwnedPlexMediaId = ownedShow.Id,
                    HitState = IsHigherQuality(remoteShow.Quality, ownedShow.Quality)
                        ? PlexMediaComparisonHitState.HigherQuality
                        : PlexMediaComparisonHitState.Matched,
                    RemoteQuality = remoteShow.Quality,
                    OwnedQuality = ownedShow.Quality,
                    MatchType = showMatchType,
                    ComparedAt = now,
                });

                if (!remoteSeasonsByShow.TryGetValue(remoteShow.Id, out var showSeasons))
                    continue;

                foreach (var remoteSeason in showSeasons)
                {
                    if (!ownedSeasonsByShowAndNumber.TryGetValue((ownedShow.Id, remoteSeason.SeasonNumber), out var seasonMatches))
                        continue;

                    foreach (var ownedSeason in seasonMatches)
                    {
                        seasonRows.Add(new PlexSeasonComparison
                        {
                            Id = 0,
                            RemotePlexLibraryId = remoteLibraryId,
                            OwnedPlexLibraryId = ownedLibraryId,
                            RemotePlexMediaId = remoteSeason.Id,
                            OwnedPlexMediaId = ownedSeason.Id,
                            HitState = IsHigherQuality(remoteSeason.Quality, ownedSeason.Quality)
                                ? PlexMediaComparisonHitState.HigherQuality
                                : PlexMediaComparisonHitState.Matched,
                            RemoteQuality = remoteSeason.Quality,
                            OwnedQuality = ownedSeason.Quality,
                            MatchType = PlexMediaComparisonMatchType.ParentAndChildNumbers,
                            ComparedAt = now,
                        });

                        if (!remoteEpisodesBySeason.TryGetValue(remoteSeason.Id, out var seasonEpisodes))
                            continue;

                        foreach (var remoteEpisode in seasonEpisodes)
                        {
                            if (!ownedEpisodesBySeasonAndNumber.TryGetValue(
                                    (ownedSeason.Id, remoteEpisode.EpisodeNumber),
                                    out var episodeMatches
                                ))
                                continue;

                            episodeRows.AddRange(episodeMatches.Select(ownedEpisode => new PlexEpisodeComparison
                            {
                                Id = 0,
                                RemotePlexLibraryId = remoteLibraryId,
                                OwnedPlexLibraryId = ownedLibraryId,
                                RemotePlexMediaId = remoteEpisode.Id,
                                OwnedPlexMediaId = ownedEpisode.Id,
                                HitState = IsHigherQuality(remoteEpisode.Quality, ownedEpisode.Quality)
                                    ? PlexMediaComparisonHitState.HigherQuality
                                    : PlexMediaComparisonHitState.Matched,
                                RemoteQuality = remoteEpisode.Quality,
                                OwnedQuality = ownedEpisode.Quality,
                                MatchType = PlexMediaComparisonMatchType.ParentAndChildNumbers,
                                ComparedAt = now,
                            }));
                        }
                    }
                }
            }
        }

        await _dbContext.ExecuteWithRetryAsync(
            async ctx =>
            {
                var dbContext = (IReaparrDbContext)ctx;

                var attemptShowRows = showRows.Select(x => new PlexTvShowComparison
                    {
                        Id = 0,
                        RemotePlexLibraryId = x.RemotePlexLibraryId,
                        OwnedPlexLibraryId = x.OwnedPlexLibraryId,
                        RemotePlexMediaId = x.RemotePlexMediaId,
                        OwnedPlexMediaId = x.OwnedPlexMediaId,
                        HitState = x.HitState,
                        RemoteQuality = x.RemoteQuality,
                        OwnedQuality = x.OwnedQuality,
                        MatchType = x.MatchType,
                        ComparedAt = x.ComparedAt,
                    }).ToList();
                    var attemptSeasonRows = seasonRows.Select(x => new PlexSeasonComparison
                    {
                        Id = 0,
                        RemotePlexLibraryId = x.RemotePlexLibraryId,
                        OwnedPlexLibraryId = x.OwnedPlexLibraryId,
                        RemotePlexMediaId = x.RemotePlexMediaId,
                        OwnedPlexMediaId = x.OwnedPlexMediaId,
                        HitState = x.HitState,
                        RemoteQuality = x.RemoteQuality,
                        OwnedQuality = x.OwnedQuality,
                        MatchType = x.MatchType,
                        ComparedAt = x.ComparedAt,
                    }).ToList();
                    var attemptEpisodeRows = episodeRows.Select(x => new PlexEpisodeComparison
                    {
                        Id = 0,
                        RemotePlexLibraryId = x.RemotePlexLibraryId,
                        OwnedPlexLibraryId = x.OwnedPlexLibraryId,
                        RemotePlexMediaId = x.RemotePlexMediaId,
                        OwnedPlexMediaId = x.OwnedPlexMediaId,
                        HitState = x.HitState,
                        RemoteQuality = x.RemoteQuality,
                        OwnedQuality = x.OwnedQuality,
                        MatchType = x.MatchType,
                        ComparedAt = x.ComparedAt,
                    }).ToList();

                    // 1. Delete old hit rows for this pair.
                    await dbContext.PlexTvShowComparisons
                        .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.OwnedPlexLibraryId == ownedLibraryId)
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.PlexSeasonComparisons
                        .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.OwnedPlexLibraryId == ownedLibraryId)
                        .ExecuteDeleteAsync(cancellationToken);

                    await dbContext.PlexEpisodeComparisons
                        .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.OwnedPlexLibraryId == ownedLibraryId)
                        .ExecuteDeleteAsync(cancellationToken);

                    // 2. Insert new hit rows via standard EF — goes through the write queue.
                    if (attemptShowRows.Count > 0)
                        dbContext.PlexTvShowComparisons.AddRange(attemptShowRows);

                    if (attemptSeasonRows.Count > 0)
                        dbContext.PlexSeasonComparisons.AddRange(attemptSeasonRows);

                    if (attemptEpisodeRows.Count > 0)
                        dbContext.PlexEpisodeComparisons.AddRange(attemptEpisodeRows);

                    // 3. Update scope last — makes new hits visible atomically to readers.
                    var librarySnapshots = await dbContext.PlexLibraries
                        .Where(x => x.Id == remoteLibraryId || x.Id == ownedLibraryId)
                        .Select(x => new { x.Id, x.UpdatedAt })
                        .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, cancellationToken);

                    var state = await dbContext.PlexComparisonScopes
                        .AsTracking()
                        .SingleOrDefaultAsync(
                            x =>
                                x.RemotePlexLibraryId == remoteLibraryId
                                && x.OwnedPlexLibraryId == ownedLibraryId
                                && x.MediaType == PlexMediaType.TvShow,
                            cancellationToken
                        );

                    if (state is null)
                    {
                        state = new PlexComparisonState
                        {
                            Id = 0,
                            RemotePlexLibraryId = remoteLibraryId,
                            OwnedPlexLibraryId = ownedLibraryId,
                            MediaType = PlexMediaType.TvShow,
                            CompletedAt = now,
                            RemoteLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(remoteLibraryId),
                            OwnedLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(ownedLibraryId),
                        };

                        await dbContext.PlexComparisonScopes.AddAsync(state, cancellationToken);
                    }
                    else
                    {
                        state.CompletedAt = now;
                        state.RemoteLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(remoteLibraryId);
                        state.OwnedLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(ownedLibraryId);
                    }

                    await dbContext.SaveChangesNewAsync(cancellationToken);

                    return 0;
            },
            cancellationToken: cancellationToken
        );

        _log.Here()
            .Information(
                "Completed TV comparison: remote library {RemoteLibId} vs owned library {OwnedLibId}, {ShowCount} show hits, {SeasonCount} season hits, {EpisodeCount} episode hits",
                remoteLibraryId,
                ownedLibraryId,
                showRows.Count,
                seasonRows.Count,
                episodeRows.Count
            );

        return Result.Ok();
    }

    private Task<List<ShowProjection>> LoadShowsAsync(int libraryId, CancellationToken cancellationToken) =>
        _dbContext.PlexTvShows
            .Where(x => x.PlexLibraryId == libraryId)
            .Select(x => new ShowProjection
            {
                Id = x.Id,
                SearchTitle = x.SearchTitle,
                Year = x.Year,
                Duration = x.Duration,
                Quality = x.Quality,
                Guid_IMDB = x.Guid_IMDB,
                Guid_TMDB = x.Guid_TMDB,
                Guid_TVDB = x.Guid_TVDB,
            })
            .ToListAsync(cancellationToken);

    private Task<List<SeasonProjection>> LoadSeasonsAsync(int libraryId, CancellationToken cancellationToken) =>
        _dbContext.PlexTvShowSeason
            .Where(x => x.PlexLibraryId == libraryId)
            .Select(x => new SeasonProjection
            {
                Id = x.Id,
                TvShowId = x.TvShowId,
                SeasonNumber = x.SeasonNumber,
                Quality = x.Quality,
            })
            .ToListAsync(cancellationToken);

    private Task<List<EpisodeProjection>> LoadEpisodesAsync(int libraryId, CancellationToken cancellationToken) =>
        _dbContext.PlexTvShowEpisodes
            .Where(x => x.PlexLibraryId == libraryId)
            .Select(x => new EpisodeProjection
            {
                Id = x.Id,
                TvShowSeasonId = x.TvShowSeasonId,
                EpisodeNumber = x.EpisodeNumber,
                Quality = x.Quality,
            })
            .ToListAsync(cancellationToken);

    private static List<(ShowProjection Owned, PlexMediaComparisonMatchType MatchType)> MatchShows(
        ShowProjection remote,
        Dictionary<int, List<ShowProjection>> ownedByTmdb,
        Dictionary<string, List<ShowProjection>> ownedByImdb,
        Dictionary<int, List<ShowProjection>> ownedByTvdb,
        Dictionary<string, List<ShowProjection>> ownedByTitleYear
    )
    {
        if (remote.Guid_TMDB.HasValue && ownedByTmdb.TryGetValue(remote.Guid_TMDB.Value, out var tmdbMatches))
            return tmdbMatches.Select(m => (m, PlexMediaComparisonMatchType.TmdbGuid)).ToList();

        if (!string.IsNullOrEmpty(remote.Guid_IMDB) && ownedByImdb.TryGetValue(remote.Guid_IMDB, out var imdbMatches))
            return imdbMatches.Select(m => (m, PlexMediaComparisonMatchType.ImdbGuid)).ToList();

        if (remote.Guid_TVDB.HasValue && ownedByTvdb.TryGetValue(remote.Guid_TVDB.Value, out var tvdbMatches))
            return tvdbMatches.Select(m => (m, PlexMediaComparisonMatchType.TvdbGuid)).ToList();

        if (!ownedByTitleYear.TryGetValue(GetTitleYearKey(remote), out var titleYearMatches))
            return [];

        if (remote.Duration > 0)
        {
            var durationMatches = titleYearMatches
                .Where(o => o.Duration > 0 && o.Duration == remote.Duration)
                .ToList();

            if (durationMatches.Count > 0)
                return durationMatches.Select(m => (m, PlexMediaComparisonMatchType.NormalizedTitleYearAndDuration)).ToList();
        }

        return titleYearMatches.Select(m => (m, PlexMediaComparisonMatchType.NormalizedTitleAndYear)).ToList();
    }

    private static bool IsHigherQuality(VideoQuality remoteQuality, VideoQuality ownedQuality) =>
        remoteQuality.ToId() > ownedQuality.ToId();

    private static string GetTitleYearKey(ShowProjection show) => $"{show.SearchTitle}\u001F{show.Year}";

    private sealed record ShowProjection
    {
        public required int Id { get; init; }
        public required string SearchTitle { get; init; }
        public required int Year { get; init; }
        public required int Duration { get; init; }
        public required VideoQuality Quality { get; init; }
        public required string? Guid_IMDB { get; init; }
        public required int? Guid_TMDB { get; init; }
        public required int? Guid_TVDB { get; init; }
    }

    private sealed record SeasonProjection
    {
        public required int Id { get; init; }
        public required int TvShowId { get; init; }
        public required int SeasonNumber { get; init; }
        public required VideoQuality Quality { get; init; }
    }

    private sealed record EpisodeProjection
    {
        public required int Id { get; init; }
        public required int TvShowSeasonId { get; init; }
        public required int EpisodeNumber { get; init; }
        public required VideoQuality Quality { get; init; }
    }
}
