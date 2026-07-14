using Microsoft.EntityFrameworkCore;

namespace Reaparr.Application;

public record GetTvShowLibraryComparisonDebugEndpointRequest
{
    [QueryParam, BindFrom("remoteLibraryId")]
    public int RemoteLibraryId { get; init; }

    [QueryParam, BindFrom("ownedLibraryId")]
    public int OwnedLibraryId { get; init; }
}

public class GetTvShowLibraryComparisonDebugEndpointRequestValidator
    : Validator<GetTvShowLibraryComparisonDebugEndpointRequest>
{
    public GetTvShowLibraryComparisonDebugEndpointRequestValidator()
    {
        RuleFor(x => x.RemoteLibraryId).GreaterThan(0).NotEqual(x => x.OwnedLibraryId);
        RuleFor(x => x.OwnedLibraryId).GreaterThan(0);
    }
}

public record TvShowLibraryComparisonDebugResponseDTO(
    int RemoteLibraryId,
    int OwnedLibraryId,
    TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugShowHitDTO> Shows,
    TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugSeasonHitDTO> Seasons,
    TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugEpisodeHitDTO> Episodes
);

public record TvShowLibraryComparisonDebugSectionDTO<THit>(
    List<THit> Matched,
    List<THit> HigherQuality,
    int MissingCount
);

public record TvShowLibraryComparisonDebugShowHitDTO(
    int RemoteMediaId,
    string RemoteTitle,
    int RemoteYear,
    VideoQuality RemoteQuality,
    int OwnedMediaId,
    string OwnedTitle,
    int OwnedYear,
    VideoQuality OwnedQuality,
    PlexMediaComparisonMatchType MatchType,
    DateTime ComparedAt
);


public record TvShowLibraryComparisonDebugSeasonHitDTO(
    int RemoteMediaId,
    int RemoteTvShowId,
    string RemoteTitle,
    int RemoteSeasonNumber,
    VideoQuality RemoteQuality,
    int OwnedMediaId,
    int OwnedTvShowId,
    string OwnedTitle,
    int OwnedSeasonNumber,
    VideoQuality OwnedQuality,
    PlexMediaComparisonMatchType MatchType,
    DateTime ComparedAt
);


public record TvShowLibraryComparisonDebugEpisodeHitDTO(
    int RemoteMediaId,
    int RemoteTvShowId,
    int RemoteSeasonId,
    string RemoteTitle,
    int RemoteEpisodeNumber,
    VideoQuality RemoteQuality,
    int OwnedMediaId,
    int OwnedTvShowId,
    int OwnedSeasonId,
    string OwnedTitle,
    int OwnedEpisodeNumber,
    VideoQuality OwnedQuality,
    PlexMediaComparisonMatchType MatchType,
    DateTime ComparedAt
);


