using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

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

public class GetPlexTvShowWithEpisodesByIdEndpoint : BaseCustomEndpoint<GetPlexTvShowWithEpisodesByIdEndpointRequest, PlexMediaSlimDTO>
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

        await SendFluentResult(Result.Ok(plexTvShow), x => x.ToSlimDTO(), ct);
    }
}