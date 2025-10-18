using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

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

public class GetMediaDetailByIdEndpoint : BaseEndpoint<GetMediaDetailByIdEndpointRequest, PlexMediaDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/detail/{PlexMediaId}";

    public GetMediaDetailByIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetMediaDetailByIdEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

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
            var plexMovie = await _dbContext.PlexMovies.GetAsync(req.PlexMediaId, ct);
            if (plexMovie is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(req.Type.GetType), req.PlexMediaId), ct);
                return;
            }

            await SetNestedMovieProperties(plexMovie, ct);

            var result = await _dbContext.GetPlexServerTokenAsync(plexMovie.PlexServerId, ct);

            await SendFluentResult(Result.Ok(plexMovie), x => x.ToDTO(result.ValueOrDefault), ct);
        }
        else if (req.Type == PlexMediaType.TvShow)
        {
            var plexTvShowResult = await GetPlexTvShow(req.PlexMediaId, ct);
            if (plexTvShowResult.IsFailed)
            {
                await SendFluentResult(plexTvShowResult, ct);
                return;
            }

            var plexServerId = plexTvShowResult.Value.PlexServerId;
            var result = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);

            await SendFluentResult(plexTvShowResult, x => x.ToDTO(result.ValueOrDefault), ct);
        }
        else
            await SendFluentResult(ResultExtensions.Create400BadRequestResult($"Type {req.Type} is not allowed"), ct);
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
