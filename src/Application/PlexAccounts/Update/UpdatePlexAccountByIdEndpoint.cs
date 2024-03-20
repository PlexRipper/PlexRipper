using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class UpdatePlexAccountByIdEndpointRequest
{
    // TODO Id is not needed as the DTO already should have it
    public int PlexAccountId { get; init; }

    [FromBody]
    public PlexAccountDTO PlexAccountDTO { get; init; }

    [QueryParam]
    public bool Inspect { get; init; }
}

public class UpdatePlexAccountByIdEndpointRequestValidator : Validator<UpdatePlexAccountByIdEndpointRequest>
{
    public UpdatePlexAccountByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
        RuleFor(x => x.PlexAccountDTO).NotNull();
        RuleFor(x => x.PlexAccountDTO.Id).GreaterThan(0);
        RuleFor(x => x.PlexAccountDTO.DisplayName).NotEmpty();
        RuleFor(x => x.PlexAccountDTO.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.Password).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.IsValidated).NotNull();
        RuleFor(x => x.PlexAccountDTO.PlexId).GreaterThan(0);
        RuleFor(x => x.PlexAccountDTO.Uuid).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.Title).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccountDTO.AuthenticationToken).NotEmpty().MinimumLength(10);
    }
}

public class UpdatePlexAccountByIdEndpoint : BaseEndpoint<UpdatePlexAccountByIdEndpointRequest, PlexAccountDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/{PlexAccountId}";

    public UpdatePlexAccountByIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(UpdatePlexAccountByIdEndpointRequest req, CancellationToken ct)
    {
        var plexAccountDTO = req.PlexAccountDTO;
        var accountInDb = await _dbContext.PlexAccounts
            .AsTracking()
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .GetAsync(plexAccountDTO.Id, ct);

        if (accountInDb == null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountDTO.Id), ct);
            return;
        }

        var updatedPlexAccount = plexAccountDTO.ToModel();
        _dbContext.Entry(accountInDb).CurrentValues.SetValues(updatedPlexAccount);
        await _dbContext.SaveChangesAsync(ct);

        // Re-validate if the credentials changed
        if (req.Inspect || accountInDb.Username != plexAccountDTO.Username || accountInDb.Password != plexAccountDTO.Password)
        {
            await SendFluentResult(
                Result.Fail("Account revalidation is not implemented yet when account is updated with a different username or password").LogError(), ct);
            return;
        }

        await SendFluentResult(Result.Ok(accountInDb), x => x.ToDTO(), ct);
    }
}