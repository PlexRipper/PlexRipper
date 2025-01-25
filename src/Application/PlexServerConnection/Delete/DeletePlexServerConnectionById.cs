using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record DeletePlexServerConnectionByIdRequest(int PlexServerConnectionId);

public class DeletePlexServerConnectionByIdRequestValidator : Validator<DeletePlexServerConnectionByIdRequest>
{
    public DeletePlexServerConnectionByIdRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class DeletePlexServerConnectionById : BaseEndpoint<DeletePlexServerConnectionByIdRequest>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/{PlexServerConnectionId}";

    public DeletePlexServerConnectionById(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeletePlexServerConnectionByIdRequest req, CancellationToken ct)
    {
        var deleteCount = await _dbContext
            .PlexServerConnections.Where(x => x.Id == req.PlexServerConnectionId)
            .ExecuteDeleteAsync(ct);

        if (deleteCount == 0)
        {
            await SendFluentResult(
                ResultExtensions.EntityNotFound(nameof(PlexServerConnection), req.PlexServerConnectionId),
                ct
            );
            return;
        }

        await SendFluentResult(Result.Ok(), ct);
    }
}
