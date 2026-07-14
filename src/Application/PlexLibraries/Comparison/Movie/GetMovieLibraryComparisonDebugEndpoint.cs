using Microsoft.EntityFrameworkCore;

namespace Reaparr.Application;

public record GetMovieLibraryComparisonDebugEndpointRequest
{
    [QueryParam, BindFrom("ownedLibraryId")]
    public int OwnedLibraryId { get; init; }
    
    [QueryParam, BindFrom("remoteLibraryId")]
    public int RemoteLibraryId { get; init; }
}

public class GetMovieLibraryComparisonDebugEndpointRequestValidator
    : Validator<GetMovieLibraryComparisonDebugEndpointRequest>
{
    public GetMovieLibraryComparisonDebugEndpointRequestValidator()
    {
        RuleFor(x => x.RemoteLibraryId).GreaterThan(0).NotEqual(x => x.OwnedLibraryId);
        RuleFor(x => x.OwnedLibraryId).GreaterThan(0);
    }
}

public record MovieLibraryComparisonDebugResponseDTO(
    int RemoteLibraryId,
    int OwnedLibraryId,
    List<MovieLibraryComparisonDebugHitDTO> Matched,
    List<MovieLibraryComparisonDebugHitDTO> HigherQuality,
    int MissingCount
);

public record MovieLibraryComparisonDebugHitDTO(
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


public class GetMovieLibraryComparisonDebugEndpoint
    : Endpoint<GetMovieLibraryComparisonDebugEndpointRequest, ResultDTO<MovieLibraryComparisonDebugResponseDTO>>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public GetMovieLibraryComparisonDebugEndpoint(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(ApiRoutes.DebugController + "/movie-library-comparison");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<MovieLibraryComparisonDebugResponseDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetMovieLibraryComparisonDebugEndpointRequest req, CancellationToken ct)
    {
        var compareResult = await _commandExecutor.Send(
            new CompareMoviePlexLibraryCommand(req.RemoteLibraryId, req.OwnedLibraryId),
            ct
        );

        if (compareResult.IsFailed)
        {
            await Send.FluentResult(compareResult, ct);
            return;
        }

        var hits = await _dbContext.PlexMovieComparisons
            .Where(x => x.RemotePlexLibraryId == req.RemoteLibraryId && x.OwnedPlexLibraryId == req.OwnedLibraryId)
            .Join(
                _dbContext.PlexMovies,
                comparison => comparison.RemotePlexMediaId,
                remote => remote.Id,
                (comparison, remote) => new { comparison, remote }
            )
            .Join(
                _dbContext.PlexMovies,
                x => x.comparison.OwnedPlexMediaId,
                owned => owned.Id,
                (x, owned) => new
                {
                    x.comparison.HitState,
                    x.comparison.MatchType,
                    x.comparison.ComparedAt,
                    RemoteMediaId = x.remote.Id,
                    RemoteTitle = x.remote.Title,
                    RemoteYear = x.remote.Year,
                    RemoteQuality = x.comparison.RemoteQuality,
                    OwnedMediaId = owned.Id,
                    OwnedTitle = owned.Title,
                    OwnedYear = owned.Year,
                    OwnedQuality = x.comparison.OwnedQuality,
                }
            )
            .OrderBy(x => x.RemoteTitle)
            .ThenBy(x => x.RemoteYear)
            .Select(x => new
            {
                x.HitState,
                Hit = new MovieLibraryComparisonDebugHitDTO(
                    x.RemoteMediaId,
                    x.RemoteTitle,
                    x.RemoteYear,
                    x.RemoteQuality,
                    x.OwnedMediaId,
                    x.OwnedTitle,
                    x.OwnedYear,
                    x.OwnedQuality,
                    x.MatchType,
                    x.ComparedAt
                ),
            })
            .ToListAsync(ct);

        var missingCount = await _dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == req.RemoteLibraryId)
            .CountAsync(remote =>
                !_dbContext.PlexMovieComparisons.Any(comparison =>
                    comparison.RemotePlexLibraryId == req.RemoteLibraryId
                    && comparison.OwnedPlexLibraryId == req.OwnedLibraryId
                    && comparison.RemotePlexMediaId == remote.Id
                ),
                ct
            );

        var response = new MovieLibraryComparisonDebugResponseDTO(
            req.RemoteLibraryId,
            req.OwnedLibraryId,
            hits.Where(x => x.HitState == PlexMediaComparisonHitState.Matched).Select(x => x.Hit).ToList(),
            hits.Where(x => x.HitState == PlexMediaComparisonHitState.HigherQuality).Select(x => x.Hit).ToList(),
            missingCount
        );

        await Send.FluentResult(Result.Ok(response), ct);
    }
}