public class GetTvShowLibraryComparisonDebugEndpoint
    : Endpoint<GetTvShowLibraryComparisonDebugEndpointRequest, ResultDTO<TvShowLibraryComparisonDebugResponseDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public GetTvShowLibraryComparisonDebugEndpoint(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.DebugController + "/tv-show-library-comparison");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<TvShowLibraryComparisonDebugResponseDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetTvShowLibraryComparisonDebugEndpointRequest req, CancellationToken ct)
    {
        var compareResult = await _commandExecutor.Send(
            new CompareTvShowPlexLibraryCommand(req.RemoteLibraryId, req.OwnedLibraryId),
            ct
        );

        if (compareResult.IsFailed)
        {
            await Send.FluentResult(compareResult, ct);
            return;
        }

        var showHits = await GetShowHitsAsync(req, ct);
        var seasonHits = await GetSeasonHitsAsync(req, ct);
        var episodeHits = await GetEpisodeHitsAsync(req, ct);

        var response = new TvShowLibraryComparisonDebugResponseDTO(
            req.RemoteLibraryId,
            req.OwnedLibraryId,
            new TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugShowHitDTO>(
                showHits.Where(x => x.HitState == PlexMediaComparisonHitState.Matched).Select(x => x.Hit).ToList(),
                showHits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality).Select(x => x.Hit).ToList(),
                await GetMissingShowCountAsync(req, ct)
            ),
            new TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugSeasonHitDTO>(
                seasonHits.Where(x => x.HitState == PlexMediaComparisonHitState.Matched).Select(x => x.Hit).ToList(),
                seasonHits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality).Select(x => x.Hit).ToList(),
                await GetMissingSeasonCountAsync(req, ct)
            ),
            new TvShowLibraryComparisonDebugSectionDTO<TvShowLibraryComparisonDebugEpisodeHitDTO>(
                episodeHits.Where(x => x.HitState == PlexMediaComparisonHitState.Matched).Select(x => x.Hit).ToList(),
                episodeHits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality).Select(x => x.Hit).ToList(),
                await GetMissingEpisodeCountAsync(req, ct)
            )
        );

        await Send.FluentResult(Result.Ok(response), ct);
    }

    private Task<List<TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugShowHitDTO>>> GetShowHitsAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexTvShowComparisons
            .Where(x => x.RemotePlexLibraryId == req.RemoteLibraryId && x.OwnedPlexLibraryId == req.OwnedLibraryId)
            .Join(_dbContext.PlexTvShows, comparison => comparison.RemotePlexMediaId, remote => remote.Id, (comparison, remote) => new { comparison, remote })
            .Join(_dbContext.PlexTvShows, x => x.comparison.OwnedPlexMediaId, owned => owned.Id, (x, owned) => new { x.comparison, x.remote, owned })
            .OrderBy(x => x.remote.Title)
            .ThenBy(x => x.remote.Year)
            .Select(x => new TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugShowHitDTO>(
                x.comparison.HitState,
                new TvShowLibraryComparisonDebugShowHitDTO(
                    x.remote.Id,
                    x.remote.Title,
                    x.remote.Year,
                    x.comparison.RemoteQuality,
                    x.owned.Id,
                    x.owned.Title,
                    x.owned.Year,
                    x.comparison.OwnedQuality,
                    x.comparison.MatchType,
                    x.comparison.ComparedAt
                )
            ))
            .ToListAsync(ct);

    private Task<int> GetMissingShowCountAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexTvShows
            .Where(x => x.PlexLibraryId == req.RemoteLibraryId)
            .CountAsync(remote => !_dbContext.PlexTvShowComparisons.Any(comparison =>
                    comparison.RemotePlexLibraryId == req.RemoteLibraryId
                    && comparison.OwnedPlexLibraryId == req.OwnedLibraryId
                    && comparison.RemotePlexMediaId == remote.Id
                ),
                ct
            );

    private Task<List<TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugSeasonHitDTO>>> GetSeasonHitsAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexSeasonComparisons
            .Where(x => x.RemotePlexLibraryId == req.RemoteLibraryId && x.OwnedPlexLibraryId == req.OwnedLibraryId)
            .Join(_dbContext.PlexTvShowSeason, comparison => comparison.RemotePlexMediaId, remote => remote.Id, (comparison, remote) => new { comparison, remote })
            .Join(_dbContext.PlexTvShowSeason, x => x.comparison.OwnedPlexMediaId, owned => owned.Id, (x, owned) => new { x.comparison, x.remote, owned })
            .OrderBy(x => x.remote.TvShowId)
            .ThenBy(x => x.remote.SeasonNumber)
            .Select(x => new TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugSeasonHitDTO>(
                x.comparison.HitState,
                new TvShowLibraryComparisonDebugSeasonHitDTO(
                    x.remote.Id,
                    x.remote.TvShowId,
                    x.remote.Title,
                    x.remote.SeasonNumber,
                    x.comparison.RemoteQuality,
                    x.owned.Id,
                    x.owned.TvShowId,
                    x.owned.Title,
                    x.owned.SeasonNumber,
                    x.comparison.OwnedQuality,
                    x.comparison.MatchType,
                    x.comparison.ComparedAt
                )
            ))
            .ToListAsync(ct);

    private Task<int> GetMissingSeasonCountAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexTvShowSeason
            .Where(x => x.PlexLibraryId == req.RemoteLibraryId)
            .CountAsync(remote => !_dbContext.PlexSeasonComparisons.Any(comparison =>
                    comparison.RemotePlexLibraryId == req.RemoteLibraryId
                    && comparison.OwnedPlexLibraryId == req.OwnedLibraryId
                    && comparison.RemotePlexMediaId == remote.Id
                ),
                ct
            );

    private Task<List<TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugEpisodeHitDTO>>> GetEpisodeHitsAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexEpisodeComparisons
            .Where(x => x.RemotePlexLibraryId == req.RemoteLibraryId && x.OwnedPlexLibraryId == req.OwnedLibraryId)
            .Join(_dbContext.PlexTvShowEpisodes, comparison => comparison.RemotePlexMediaId, remote => remote.Id, (comparison, remote) => new { comparison, remote })
            .Join(_dbContext.PlexTvShowEpisodes, x => x.comparison.OwnedPlexMediaId, owned => owned.Id, (x, owned) => new { x.comparison, x.remote, owned })
            .OrderBy(x => x.remote.TvShowId)
            .ThenBy(x => x.remote.TvShowSeasonId)
            .ThenBy(x => x.remote.EpisodeNumber)
            .Select(x => new TvShowLibraryComparisonDebugHit<TvShowLibraryComparisonDebugEpisodeHitDTO>(
                x.comparison.HitState,
                new TvShowLibraryComparisonDebugEpisodeHitDTO(
                    x.remote.Id,
                    x.remote.TvShowId,
                    x.remote.TvShowSeasonId,
                    x.remote.Title,
                    x.remote.EpisodeNumber,
                    x.comparison.RemoteQuality,
                    x.owned.Id,
                    x.owned.TvShowId,
                    x.owned.TvShowSeasonId,
                    x.owned.Title,
                    x.owned.EpisodeNumber,
                    x.comparison.OwnedQuality,
                    x.comparison.MatchType,
                    x.comparison.ComparedAt
                )
            ))
            .ToListAsync(ct);

    private Task<int> GetMissingEpisodeCountAsync(
        GetTvShowLibraryComparisonDebugEndpointRequest req,
        CancellationToken ct
    ) =>
        _dbContext.PlexTvShowEpisodes
            .Where(x => x.PlexLibraryId == req.RemoteLibraryId)
            .CountAsync(remote => !_dbContext.PlexEpisodeComparisons.Any(comparison =>
                    comparison.RemotePlexLibraryId == req.RemoteLibraryId
                    && comparison.OwnedPlexLibraryId == req.OwnedLibraryId
                    && comparison.RemotePlexMediaId == remote.Id
                ),
                ct
            );

    private sealed record TvShowLibraryComparisonDebugHit<THit>(PlexMediaComparisonHitState HitState, THit Hit);
}
