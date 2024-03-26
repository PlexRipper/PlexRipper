using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record GetThumbnailImageEndpointRequest(int MediaId, PlexMediaType MediaType, int Width = 0, int Height = 0);

public class GetThumbnailImageEndpointRequestValidator : Validator<GetThumbnailImageEndpointRequest>
{
    public GetThumbnailImageEndpointRequestValidator()
    {
        RuleFor(x => x.MediaId).GreaterThan(0);
        RuleFor(x => x.MediaType).Must(x => x is PlexMediaType.Movie or PlexMediaType.TvShow);
    }
}

public class GetThumbnailImageEndpoint : BaseEndpoint<GetThumbnailImageEndpointRequest, ResultDTO<FileContentResult>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexServiceApi;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/thumb";

    public GetThumbnailImageEndpoint(IPlexRipperDbContext dbContext, IPlexApiService plexServiceApi)
    {
        _dbContext = dbContext;
        _plexServiceApi = plexServiceApi;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetThumbnailImageEndpointRequest req, CancellationToken ct)
    {
        PlexMediaSlimDTO plexMedia = null;
        PlexServer plexServer = null;
        switch (req.MediaType)
        {
            case PlexMediaType.Movie:
            {
                var plexMovie = await _dbContext.PlexMovies.AsQueryable()
                    .Include(x => x.PlexServer)
                    .GetAsync(req.MediaId, ct);

                if (plexMovie is null)
                {
                    await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexMovie), req.MediaId).LogError(), ct);
                    return;
                }

                plexMedia = plexMovie.ToDTO();
                plexServer = plexMovie.PlexServer;
                break;
            }
            case PlexMediaType.TvShow:
            {
                var tvShow = await _dbContext.PlexTvShows.AsQueryable()
                    .Include(x => x.PlexServer)
                    .GetAsync(req.MediaId, ct);

                if (tvShow is null)
                {
                    await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexTvShow), req.MediaId).LogError(), ct);
                    return;
                }

                plexMedia = tvShow.ToDTO();
                plexServer = tvShow.PlexServer;
                break;
            }
        }

        if (!plexMedia.HasThumb)
        {
            await SendFluentResult(Result.Fail($"{plexMedia.Type}: {plexMedia.Title} has no thumbnail."), ct);
            return;
        }

        var imageResult = await _plexServiceApi.GetPlexMediaImageAsync(plexServer!, plexMedia.FullThumbUrl, req.Width, req.Height, ct);
        if (imageResult.IsFailed)
            await SendFluentResult(imageResult.ToResult(), ct);

        await SendBytesAsync(imageResult.Value, contentType: "image/jpeg", cancellation: ct);
    }
}