using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetPlexAccountsWithAccessByPlexServerIdQueryValidator : AbstractValidator<GetPlexAccountsWithAccessByPlexServerIdQuery>
{
    public GetPlexAccountsWithAccessByPlexServerIdQueryValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class GetPlexAccountsWithAccessByPlexServerIdQueryHandler : BaseHandler,
    IRequestHandler<GetPlexAccountsWithAccessByPlexServerIdQuery, Result<List<PlexAccount>>>
{
    public GetPlexAccountsWithAccessByPlexServerIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<PlexAccount>>> Handle(GetPlexAccountsWithAccessByPlexServerIdQuery request, CancellationToken cancellationToken)
    {
        var query = await _dbContext.PlexAccountServers
            .Include(x => x.PlexAccount)
            .Where(x => x.PlexServerId == request.PlexServerId)
            .Select(x => x.PlexAccount)
            .ToListAsync(cancellationToken);

        if (!query.Any())
            return Result.Fail($"There were no PlexAccounts that have access to PlexServer with id: {request.PlexServerId}").LogError();

        return Result.Ok(query);
    }
}