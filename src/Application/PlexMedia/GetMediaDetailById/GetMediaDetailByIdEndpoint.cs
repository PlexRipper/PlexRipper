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
        RuleFor(x => x.Type).Must(x => x != PlexMediaType.None && x != PlexMediaType.Unknown);
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
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetMediaDetailByIdEndpointRequest req, CancellationToken ct)
    {
        PlexMediaDTO plexMediaDTO = null;
        switch (req.Type)
        {
            case PlexMediaType.Movie:
            {
                var plexMovie = await _dbContext.PlexMovies.GetAsync(req.PlexMediaId, ct);
                if (plexMovie is null)
                    break;

                plexMediaDTO = plexMovie.ToDTO();
                break;
            }
            case PlexMediaType.TvShow:
            {
                var plexTvShow = await _dbContext.PlexTvShows.IncludeEpisodes().GetAsync(req.PlexMediaId, ct);
                if (plexTvShow is null)
                    break;

                plexTvShow.Seasons = plexTvShow.Seasons.OrderByNatural(x => x.Title).ToList();
                plexMediaDTO = plexTvShow.ToDTO();
                break;
            }
            case PlexMediaType.Season:
                var plexSeason = await _dbContext.PlexTvShowSeason.IncludeEpisodes().GetAsync(req.PlexMediaId, ct);
                if (plexSeason is null)
                    break;

                plexMediaDTO = plexSeason.ToDTO();
                break;
            case PlexMediaType.Episode:
                var plexEpisode = await _dbContext.PlexTvShowEpisodes.GetAsync(req.PlexMediaId, ct);
                if (plexEpisode is null)
                    break;

                plexMediaDTO = plexEpisode.ToDTO();
                break;
            default:
                await SendFluentResult(ResultExtensions.Create400BadRequestResult($"Type {req.Type} is not allowed"), ct);
                return;
        }

        if (plexMediaDTO is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(req.Type.GetType), req.PlexMediaId), ct);
            return;
        }

        // Do continue, even if the connection is invalid, worst case is that the thumbnail will not work
        var plexServerConnection = await _dbContext.GetValidPlexServerConnection(plexMediaDTO.PlexServerId, ct);
        if (plexServerConnection.IsFailed)
            plexServerConnection.ToResult().LogError();

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexMediaDTO.PlexServerId, ct);
        if (plexServerToken.IsFailed)
            plexServerToken.ToResult().LogError();

        SetNestedProperties(plexMediaDTO, plexServerConnection.Value.Url, plexServerToken.Value);

        await SendFluentResult(Result.Ok(plexMediaDTO), x => x, ct);
    }

    private void SetNestedProperties(PlexMediaDTO plexMediaDto, string connectionUrl, string plexServerToken)
    {
        plexMediaDto.SetFullThumbnailUrl(connectionUrl, plexServerToken);
        if (!plexMediaDto.Children.Any())
            return;

        foreach (var child in plexMediaDto.Children)
            SetNestedProperties(child, connectionUrl, plexServerToken);
    }
}