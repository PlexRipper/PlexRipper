using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class GetPlexServerStatusByIdQueryValidator : AbstractValidator<GetPlexServerStatusByIdQuery>
{
    public GetPlexServerStatusByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetPlexServerStatusByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerStatusByIdQuery, Result<PlexServerStatus>>
{
    public GetPlexServerStatusByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<PlexServerStatus>> Handle(GetPlexServerStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.PlexServerStatuses.AsQueryable();

        if (request.IncludePlexServer)
        {
            query = query
                .Include(x => x.PlexServer);
        }

        var status = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (status == null)
            return ResultExtensions.EntityNotFound(nameof(PlexServerStatus), request.Id);

        return Result.Ok(status);
    }
}