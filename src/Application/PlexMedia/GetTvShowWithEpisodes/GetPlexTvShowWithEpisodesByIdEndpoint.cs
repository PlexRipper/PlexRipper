using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using PlexRipper.Domain.PlexMediaExtensions;

namespace PlexRipper.Application;

/// <summary>
/// Get the <see cref="PlexTvShow"/> with the <see cref="PlexTvShowSeason"/> and <see cref="PlexTvShowEpisode"/> in a minimal format.
/// </summary>
/// <param name="PlexTvShowId">The id of the <see cref="PlexTvShow"/></param>
public record GetPlexTvShowWithEpisodesByIdEndpointRequest(int PlexTvShowId);

public class GetPlexTvShowWithEpisodesByIdEndpointRequestValidator : Validator<GetPlexTvShowWithEpisodesByIdEndpointRequest>
{
    public GetPlexTvShowWithEpisodesByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexTvShowId).GreaterThan(0);
    }
}

public class GetPlexTvShowWithEpisodesByIdEndpoint : BaseEndpoint<GetPlexTvShowWithEpisodesByIdEndpointRequest, PlexMediaSlimDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/tvshow/{PlexTvShowId}";

    public GetPlexTvShowWithEpisodesByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaSlimDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetPlexTvShowWithEpisodesByIdEndpointRequest req, CancellationToken ct)
    {
        var plexTvShow = await _dbContext.PlexTvShows.IncludeEpisodes().GetAsync(req.PlexTvShowId, ct);

        if (plexTvShow is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexTvShow), req.PlexTvShowId), ct);
            return;
        }

        plexTvShow.Seasons = plexTvShow.Seasons.OrderByNatural(x => x.Title).ToList();

        // Do continue, even if the connection is invalid, worst case is that the thumbnail will not work
        var plexServerConnection = await _dbContext.GetValidPlexServerConnection(plexTvShow.PlexServerId, ct);
        if (plexServerConnection.IsFailed)
            plexServerConnection.ToResult().LogError();

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexTvShow.PlexServerId, ct);
        if (plexServerToken.IsFailed)
            plexServerToken.ToResult().LogError();

        plexTvShow.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
        foreach (var season in plexTvShow.Seasons)
        {
            season.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
            season.Episodes.ForEach(x => x.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value));
        }

        await SendFluentResult(Result.Ok(plexTvShow), x => x.ToSlimDTO(), ct);
    }
}