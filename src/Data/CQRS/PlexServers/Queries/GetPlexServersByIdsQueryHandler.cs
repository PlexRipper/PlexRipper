using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetPlexServersByIdsQueryValidator : AbstractValidator<GetPlexServersByIdsQuery>
{
    public GetPlexServersByIdsQueryValidator()
    {
        RuleFor(x => x.Ids.Any()).Equal(true);
    }
}

public class GetPlexServersByIdsQueryHandler : BaseHandler,
    IRequestHandler<GetPlexServersByIdsQuery, Result<List<PlexServer>>>
{
    public GetPlexServersByIdsQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<PlexServer>>> Handle(GetPlexServersByIdsQuery request, CancellationToken cancellationToken)
    {
        var query = PlexServerQueryable.AsQueryable();

        if (request.IncludeConnections)
            query = query.Include(x => x.PlexServerConnections);

        if (request.IncludeLibraries)
            query = query.Include(x => x.PlexLibraries);

        var plexServers = await query
            .Where(x => request.Ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return Result.Ok(plexServers);
    }
}