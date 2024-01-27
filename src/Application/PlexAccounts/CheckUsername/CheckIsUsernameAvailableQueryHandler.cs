using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class CheckUsernameTaskValidator : AbstractValidator<CheckIsUsernameAvailableQuery>
{
    public CheckUsernameTaskValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Username).MinimumLength(5);
    }
}

public class CheckIsUsernameAvailableQueryHandler : IRequestHandler<CheckIsUsernameAvailableQuery, Result<bool>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public CheckIsUsernameAvailableQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(CheckIsUsernameAvailableQuery request, CancellationToken cancellationToken)
    {
        var plexAccount = await _dbContext.PlexAccounts.FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);
        return plexAccount is null ? AvailableAccount(request) : UnavailableAccount(request);
    }

    private Result<bool> UnavailableAccount(CheckIsUsernameAvailableQuery request)
    {
        _log.Warning("An Account with the username: {UserName} already exists", request.Username);
        return Result.Ok(false);
    }

    private Result<bool> AvailableAccount(CheckIsUsernameAvailableQuery request)
    {
        _log.Debug("The username: {UserName} is available", request.Username);
        return Result.Ok(true);
    }
}