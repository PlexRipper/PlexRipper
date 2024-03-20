using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Creates an <see cref="PlexAccount"/> in the Database and performs an QueueInspectPlexServerByPlexAccountIdJob().
/// </summary>
/// <returns>Returns the created <see cref="PlexAccount"/> as a DTO</returns>
public class CreatePlexAccountEndpointRequest
{
    [FromBody]
    public PlexAccountDTO PlexAccount { get; init; }
}

public class CreatePlexAccountEndpointRequestValidator : Validator<CreatePlexAccountEndpointRequest>
{
    public CreatePlexAccountEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccount.Id).Equal(0).WithMessage("The Id should be 0 when creating a new PlexAccount");
        RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
    }
}

public class CreatePlexAccountEndpoint : BaseCustomEndpoint<CreatePlexAccountEndpointRequest, PlexAccountDTO>
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
        Description(x => x
            .Produces(StatusCodes.Status201Created, typeof(ResultDTO<PlexAccountDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CreatePlexAccountEndpointRequest req, CancellationToken ct)
    {
        var plexAccount = req.PlexAccount.ToModel();

        _log.Debug("Creating account with username {UserName}", plexAccount.Username);

        var isAvailable = await _dbContext.IsUsernameAvailable(plexAccount.Username, ct);

        if (!isAvailable)
        {
            var msg =
                $"Account with username {plexAccount.Username} cannot be created due to an account with the same username already existing";
            await SendFluentResult(Result.Fail(msg).LogError(), ct);
            return;
        }

        _log.DebugLine("Creating a new Account in DB");

        // Generate plexAccount clientId
        plexAccount.ClientId = StringExtensions.RandomString(24, true, true);

        await _dbContext.PlexAccounts.AddAsync(plexAccount, ct);
        await _dbContext.SaveChangesAsync(ct);
        await _dbContext.Entry(plexAccount).GetDatabaseValuesAsync(ct);

        var inspectResult = await _mediator.Send(new InspectAllPlexServersByAccountIdCommand(plexAccount.Id), ct);
        if (inspectResult.IsFailed)
        {
            _log.Error("Failed to queue inspect server job for PlexAccount with id {PlexAccountId}", plexAccount.Id);
            await SendFluentResult(inspectResult, ct);
            return;
        }

        var plexAccountDTO = await _dbContext.PlexAccounts
            .IncludeServerAccess()
            .IncludeLibraryAccess()
            .GetAsync(plexAccount.Id, ct);

        var result = Result.Ok(plexAccountDTO).Add201CreatedRequestSuccess("PlexAccount created successfully.");

        await SendFluentResult(result, model => model.ToDTO(), ct);
    }
}