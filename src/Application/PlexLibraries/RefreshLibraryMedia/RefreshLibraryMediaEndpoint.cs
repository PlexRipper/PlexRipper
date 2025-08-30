using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record RefreshLibraryMediaEndpointRequest(int PlexLibraryId);

public class RefreshLibraryMediaEndpointRequestValidator : Validator<RefreshLibraryMediaEndpointRequest>
{
    public RefreshLibraryMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaEndpoint : BaseEndpoint<RefreshLibraryMediaEndpointRequest, PlexLibraryDTO>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/refresh/{PlexLibraryId}";

    public RefreshLibraryMediaEndpoint(IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
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

    public override async Task HandleAsync(RefreshLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new RefreshLibraryMediaCommand(req.PlexLibraryId, _ => { }), ct);
        if (result.IsFailed)
        {
            await SendFluentResult(result, ct);
            return;
        }

        var plexLibrary = await _dbContext.PlexLibraries.GetAsync(req.PlexLibraryId, cancellationToken: ct);

        if (plexLibrary is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        await SendFluentResult(Result.Ok(plexLibrary), x => x.ToDTO(), ct);
    }
}
