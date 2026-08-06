namespace Reaparr.Application;

public class CompareMoviePlexLibraryCommandValidator : AbstractValidator<CompareMoviePlexLibraryCommand>
{
    public CompareMoviePlexLibraryCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0).NotEqual(x => x.OwnedPlexLibraryId);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
    }
}

public class CompareMoviePlexLibraryCommandHandler : ICommandHandler<CompareMoviePlexLibraryCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public CompareMoviePlexLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext
    )
    {
        _log = log.ForContext<CompareMoviePlexLibraryCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        CompareMoviePlexLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        return await Result.Try(new Func<Task<Result>>(async () =>
        {
            var (remoteLibraryId, ownedLibraryId) = command;

            _log.Here()
                .Information(
                    "Starting movie comparison: remote library {RemoteLibId} vs owned library {OwnedLibId}",
                    remoteLibraryId,
                    ownedLibraryId
                );

            var validationResult = await ValidateLibrariesAsync(remoteLibraryId, ownedLibraryId, cancellationToken);

            if (validationResult.IsFailed)
                return validationResult;

            return await CompareMoviesAsync(remoteLibraryId, ownedLibraryId, cancellationToken);
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

        if (remoteLibrary.Type != PlexMediaType.Movie || ownedLibrary.Type != PlexMediaType.Movie)
            return Result.Fail("Both libraries must be movie libraries");

        return Result.Ok();
    }

    private async Task<Result> CompareMoviesAsync(
        int remoteLibraryId,
        int ownedLibraryId,
        CancellationToken cancellationToken
    )
    {
        var now = DateTime.UtcNow;

        // Load remote movies with their quality
        var remoteMovies = await _dbContext.PlexMovies
            .Where(m => m.PlexLibraryId == remoteLibraryId)
            .Select(m => new MovieProjection
            {
                Id = m.Id,
                SearchTitle = m.SearchTitle,
                Year = m.Year,
                Duration = m.Duration,
                Quality = m.Quality,
                Guid_IMDB = m.Guid_IMDB,
                Guid_TMDB = m.Guid_TMDB,
                Guid_TVDB = m.Guid_TVDB,
            })
            .ToListAsync(cancellationToken);

        // Load owned movies with their quality
        var ownedMovies = await _dbContext.PlexMovies
            .Where(m => m.PlexLibraryId == ownedLibraryId)
            .Select(m => new MovieProjection
            {
                Id = m.Id,
                SearchTitle = m.SearchTitle,
                Year = m.Year,
                Duration = m.Duration,
                Quality = m.Quality,
                Guid_IMDB = m.Guid_IMDB,
                Guid_TMDB = m.Guid_TMDB,
                Guid_TVDB = m.Guid_TVDB,
            })
            .ToListAsync(cancellationToken);

        _log.Here()
            .Debug(
                "Loaded {RemoteCount} remote movies and {OwnedCount} owned movies for comparison",
                remoteMovies.Count,
                ownedMovies.Count
            );

        // Build GUID lookup dictionaries
        var ownedByTmdb = ownedMovies
            .Where(x => x.Guid_TMDB.HasValue)
            .GroupBy(x => x.Guid_TMDB!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ownedByImdb = ownedMovies
            .Where(x => !string.IsNullOrEmpty(x.Guid_IMDB))
            .GroupBy(x => x.Guid_IMDB!)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var ownedByTvdb = ownedMovies
            .Where(x => x.Guid_TVDB.HasValue)
            .GroupBy(x => x.Guid_TVDB!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ownedByTitleYear = ownedMovies
            .GroupBy(GetTitleYearKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var hitRows = new List<PlexMovieComparison>();

        foreach (var remote in remoteMovies)
        {
            var matches = MatchMovie(remote, ownedByTmdb, ownedByImdb, ownedByTvdb, ownedByTitleYear);

            foreach (var (owned, matchType) in matches)
            {
                var remoteQuality = remote.Quality;
                var ownedQuality = owned.Quality;

                hitRows.Add(new PlexMovieComparison
                {
                    Id = 0,
                    RemotePlexLibraryId = remoteLibraryId,
                    OwnedPlexLibraryId = ownedLibraryId,
                    RemotePlexMediaId = remote.Id,
                    OwnedPlexMediaId = owned.Id,
                    HitState = IsHigherQuality(remoteQuality, ownedQuality)
                        ? PlexMediaComparisonHitState.HigherQuality
                        : PlexMediaComparisonHitState.Matched,
                    RemoteQuality = remoteQuality,
                    OwnedQuality = ownedQuality,
                    MatchType = matchType,
                    ComparedAt = now,
                });
            }
        }

        _log.Here()
            .Debug(
                "Produced {HitCount} comparison hit rows for {RemoteCount} remote movies",
                hitRows.Count,
                remoteMovies.Count
            );

        var transactionResult = await _dbContext.ExecuteSerializedTransactionAsync(async (ctx, txCt) =>
        {
            var librarySnapshots = await ctx.PlexLibraries
                .Where(x => x.Id == remoteLibraryId || x.Id == ownedLibraryId)
                .Select(x => new { x.Id, x.UpdatedAt })
                .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, txCt);

            var state = await ctx.PlexComparisonScopes
                .AsTracking()
                .SingleOrDefaultAsync(
                    x =>
                        x.RemotePlexLibraryId == remoteLibraryId
                        && x.OwnedPlexLibraryId == ownedLibraryId
                        && x.MediaType == PlexMediaType.Movie,
                    txCt
                );

            if (state is null)
            {
                state = new PlexComparisonState
                {
                    Id = 0,
                    RemotePlexLibraryId = remoteLibraryId,
                    OwnedPlexLibraryId = ownedLibraryId,
                    MediaType = PlexMediaType.Movie,
                    CompletedAt = now,
                    RemoteLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(remoteLibraryId),
                    OwnedLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(ownedLibraryId),
                };

                await ctx.PlexComparisonScopes.AddAsync(state, txCt);
            }
            else
            {
                state.CompletedAt = now;
                state.RemoteLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(remoteLibraryId);
                state.OwnedLibraryUpdatedAt = librarySnapshots.GetValueOrDefault(ownedLibraryId);
            }
            await ctx.PlexMovieComparisons
                .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.OwnedPlexLibraryId == ownedLibraryId)
                .ExecuteDeleteAsync(txCt);

            if (hitRows.Count > 0)
                ctx.PlexMovieComparisons.AddRange(hitRows);

            await ctx.SaveChangesAsync(txCt);
        }, cancellationToken);
        if (transactionResult.IsFailed)
            return transactionResult;

        _log.Here()
            .Information(
                "Completed comparison for movies: remote library {RemoteLibId} vs owned library {OwnedLibId}, {HitCount} hits",
                remoteLibraryId,
                ownedLibraryId,
                hitRows.Count
            );

        return Result.Ok();
    }

    private static List<(MovieProjection Owned, PlexMediaComparisonMatchType MatchType)> MatchMovie(
        MovieProjection remote,
        Dictionary<int, List<MovieProjection>> ownedByTmdb,
        Dictionary<string, List<MovieProjection>> ownedByImdb,
        Dictionary<int, List<MovieProjection>> ownedByTvdb,
        Dictionary<string, List<MovieProjection>> ownedByTitleYear
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

    private static string GetTitleYearKey(MovieProjection movie) => $"{movie.SearchTitle}\u001F{movie.Year}";

    /// <summary>
    /// Projection used for matching — only the fields needed for comparison.
    /// </summary>
    private sealed record MovieProjection
    {
        public required int Id { get; init; }
        public required string SearchTitle { get; init; }
        public required int Year { get; init; }
        public required int Duration { get; init; }
        public required VideoQuality Quality { get; init; }
        
        // ReSharper disable once InconsistentNaming
        public required string? Guid_IMDB { get; init; }
        
        // ReSharper disable once InconsistentNaming
        public required int? Guid_TMDB { get; init; }
        
        // ReSharper disable once InconsistentNaming
        public required int? Guid_TVDB { get; init; }
    }
}
