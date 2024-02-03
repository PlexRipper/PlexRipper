using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class DeletePlexAccountValidator : AbstractValidator<DeletePlexAccountCommand>
{
    public DeletePlexAccountValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeletePlexAccountHandler : IRequestHandler<DeletePlexAccountCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public DeletePlexAccountHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeletePlexAccountCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = await _dbContext.PlexAccounts.AsTracking().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (plexAccount == null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), command.Id);

        _dbContext.PlexAccounts.Remove(plexAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _log.Debug("Deleted {PlexAccount} with Id: {CommandId} from the database", nameof(PlexAccount), command.Id);

        return Result.Ok();
    }
}