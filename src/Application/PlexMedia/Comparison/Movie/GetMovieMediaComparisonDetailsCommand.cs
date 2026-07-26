namespace Reaparr.Application;

public record GetMovieMediaComparisonDetailsCommand(int PlexMediaId) : ICommand<Result<PlexMediaComparisonDetailsDTO>>;

public class GetMovieMediaComparisonDetailsCommandValidator : AbstractValidator<GetMovieMediaComparisonDetailsCommand>
{
    public GetMovieMediaComparisonDetailsCommandValidator()
    {
        RuleFor(x => x.PlexMediaId).GreaterThan(0);
    }
}

public class GetMovieMediaComparisonDetailsCommandHandler
    : ICommandHandler<GetMovieMediaComparisonDetailsCommand, Result<PlexMediaComparisonDetailsDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private int _rowId;

    public GetMovieMediaComparisonDetailsCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PlexMediaComparisonDetailsDTO>> ExecuteAsync(
        GetMovieMediaComparisonDetailsCommand command,
        CancellationToken ct)
    {
        var movie = await _dbContext.PlexMovies.Include(x => x.MediaDataList)
            .SingleOrDefaultAsync(x => x.Id == command.PlexMediaId, ct);
        if (movie is null)
            return ResultExtensions.EntityNotFound(nameof(PlexMovie), command.PlexMediaId).LogError();

        var isOwned = await _dbContext.PlexLibraries.WhereIsOwned().AnyAsync(x => x.Id == movie.PlexLibraryId, ct);
        var rows = isOwned
            ? await GetOwnedMovieRowsAsync(movie, ct)
            : await GetRemoteMovieRowsAsync(movie, ct);

        return Result.Ok(new PlexMediaComparisonDetailsDTO
        {
            PlexMediaId = movie.Id,
            Type = PlexMediaType.Movie,
            State = PlexMediaComparisonDetailsMapper.ToParentState(rows),
            Rows = PlexMediaComparisonDetailsMapper.ToDtoRows(rows),
        });
    }

    private async Task<List<ComparisonDetailsRow>> GetRemoteMovieRowsAsync(PlexMovie movie, CancellationToken ct)
    {
        var currentOwnedLibraryIds = await _dbContext.GetCurrentOwnedLibraryIds(movie.PlexLibraryId, PlexMediaType.Movie, ct);
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
                PlexMediaComparisonDetailsMapper.ToComparisonDetailsRow(
                    rowId: ++_rowId,
                    parentRowId: null,
                    level: 0,
                    plexMediaId: movie.Id,
                    type: PlexMediaType.Movie,
                    title: movie.Title,
                    state: PlexMediaComparisonState.Missing,
                    remoteQuality: movie.Quality,
                    ownedQuality: VideoQuality.None,
                    remoteLocation: movie.MediaDataList.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    ownedLocation: string.Empty,
                    remotePlexLibraryId: movie.PlexLibraryId,
                    remotePlexServerId: await _dbContext.GetPlexServerIdFromPlexLibraryId(movie.PlexLibraryId))
            ];

        return await CreateRemoteMovieUpgradeRowsAsync(movie, hits, ct);
    }

    private async Task<List<ComparisonDetailsRow>> CreateRemoteMovieUpgradeRowsAsync(
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

        var remoteServerIds = await GetPlexServerIdsByLibraryIdAsync(upgradeHits.Select(x => x.RemotePlexLibraryId), ct);

        return upgradeHits
            .Select(hit =>
            {
                ownedMovies.TryGetValue(hit.OwnedPlexMediaId, out var ownedMovie);
                return PlexMediaComparisonDetailsMapper.ToComparisonDetailsRow(
                    rowId: ++_rowId,
                    parentRowId: null,
                    level: 0,
                    plexMediaId: movie.Id,
                    type: PlexMediaType.Movie,
                    title: movie.Title,
                    state: PlexMediaComparisonState.HigherQuality,
                    remoteQuality: hit.RemoteQuality,
                    ownedQuality: hit.OwnedQuality,
                    remoteLocation: movie.MediaDataList.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    ownedLocation: ownedMovie?.MediaDataList.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    remotePlexLibraryId: hit.RemotePlexLibraryId,
                    remotePlexServerId: remoteServerIds.GetValueOrDefault(hit.RemotePlexLibraryId));
            })
            .ToList();
    }

    private async Task<List<ComparisonDetailsRow>> GetOwnedMovieRowsAsync(PlexMovie movie, CancellationToken ct)
    {
        var currentRemoteLibraryIds = await _dbContext.GetCurrentRemoteLibraryIds(movie.PlexLibraryId, PlexMediaType.Movie, ct);
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
        var remoteServerIds = await GetPlexServerIdsByLibraryIdAsync(upgradeHits.Select(x => x.RemotePlexLibraryId), ct);

        return upgradeHits
            .Select(hit =>
            {
                remoteMovies.TryGetValue(hit.RemotePlexMediaId, out var remoteMovie);
                return PlexMediaComparisonDetailsMapper.ToComparisonDetailsRow(
                    rowId: ++_rowId,
                    parentRowId: null,
                    level: 0,
                    plexMediaId: remoteMovie?.Id ?? hit.RemotePlexMediaId,
                    type: PlexMediaType.Movie,
                    title: remoteMovie?.Title ?? movie.Title,
                    state: PlexMediaComparisonState.HigherQuality,
                    remoteQuality: hit.RemoteQuality,
                    ownedQuality: hit.OwnedQuality,
                    remoteLocation: remoteMovie?.MediaDataList.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    ownedLocation: movie.MediaDataList.Select(x => x.GetFileName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    remotePlexLibraryId: hit.RemotePlexLibraryId,
                    remotePlexServerId: remoteServerIds.GetValueOrDefault(hit.RemotePlexLibraryId));
            })
            .ToList();
    }

    private async Task<Dictionary<int, int>> GetPlexServerIdsByLibraryIdAsync(IEnumerable<int> plexLibraryIds, CancellationToken ct)
    {
        var ids = plexLibraryIds.ToHashSet();
        return await _dbContext.PlexLibraries
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.PlexServerId, ct);
    }
}