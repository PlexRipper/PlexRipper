using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public class CreatePlexAccountCommandValidator : AbstractValidator<CreatePlexAccountCommand>
{
    public CreatePlexAccountCommandValidator()
    {
        RuleFor(x => x.PlexAccount.Id).Equal(0).WithMessage("The Id should be 0 when creating a new PlexAccount");
        RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.ClientId).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
    }
}

public class CreatePlexAccountHandler : IRequestHandler<CreatePlexAccountCommand, Result<int>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IInspectServerScheduler _inspectServerScheduler;

    public CreatePlexAccountHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator, IInspectServerScheduler inspectServerScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _inspectServerScheduler = inspectServerScheduler;
    }

    public async Task<Result<int>> Handle(CreatePlexAccountCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = command.PlexAccount;

        _log.Debug("Creating account with username {UserName}", command.PlexAccount.Username);

        var result = await _mediator.Send(new IsUsernameAvailableQuery(plexAccount.Username), cancellationToken);

        // Fail on validation errors
        if (result.IsFailed)
            return result.ToResult();

        if (!result.Value)
        {
            var msg =
                $"Account with username {plexAccount.Username} cannot be created due to an account with the same username already existing";
            return result.ToResult().WithError(msg).LogWarning();
        }

        _log.DebugLine("Creating a new Account in DB");

        // Generate plexAccount clientId
        plexAccount.ClientId = StringExtensions.RandomString(24, true, true);

        await _dbContext.PlexAccounts.AddAsync(command.PlexAccount, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(command.PlexAccount).GetDatabaseValuesAsync(cancellationToken);

        var inspectResult = await _mediator.Send(new InspectAllPlexServersByAccountIdCommand(command.PlexAccount.Id), cancellationToken);
        if (result.IsFailed)
        {
            _log.Error("Failed to queue inspect server job for PlexAccount with id {PlexAccountId}", command.PlexAccount.Id);
            return inspectResult;
        }

        return Result.Ok(command.PlexAccount.Id).Add201CreatedRequestSuccess("PlexAccount created successfully.");
    }
}