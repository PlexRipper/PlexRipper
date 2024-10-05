using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class UpdatePlexAccountByIdEndpointRequest
{
    [FromBody]
    public PlexAccountDTO PlexAccountDTO { get; init; }
}

public class UpdatePlexAccountByIdEndpointRequestValidator : Validator<UpdatePlexAccountByIdEndpointRequest>
{
    public UpdatePlexAccountByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountDTO).NotNull();
        RuleFor(x => x.PlexAccountDTO.Id).GreaterThan(0);
        RuleFor(x => x.PlexAccountDTO.DisplayName).NotEmpty();
        RuleFor(x => x.PlexAccountDTO.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.Password).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.IsEnabled).NotNull();
        RuleFor(x => x.PlexAccountDTO.IsMain).NotNull();
    }
}

public class UpdatePlexAccountByIdEndpoint : BaseEndpoint<UpdatePlexAccountByIdEndpointRequest, PlexAccountDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController;

    public UpdatePlexAccountByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(UpdatePlexAccountByIdEndpointRequest req, CancellationToken ct)
    {
        var plexAccountDTO = req.PlexAccountDTO;
        var accountInDb = await _dbContext
            .PlexAccounts.AsTracking()
            .Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .GetAsync(plexAccountDTO.Id, ct);

        if (accountInDb == null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountDTO.Id), ct);
            return;
        }

        var updatedPlexAccount = plexAccountDTO.ToModel();
        _dbContext.Entry(accountInDb).CurrentValues.SetValues(updatedPlexAccount);
        await _dbContext.SaveChangesAsync(ct);

        await SendFluentResult(Result.Ok(accountInDb), x => x.ToDTO(), ct);
    }
}
