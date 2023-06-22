using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetAllPlexServersQueryValidator : AbstractValidator<GetAllPlexServersQuery> { }

public class GetAllPlexServersQueryHandler : BaseHandler, IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
{
    #region Constructors

    public GetAllPlexServersQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    #endregion

    #region Methods

    #region Public

    public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersQuery request, CancellationToken cancellationToken)
    {
        var query = PlexServerQueryable.AsQueryable();

        if (request.IncludeConnections)
        {
            query = query
                .Include(x => x.PlexServerConnections)
                .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1));
        }

        if (request.IncludeLibraries)
            query = query.Include(x => x.PlexLibraries);

        var plexServers = await query.ToListAsync(cancellationToken);

        return Result.Ok(plexServers);
    }

    #endregion

    #endregion
}