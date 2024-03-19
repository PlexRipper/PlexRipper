using System.ComponentModel;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class GetPlexLibraryMediaEndpointRequest
{
    public int PlexLibraryId { get; init; }

    [QueryParam]
    [DefaultValue(0)]
    public int Page { get; init; } = 0;

    [QueryParam]
    [DefaultValue(0)]
    public int Size { get; init; } = 0;
}

public class GetPlexLibraryMediaEndpointRequestValidator : Validator<GetPlexLibraryMediaEndpointRequest>
{
    public GetPlexLibraryMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class GetPlexLibraryMediaEndpoint : BaseCustomEndpoint<GetPlexLibraryMediaEndpointRequest, ResultDTO<List<PlexMediaSlimDTO>>>
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
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexMediaSlimDTO>>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetPlexLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var plexLibrary = await _dbContext.PlexLibraries
            .AsNoTracking()
            .IncludePlexServer()
            .GetAsync(req.PlexLibraryId, ct);
        if (plexLibrary is null)
        {
            await SendResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        // When 0, just take everything
        var take = req.Size <= 0 ? -1 : req.Size;
        var skip = req.Page * req.Size;

        List<PlexMediaSlimDTO> entities;
        switch (plexLibrary.Type)
        {
            case PlexMediaType.Movie:
            {
                entities = await _dbContext.PlexMovies.AsNoTracking()
                    .Where(x => x.PlexLibraryId == req.PlexLibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectToDTO()
                    .ToListAsync(ct);
                break;
            }
            case PlexMediaType.TvShow:
            {
                entities = await _dbContext.PlexTvShows.AsNoTracking()
                    .Where(x => x.PlexLibraryId == req.PlexLibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectToDTO()
                    .ToListAsync(ct);
                break;
            }
            default:
                await SendResult(Result.Fail($"Type {plexLibrary.Type} is not supported for retrieving the PlexMedia data by library id"), ct);
                return;
        }

        var plexServerId = plexLibrary.PlexServerId;

        var plexServerConnection = await _dbContext.GetValidPlexServerConnection(plexServerId, ct);
        if (plexServerConnection.IsFailed)
        {
            await SendResult(plexServerConnection.ToResult(), ct);
            return;
        }

        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexServerId, ct);
        if (plexServerToken.IsFailed)
        {
            await SendResult(plexServerToken.ToResult(), ct);
            return;
        }

        foreach (var mediaSlim in entities)
            mediaSlim.ThumbUrl = GetThumbnailUrl(plexServerConnection.Value.Url, mediaSlim.ThumbUrl, plexServerToken.Value);

        await SendResult(Result.Ok(entities.SetIndex()), ct);
    }

    private string GetThumbnailUrl(string connectionUrl, string thumbPath, string plexServerToken)
    {
        var uri = new Uri(connectionUrl + thumbPath);
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&X-Plex-Token={plexServerToken}";
    }
}