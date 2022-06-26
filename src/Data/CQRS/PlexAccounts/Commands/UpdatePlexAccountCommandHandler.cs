using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class UpdatePlexAccountValidator : AbstractValidator<UpdatePlexAccountCommand>
{
    public UpdatePlexAccountValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount.Id).GreaterThan(0);
        RuleFor(x => x.PlexAccount.DisplayName).NotEmpty();
        RuleFor(x => x.PlexAccount.Username).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.Password).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.IsValidated).NotNull();
        RuleFor(x => x.PlexAccount.PlexId).GreaterThan(0);
        RuleFor(x => x.PlexAccount.Uuid).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.Title).NotEmpty().MinimumLength(5);
        RuleFor(x => x.PlexAccount.AuthenticationToken).NotEmpty().MinimumLength(10);
    }
}

public class UpdatePlexAccountHandler : BaseHandler, IRequestHandler<UpdatePlexAccountCommand, Result>
{
    public UpdatePlexAccountHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(UpdatePlexAccountCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = command.PlexAccount;
        var accountInDb = await _dbContext.PlexAccounts
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .AsTracking().FirstOrDefaultAsync(x => x.Id == plexAccount.Id, cancellationToken);

        if (accountInDb == null)
        {
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccount.Id);
        }

        _dbContext.Entry(accountInDb).CurrentValues.SetValues(plexAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}