using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetPlexServerConnectionByIdQueryValidator : AbstractValidator<GetPlexServerConnectionByIdQuery>
{
    public GetPlexServerConnectionByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexServerConnectionByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerConnectionByIdQuery, Result<PlexServerConnection>>
{
    public GetPlexServerConnectionByIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<PlexServerConnection>> Handle(GetPlexServerConnectionByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext
            .PlexServerConnections
            .AsQueryable();

        if (request.IncludeServer)
            query = query.Include(x => x.PlexServer);

        var plexServerConnection = await query
            .Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (plexServerConnection == null)
            return ResultExtensions.EntityNotFound(nameof(PlexServerConnection), request.Id);

        return Result.Ok(plexServerConnection);
    }
}