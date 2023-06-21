using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetPlexServerByIdQueryValidator : AbstractValidator<GetPlexServerByIdQuery>
{
    #region Constructors

    public GetPlexServerByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }

    #endregion
}

public class GetPlexServerByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerByIdQuery, Result<PlexServer>>
{
    #region Constructors

    public GetPlexServerByIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    #endregion

    #region Methods

    #region Public

    public async Task<Result<PlexServer>> Handle(GetPlexServerByIdQuery request, CancellationToken cancellationToken)
    {
        var query = PlexServerQueryable.AsQueryable();

        if (request.IncludeLibraries)
            query = query.Include(x => x.PlexLibraries);

        if (request.IncludeConnections)
            query = query.Include(x => x.PlexServerConnections).ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1));

        var plexServer = await query
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexServer == null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), request.Id);

        return Result.Ok(plexServer);
    }

    #endregion

    #endregion
}