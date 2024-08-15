using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetPlexAccountByIdEndpointRequest(int PlexAccountId);

public class GetPlexAccountByIdEndpointRequestValidator : Validator<GetPlexAccountByIdEndpointRequest>
{
    public GetPlexAccountByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class GetPlexAccountByIdEndpoint : BaseEndpoint<GetPlexAccountByIdEndpointRequest, PlexAccountDTO>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/{PlexAccountId}";

    public GetPlexAccountByIdEndpoint(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexAccountByIdEndpointRequest req, CancellationToken ct)
    {
        var plexAccount = await _dbContext
            .PlexAccounts.Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .GetAsync(req.PlexAccountId, ct);
        if (plexAccount is null)
        {
            await SendFluentResult(
                ResultExtensions.EntityNotFound(nameof(PlexAccount), req.PlexAccountId).LogWarning(),
                ct
            );
            return;
        }

        _log.Debug("Found an {NameOfPlexAccount} with the id: {AccountId}", nameof(PlexAccount), req.PlexAccountId);
        await SendFluentResult(Result.Ok(plexAccount), x => x.ToDTO(), ct);
    }
}
