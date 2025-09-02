using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public record GetPlexLibraryMediaEndpointRequest : PlexMediaFilterQueryRequest
{
    public int PlexLibraryId { get; init; }
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
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/media";

    public GetPlexLibraryMediaEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetPlexLibraryMediaEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaStatisticsDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
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

        // Do continue, even if the connection is invalid, the worst case is that the thumbnail will not work
        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexServerId, ct);
        if (plexServerConnection.IsFailed)
            plexServerConnection.ToResult().LogError();

        var mediaListResult = await _dbContext.GetMediaByType(
            new MediaQueryFilter
            {
                MediaType = plexLibrary.Type,
                Skip = skip,
                Take = take,
                PlexLibraryId = plexLibrary.Id,
                FilterOfflineMedia = req.FilterOfflineMedia,
                FilterOwnedMedia = req.FilterOwnedMedia,
                CountryId = req.CountryId,
                ActorId = req.ActorId,
                GenreId = req.GenreId,
            },
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
