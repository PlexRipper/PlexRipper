using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

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

public class CreateAccountHandler : BaseHandler, IRequestHandler<CreatePlexAccountCommand, Result<int>>
{
    public CreateAccountHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<int>> Handle(CreatePlexAccountCommand command, CancellationToken cancellationToken)
    {
        _log.DebugLine("Creating a new Account in DB");

        await _dbContext.PlexAccounts.AddAsync(command.PlexAccount, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(command.PlexAccount).GetDatabaseValuesAsync(cancellationToken);

        return Result.Ok(command.PlexAccount.Id).Add201CreatedRequestSuccess("PlexAccount created successfully.");
    }
}