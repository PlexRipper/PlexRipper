using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Set the default media destination where the media will be stored after the download process is finished.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to update.</param>
/// <param name="FolderPathId">The id of the <see cref="FolderPath"/> to set as the default destination.</param>
public record SetPlexLibraryDefaultDestinationByIdEndpointRequest(int PlexLibraryId, int FolderPathId);

public class SetPlexLibraryDefaultDestinationByIdEndpointRequestValidator
    : Validator<SetPlexLibraryDefaultDestinationByIdEndpointRequest>
{
    public SetPlexLibraryDefaultDestinationByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.FolderPathId).GreaterThan(0);
    }
}

public class SetPlexLibraryDefaultDestinationByIdEndpoint
    : BaseEndpoint<SetPlexLibraryDefaultDestinationByIdEndpointRequest, ResultDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath =>
        ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/default/destination/{FolderPathId}";

    public SetPlexLibraryDefaultDestinationByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(
        SetPlexLibraryDefaultDestinationByIdEndpointRequest req,
        CancellationToken ct
    )
    {
        var plexLibraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == req.PlexLibraryId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.DefaultDestinationId, req.FolderPathId), ct);

        if (plexLibraryDb == 0)
        {
            await SendFluentResult(
                Result.Fail(
                    $"No library found with id {req.PlexLibraryId} that could have its default folder destination updated"
                ),
                ct
            );
        }
        else
            await SendFluentResult(Result.Ok(), ct);
    }
}
