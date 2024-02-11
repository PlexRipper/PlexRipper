using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

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

public class UpdatePlexAccountHandler : IRequestHandler<UpdatePlexAccountCommand, Result<PlexAccount>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public UpdatePlexAccountHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<PlexAccount>> Handle(UpdatePlexAccountCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = command.PlexAccount;
        var accountInDb = await _dbContext.PlexAccounts
            .Include(x => x.PlexAccountServers)
            .ThenInclude(x => x.PlexServer)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == plexAccount.Id, cancellationToken);

        if (accountInDb == null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccount.Id);

        _dbContext.Entry(accountInDb).CurrentValues.SetValues(plexAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Re-validate if the credentials changed
        if (command.inspectServers || accountInDb.Username != plexAccount.Username || accountInDb.Password != plexAccount.Password)
            throw new NotImplementedException("Account revalidation is not implemented yet when account is updated with a different username or password");

        return Result.Ok(accountInDb);
    }
}