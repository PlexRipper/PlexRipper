using FastEndpoints;
using FluentResults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Reaparr.Application;

public class CompareMoviePlexLibraryCommandValidator : AbstractValidator<CompareMoviePlexLibraryCommand>
{
    public CompareMoviePlexLibraryCommandValidator()
    {
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
        RuleFor(x => x.MediaType).IsInEnum();
    }
}

public class CompareMoviePlexLibraryCommandHandler : ICommandHandler<CompareMoviePlexLibraryCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly INotificationHubService _notificationHubService;

    private const int AlgorithmVersion = 1;

    public CompareMoviePlexLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IReaparrDbContextFactory dbContextFactory,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<CompareMoviePlexLibraryCommandHandler>();
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result> ExecuteAsync(
        CompareMoviePlexLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        return await Result.Try(new Func<Task<Result>>(async () =>
        {
            var (remoteLibraryId, ownedLibraryId, mediaType) = command;

            _log.Here()
                .Information(
                    "Starting comparison for media type {MediaType}: remote library {RemoteLibId} vs owned library {OwnedLibId}",
                    mediaType,
                    remoteLibraryId,
                    ownedLibraryId
                );

            return mediaType switch
            {
                PlexMediaType.Movie => await CompareMoviesAsync(remoteLibraryId, ownedLibraryId, cancellationToken),
                _ => Result.Fail($"Media type {mediaType} comparison is not yet implemented"),
            };
        }));
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
            .AsNoTracking()
            .Where(m => m.PlexLibraryId == remoteLibraryId)
            .Select(m => new MovieProjection
            {
                Id = m.Id,
                Title = m.Title,
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
            .AsNoTracking()
            .Where(m => m.PlexLibraryId == ownedLibraryId)
            .Select(m => new MovieProjection
            {
                Id = m.Id,
                Title = m.Title,
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

        var nowTimestamp = now;

        var hitRows = new List<PlexMovieComparison>();

        foreach (var remote in remoteMovies)
        {
            var matches = MatchMovie(remote, ownedByTmdb, ownedByImdb, ownedByTvdb, ownedMovies);

            foreach (var (owned, matchType) in matches)
            {
                var remoteQuality = remote.Quality;
                var ownedQuality = owned.Quality;
                var isHigherQuality = remoteQuality > ownedQuality;

                hitRows.Add(new PlexMovieComparison
                {
                    Id = 0,
                    RemotePlexLibraryId = remoteLibraryId,
                    OwnedPlexLibraryId = ownedLibraryId,
                    RemotePlexMediaId = remote.Id,
                    OwnedPlexMediaId = owned.Id,
                    HitState = isHigherQuality ? PlexMediaComparisonHitState.HigherQuality : PlexMediaComparisonHitState.Matched,
                    RemoteQuality = remoteQuality,
                    OwnedQuality = ownedQuality,
                    MatchType = matchType,
                    ComparedAt = nowTimestamp,
                    AlgorithmVersion = AlgorithmVersion,
                });
            }
        }

        _log.Here()
            .Debug(
                "Produced {HitCount} comparison hit rows for {RemoteCount} remote movies",
                hitRows.Count,
                remoteMovies.Count
            );

        // Atomic scope update: mark old scopes as not current, insert new scope + hits
        using var dbContext = await _dbContextFactory.CreateAsync();
        await using var transaction = await dbContext.BeginTransactionAsync(cancellationToken);

        // Insert new scope
        var scope = new PlexComparisonState
        {
            Id = 0,
            RemotePlexLibraryId = remoteLibraryId,
            OwnedPlexLibraryId = ownedLibraryId,
            MediaType = PlexMediaType.Movie,
            CompletedAt = nowTimestamp,
            AlgorithmVersion = AlgorithmVersion,
        };

        await dbContext.PlexComparisonScopes.AddAsync(scope, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Insert hit rows
        if (hitRows.Count > 0)
        {
            await dbContext.BulkInsertAsync(hitRows, cancellationToken: cancellationToken);
        }

        // Clean up orphan hit rows from old versions
        await dbContext.PlexMovieComparisons
            .Where(c =>
                c.RemotePlexLibraryId == remoteLibraryId
                && c.OwnedPlexLibraryId == ownedLibraryId
                && c.AlgorithmVersion < AlgorithmVersion
            )
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

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
        List<MovieProjection> allOwnedMovies
    )
    {
        // GUID waterfall: TMDB > IMDB > TVDB
        if (remote.Guid_TMDB.HasValue && ownedByTmdb.TryGetValue(remote.Guid_TMDB.Value, out var tmdbMatches))
            return tmdbMatches.Select(m => (m, PlexMediaComparisonMatchType.TmdbGuid)).ToList();

        if (!string.IsNullOrEmpty(remote.Guid_IMDB) && ownedByImdb.TryGetValue(remote.Guid_IMDB, out var imdbMatches))
            return imdbMatches.Select(m => (m, PlexMediaComparisonMatchType.ImdbGuid)).ToList();

        if (remote.Guid_TVDB.HasValue && ownedByTvdb.TryGetValue(remote.Guid_TVDB.Value, out var tvdbMatches))
            return tvdbMatches.Select(m => (m, PlexMediaComparisonMatchType.TvdbGuid)).ToList();

        // Title/year fallback
        var titleYearMatches = allOwnedMovies
            .Where(o =>
                string.Equals(o.SearchTitle, remote.SearchTitle, StringComparison.OrdinalIgnoreCase)
                && o.Year == remote.Year
            )
            .ToList();

        if (titleYearMatches.Count > 0)
        {
            // If duration is available, try duration match first
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

        return [];
    }

    private async Task<DateTime?> GetLibraryUpdatedAt(int libraryId, CancellationToken cancellationToken)
    {
        return await _dbContext.PlexLibraries
            .Where(l => l.Id == libraryId)
            .Select(l => l.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Projection used for matching — only the fields needed for comparison.
    /// </summary>
    private sealed record MovieProjection
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required string SearchTitle { get; init; }
        public required int Year { get; init; }
        public required int Duration { get; init; }
        public required VideoQuality Quality { get; init; }
        public required string? Guid_IMDB { get; init; }
        public required int? Guid_TMDB { get; init; }
        public required int? Guid_TVDB { get; init; }
    }
}
