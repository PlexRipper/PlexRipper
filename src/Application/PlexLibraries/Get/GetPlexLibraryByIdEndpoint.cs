using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Returns the PlexLibrary by the Id, will refresh if the library has no media assigned.
/// Note: this will not include the media.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <returns>Valid result if found.</returns>
public record GetPlexLibraryByIdEndpointRequest(int PlexLibraryId);

public class GetPlexLibraryByIdEndpointRequestValidator : Validator<GetPlexLibraryByIdEndpointRequest>
{
    public GetPlexLibraryByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class GetPlexLibraryByIdEndpoint : BaseEndpoint<GetPlexLibraryByIdEndpointRequest, PlexLibraryDTO>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/{PlexLibraryId}";

    public GetPlexLibraryByIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexLibraryByIdEndpointRequest req, CancellationToken ct)
    {
        var plexLibrary = await _dbContext.PlexLibraries.GetAsync(req.PlexLibraryId, ct);
        if (plexLibrary is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(plexLibrary), req.PlexLibraryId), ct);
            return;
        }

        await SendFluentResult(Result.Ok(plexLibrary), x => x.ToDTO(), ct);
    }
}
