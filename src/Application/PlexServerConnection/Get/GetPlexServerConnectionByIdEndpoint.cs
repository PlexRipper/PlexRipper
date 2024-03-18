using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetPlexServerConnectionByIdEndpointRequest(int PlexServerConnectionId);

public class GetPlexServerConnectionByIdEndpointRequestValidator : Validator<GetPlexServerConnectionByIdEndpointRequest>
{
    public GetPlexServerConnectionByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerConnectionId).GreaterThan(0);
    }
}

public class GetPlexServerConnectionByIdEndpoint : BaseCustomEndpoint<GetPlexServerConnectionByIdEndpointRequest, ResultDTO<PlexServerConnectionDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/{PlexServerConnectionId}";

    public GetPlexServerConnectionByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerConnectionDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetPlexServerConnectionByIdEndpointRequest req, CancellationToken ct)
    {
        var plexServerConnection = await _dbContext
            .PlexServerConnections
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .FirstOrDefaultAsync(x => x.Id == req.PlexServerConnectionId, ct);

        if (plexServerConnection == null)
        {
            await SendResult(
                ResultExtensions.EntityNotFound(nameof(PlexServerConnection), req.PlexServerConnectionId), ct);
            return;
        }

        await SendResult(Result.Ok(plexServerConnection.ToDTO()), ct);
    }
}