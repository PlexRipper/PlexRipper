using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetAllPlexServersQueryValidator : AbstractValidator<GetAllPlexServersQuery> { }

public class GetAllPlexServersQueryHandler : BaseHandler, IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
{
    public GetAllPlexServersQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersQuery request, CancellationToken cancellationToken)
    {
        var query = PlexServerQueryable.AsQueryable();

        if (request.IncludeConnections)
            query = query.Include(x => x.PlexServerConnections);

        if (request.IncludeLibraries)
            query = query.Include(x => x.PlexLibraries);

        var plexServers = await query.ToListAsync(cancellationToken);

        return Result.Ok(plexServers);
    }
}