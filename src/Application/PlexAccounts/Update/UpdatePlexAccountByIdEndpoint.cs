using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class UpdatePlexAccountByIdEndpointRequest
{
    [FromBody]
    public PlexAccountDTO? PlexAccountDTO { get; init; }
}

public class UpdatePlexAccountByIdEndpointRequestValidator : Validator<UpdatePlexAccountByIdEndpointRequest>
{
    public UpdatePlexAccountByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountDTO).NotNull();
        RuleFor(x => x.PlexAccountDTO!.Id).GreaterThan(0);
        RuleFor(x => x.PlexAccountDTO!.DisplayName).NotEmpty();
        RuleFor(x => x.PlexAccountDTO!.Username)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccountDTO!.CustomAuthenticationToken));

        RuleFor(x => x.PlexAccountDTO!.Password)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccountDTO!.CustomAuthenticationToken));
    }
}

public class UpdatePlexAccountByIdEndpoint : BaseEndpoint<UpdatePlexAccountByIdEndpointRequest, PlexAccountDTO>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController;

    public UpdatePlexAccountByIdEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(UpdatePlexAccountByIdEndpointRequest req, CancellationToken ct)
    {
        var plexAccountDTO = req.PlexAccountDTO!;
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
