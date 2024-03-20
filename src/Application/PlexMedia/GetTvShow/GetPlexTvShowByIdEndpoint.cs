using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Get the <see cref="PlexTvShow"/> without the <see cref="PlexTvShowSeason"/> and <see cref="PlexTvShowEpisode"/>
/// </summary>
/// <param name="PlexTvShowId">The id of the <see cref="PlexTvShow"/>.</param>
public record GetPlexTvShowByIdEndpointRequest(int PlexTvShowId);

public class GetPlexTvShowByIdEndpointRequestValidator : Validator<GetPlexTvShowByIdEndpointRequest>
{
    public GetPlexTvShowByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexTvShowId).GreaterThan(0);
    }
}

public class GetPlexTvShowByIdEndpoint : BaseCustomEndpoint<GetPlexTvShowByIdEndpointRequest, ResultDTO<PlexMediaDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/tvshow/detail/{PlexTvShowId}";

    public GetPlexTvShowByIdEndpoint(IPlexRipperDbContext dbContext)
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

    public override async Task HandleAsync(GetPlexTvShowByIdEndpointRequest req, CancellationToken ct)
    {
        var plexTvShow = await _dbContext.PlexTvShows.GetAsync(req.PlexTvShowId, ct);

        if (plexTvShow is null)
        {
            await SendResult(ResultExtensions.EntityNotFound(nameof(PlexTvShow), req.PlexTvShowId), ct);
            return;
        }

        await SendResult(Result.Ok(plexTvShow.ToDTO()), ct);
    }
}