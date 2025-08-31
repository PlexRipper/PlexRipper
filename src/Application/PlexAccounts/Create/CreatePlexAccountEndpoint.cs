using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Creates an <see cref="PlexAccount"/> in the Database and performs an QueueInspectPlexServerByPlexAccountIdJob().
/// </summary>
/// <returns>Returns the created <see cref="PlexAccount"/> as a DTO</returns>
public class CreatePlexAccountEndpointRequest
{
    [FromBody]
    public required PlexAccountDTO? PlexAccount { get; init; }
}

public class CreatePlexAccountEndpointRequestValidator : Validator<CreatePlexAccountEndpointRequest>
{
    public CreatePlexAccountEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount!.DisplayName).NotEmpty();

        RuleFor(x => x.PlexAccount!.Username)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccount!.CustomAuthenticationToken));

        RuleFor(x => x.PlexAccount!.Password)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccount!.CustomAuthenticationToken));
    }
}

public class CreatePlexAccountEndpoint : BaseEndpoint<CreatePlexAccountEndpointRequest, PlexAccountDTO>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/";

    public CreatePlexAccountEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log;
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status201Created, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
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
        if (plexAccount.ClientId == string.Empty)
            plexAccount.ClientId = Guid.NewGuid().ToString();

        await _dbContext.PlexAccounts.AddAsync(plexAccount, ct);
        await _dbContext.SaveChangesAsync(ct);
        await _dbContext.Entry(plexAccount).GetDatabaseValuesAsync(ct);

        var plexAccountDb = await _dbContext
            .PlexAccounts.Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .GetAsync(plexAccount.Id, ct);

        if (plexAccountDb is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccount.Id), ct);
            return;
        }

        var result = Result.Ok(plexAccountDb).Add201CreatedRequestSuccess("PlexAccount created successfully.");

        await SendFluentResult(result, model => model.ToDTO(), ct);

        // Return the Ok result and then kick off the inspecting job
        var inspectResult = await _commandExecutor.Send(
            new InspectAllPlexServersByAccountIdCommand(plexAccount.Id),
            ct
        );
        if (inspectResult.IsFailed)
            _log.Error("Failed to queue inspect server job for PlexAccount with id {PlexAccountId}", plexAccount.Id);
    }
}
