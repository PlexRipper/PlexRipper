using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetAllPlexServerConnectionsQueryValidator : AbstractValidator<GetAllPlexServerConnectionsQuery> { }

public class GetAllPlexServerConnectionsQueryHandlerHandler : BaseHandler, IRequestHandler<GetAllPlexServerConnectionsQuery, Result<List<PlexServerConnection>>>
{
    public GetAllPlexServerConnectionsQueryHandlerHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<PlexServerConnection>>> Handle(GetAllPlexServerConnectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext
            .PlexServerConnections
            .AsQueryable();

        if (request.IncludeServer)
            query = query.Include(x => x.PlexServer);

        if (request.IncludeServer)
            query = query.Include(x => x.PlexServerStatus);

        var plexServerConnections = await query
            .ToListAsync(cancellationToken);

        return Result.Ok(plexServerConnections);
    }
}