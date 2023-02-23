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

        if (request.IncludeStatus)
            query = query.Include(x => x.PlexServerStatus);

        var plexServerConnections = await query
            .ToListAsync(cancellationToken);

        if (request.IncludeStatus)
        {
            foreach (var plexServerConnection in plexServerConnections)
                plexServerConnection.PlexServerStatus.Sort((x, y) => DateTime.Compare(x.LastChecked, y.LastChecked));
        }

        return Result.Ok(plexServerConnections);
    }
}