using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetAllPlexServerConnectionsQueryValidator : AbstractValidator<GetAllPlexServerConnectionsQuery> { }

public class GetAllPlexServerConnectionsQueryHandlerHandler : BaseHandler, IRequestHandler<GetAllPlexServerConnectionsQuery, Result<List<PlexServerConnection>>>
{
    public GetAllPlexServerConnectionsQueryHandlerHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<PlexServerConnection>>> Handle(GetAllPlexServerConnectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext
            .PlexServerConnections
            .AsQueryable();

        if (request.IncludeServer)
            query = query.Include(x => x.PlexServer);

        var plexServerConnections = await query
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .ToListAsync(cancellationToken);

        return Result.Ok(plexServerConnections);
    }
}