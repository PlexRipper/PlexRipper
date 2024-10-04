using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Gets the <see cref="PlexMediaDTO"/> with all children
/// </summary>
public class GetMediaDetailByIdEndpointRequest
{
    /// <summary>
    /// Gets the <see cref="PlexMediaDTO"/> with all children
    /// </summary>
    /// <param name="PlexMediaId">The id of the <see cref="PlexMedia"/>.</param>
    /// <param name="Type"> The <see cref="PlexMediaType">Type</see> of the PlexMedia.</param>
    public GetMediaDetailByIdEndpointRequest(int PlexMediaId, PlexMediaType Type)
    {
        this.PlexMediaId = PlexMediaId;
        this.Type = Type;
    }

    /// <summary>The id of the <see cref="PlexMedia"/>.</summary>
    public int PlexMediaId { get; init; }

    /// <summary> The <see cref="PlexMediaType">Type</see> of the PlexMedia.</summary>
    [QueryParam]
    public PlexMediaType Type { get; init; }
}

public class GetMediaDetailByIdEndpointRequestValidator : Validator<GetMediaDetailByIdEndpointRequest>
{
    public GetMediaDetailByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexMediaId).GreaterThan(0);
        RuleFor(x => x.Type).Must(x => x == PlexMediaType.Movie || x == PlexMediaType.TvShow);
    }
}

public class GetMediaDetailByIdEndpoint : BaseEndpoint<GetMediaDetailByIdEndpointRequest, PlexMediaDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/detail/{PlexMediaId}";

    public GetMediaDetailByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetMediaDetailByIdEndpointRequest req, CancellationToken ct)
    {
        if (req.Type == PlexMediaType.Movie)
        {
            var plexMovie = await _dbContext.PlexMovies.GetAsync(req.PlexMediaId, ct);
            if (plexMovie is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(req.Type.GetType), req.PlexMediaId), ct);
                return;
            }

            await SetNestedMovieProperties(plexMovie, ct);

            await SendFluentResult(Result.Ok(plexMovie), x => x.ToDTO(), ct);
        }
        else if (req.Type == PlexMediaType.TvShow)
        {
            var plexTvShow = await GetPlexTvShow(req.PlexMediaId, ct);
            await SendFluentResult(plexTvShow, x => x.ToDTO(), ct);
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

        plexTvShow.Seasons.OrderByNatural(x => x.SortTitle);

        foreach (var season in plexTvShow.Seasons)
            season.Episodes = _dbContext
                .PlexTvShowEpisodes.Where(x => x.TvShowSeasonId == season.Id)
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
            return;
        }

        plexMovie.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
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
            return;
        }

        plexTvShow.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
        foreach (var plexTvShowSeason in plexTvShow.Seasons)
        {
            plexTvShowSeason.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
            foreach (var plexTvShowEpisode in plexTvShowSeason.Episodes)
                plexTvShowEpisode.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
        }
    }
}