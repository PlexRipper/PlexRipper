using System.ComponentModel;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.PlexMediaExtensions;

namespace PlexRipper.Application;

public class GetPlexLibraryMediaEndpointRequest
{
    public int PlexLibraryId { get; init; }

    [QueryParam]
    [DefaultValue(0)]
    public int Page { get; init; }

    [QueryParam]
    [DefaultValue(0)]
    public int Size { get; init; }
}

public class GetPlexLibraryMediaEndpointRequestValidator : Validator<GetPlexLibraryMediaEndpointRequest>
{
    public GetPlexLibraryMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class GetPlexLibraryMediaEndpoint : BaseEndpoint<GetPlexLibraryMediaEndpointRequest, List<PlexMediaSlimDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/media";

    public GetPlexLibraryMediaEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexMediaSlimDTO>>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.AsNoTracking()
            .IncludePlexServer()
            .GetAsync(req.PlexLibraryId, ct);
        if (plexLibrary is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        // When 0, just take everything
        var take = req.Size <= 0 ? -1 : req.Size;
        var skip = req.Page * req.Size;

        var plexServerId = plexLibrary.PlexServerId;

        // Do continue, even if the connection is invalid, worst case is that the thumbnail will not work
        var plexServerConnection = await _dbContext.GetValidPlexServerConnection(plexServerId, ct);
        if (plexServerConnection.IsFailed)
            plexServerConnection.ToResult().LogError();

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);
        if (plexServerToken.IsFailed)
            plexServerToken.ToResult().LogError();

        var entities = new List<PlexMediaSlimDTO>();
        switch (plexLibrary.Type)
        {
            case PlexMediaType.Movie:
            {
                var plexMovies = await _dbContext
                    .PlexMovies.AsNoTracking()
                    .Where(x => x.PlexLibraryId == req.PlexLibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(ct);

                foreach (var plexMovie in plexMovies)
                {
                    if (plexMovie.HasThumb)
                        plexMovie.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
                    entities.Add(plexMovie.ToSlimDTO());
                }

                break;
            }
            case PlexMediaType.TvShow:
            {
                var plexTvShow = await _dbContext
                    .PlexTvShows.AsNoTracking()
                    .Where(x => x.PlexLibraryId == req.PlexLibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync(ct);

                foreach (var tvShow in plexTvShow)
                {
                    if (tvShow.HasThumb)
                        tvShow.SetFullThumbnailUrl(plexServerConnection.Value.Url, plexServerToken.Value);
                    entities.Add(tvShow.ToSlimDTO());
                }

                break;
            }
            default:
                await SendFluentResult(
                    Result.Fail(
                        $"Type {plexLibrary.Type} is not supported for retrieving the PlexMedia data by library id"
                    ),
                    ct
                );
                return;
        }

        await SendFluentResult(Result.Ok(entities.SetIndex()), _ => _, ct);
    }
}
