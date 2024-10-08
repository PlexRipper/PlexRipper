using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Creates an <see cref="PlexAccount"/> in the Database and performs an QueueInspectPlexServerByPlexAccountIdJob().
/// </summary>
/// <returns>Returns the created <see cref="PlexAccount"/> as a DTO</returns>
public class CreatePlexAccountEndpointRequest
{
    [FromBody]
    public PlexAccountDTO? PlexAccount { get; init; }
}

public class CreatePlexAccountEndpointRequestValidator : Validator<CreatePlexAccountEndpointRequest>
{
    public CreatePlexAccountEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount!.IsValidated).Equal(true);
        RuleFor(x => x.PlexAccount!.DisplayName).NotEmpty();
        RuleFor(x => x.PlexAccount!.Username).NotEmpty().MinimumLength(5).When(m => !m.PlexAccount!.IsAuthTokenMode);
        RuleFor(x => x.PlexAccount!.Password).NotEmpty().MinimumLength(5).When(m => !m.PlexAccount!.IsAuthTokenMode);
        RuleFor(x => x.PlexAccount!.AuthenticationToken).NotEmpty().When(m => m.PlexAccount!.IsAuthTokenMode);
    }
}

public class CreatePlexAccountEndpoint : BaseEndpoint<CreatePlexAccountEndpointRequest, PlexAccountDTO>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/";

    public CreatePlexAccountEndpoint(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status201Created, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CreatePlexAccountEndpointRequest req, CancellationToken ct)
    {
        var plexAccount = req.PlexAccount!.ToModel();
        plexAccount.Id = 0;

        if (!plexAccount.IsAuthTokenMode)
        {
            // Check if account with the same username already exists
            var isAvailable = await _dbContext.IsUsernameAvailable(plexAccount.Username, ct);
            if (!isAvailable)
            {
                var msg =
                    $"Account with username {plexAccount.Username} cannot be created due to an account with the same username already existing";
                await SendFluentResult(Result.Fail(msg).LogError(), ct);
                return;
            }

            // Check if account with the same UUID already exists
            var uuidResult = await _dbContext
                .PlexAccounts.Where(x => x.Uuid == plexAccount.Uuid)
                .FirstOrDefaultAsync(ct);
            if (uuidResult is not null)
            {
                var badResult = ResultExtensions
                    .Create400BadRequestResult("Account with the same UUID {plexAccount.Uuid} already exists")
                    .LogWarning();
                await SendFluentResult(badResult, ct);
                return;
            }
        }

        _log.Debug("Creating account with username {DisplayName}", plexAccount.DisplayName);

        // Generate plexAccount clientId
        plexAccount.ClientId = StringExtensions.RandomString(24, true, true);

        await _dbContext.PlexAccounts.AddAsync(plexAccount, ct);
        await _dbContext.SaveChangesAsync(ct);
        await _dbContext.Entry(plexAccount).GetDatabaseValuesAsync(ct);

        var plexAccountDTO = await _dbContext
            .PlexAccounts.Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .GetAsync(plexAccount.Id, ct);

        if (plexAccountDTO is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccount.Id), ct);
            return;
        }

        var result = Result.Ok(plexAccountDTO).Add201CreatedRequestSuccess("PlexAccount created successfully.");

        await SendFluentResult(result, model => model.ToDTO(), ct);

        // Return the Ok result and then kick off the inspect job
        var inspectResult = await _mediator.Send(new InspectAllPlexServersByAccountIdCommand(plexAccount.Id), ct);
        if (inspectResult.IsFailed)
            _log.Error("Failed to queue inspect server job for PlexAccount with id {PlexAccountId}", plexAccount.Id);
    }
}