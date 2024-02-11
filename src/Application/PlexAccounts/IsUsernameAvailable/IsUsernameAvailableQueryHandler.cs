using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class IsUsernameAvailableQueryValidator : AbstractValidator<IsUsernameAvailableQuery>
{
    public IsUsernameAvailableQueryValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Username).MinimumLength(5);
    }
}

public class IsUsernameAvailableQueryHandler : IRequestHandler<IsUsernameAvailableQuery, Result<bool>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public IsUsernameAvailableQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(IsUsernameAvailableQuery request, CancellationToken cancellationToken)
    {
        var plexAccount = await _dbContext.PlexAccounts.FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        return plexAccount is null ? AvailableAccount(request) : UnavailableAccount(request);
    }

    private Result<bool> UnavailableAccount(IsUsernameAvailableQuery request)
    {
        _log.Warning("An Account with the username: {UserName} already exists", request.Username);
        return Result.Ok(false);
    }

    private Result<bool> AvailableAccount(IsUsernameAvailableQuery request)
    {
        _log.Debug("The username: {UserName} is available", request.Username);
        return Result.Ok(true);
    }
}