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
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Size).GreaterThanOrEqualTo(0);
    }
}

public class GetPlexLibraryMediaEndpoint : BaseEndpoint<GetPlexLibraryMediaEndpointRequest, PlexMediaStatisticsDTO>
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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaStatisticsDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.AsNoTracking()
            .Include(x => x.PlexServer)
            .GetAsync(req.PlexLibraryId, ct);
        if (plexLibrary is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        // When 0, just take everything
        var take = req.Size <= 0 ? -1 : req.Size;
        var skip = req.Page * req.Size;

        if (take == 0)
        {
            // Take everything, but stop searching when the media count is reached
            take = plexLibrary.MediaCount;
        }

        var plexServerId = plexLibrary.PlexServerId;

        // Do continue, even if the connection is invalid, worst case is that the thumbnail will not work
        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexServerId, ct);
        if (plexServerConnection.IsFailed)
            plexServerConnection.ToResult().LogError();

        var mediaListResult = await _dbContext.GetMediaByType(
            mediaType: plexLibrary.Type,
            skip: skip,
            take: take,
            plexLibraryId: plexLibrary.Id,
            filterOfflineMedia: false,
            filterOwnedMedia: false,
            ct: ct
        );

        if (mediaListResult.IsFailed)
        {
            await SendFluentResult(mediaListResult, ct);
            return;
        }

        await SendFluentResult(Result.Ok(mediaListResult.Value.ToStatisticsDTO()), ct);
    }
}
