using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetAllPlexServerConnectionsEndpointRequest();

public class GetAllPlexServerConnectionsEndpointRequestValidator : Validator<GetAllPlexServerConnectionsEndpointRequest>
{
    public GetAllPlexServerConnectionsEndpointRequestValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class GetAllPlexServerConnectionsEndpoint : BaseCustomEndpoint<GetAllPlexServerConnectionsEndpointRequest, ResultDTO<List<PlexServerConnectionDTO>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/";

    public GetAllPlexServerConnectionsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerConnectionDTO>>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetAllPlexServerConnectionsEndpointRequest req, CancellationToken ct)
    {
        var plexServerConnections = await _dbContext
            .PlexServerConnections
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .ToListAsync(ct);

        await SendResult(Result.Ok(plexServerConnections.ToDTO()), ct);
    }
}