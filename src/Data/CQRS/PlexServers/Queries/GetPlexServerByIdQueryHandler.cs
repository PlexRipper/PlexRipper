using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers
{
    public class GetPlexServerByIdQueryValidator : AbstractValidator<GetPlexServerByIdQuery>
    {
        public GetPlexServerByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexServerByIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var query = PlexServerQueryable
                .Include(x => x.ServerStatus)
                .AsQueryable();

            if (request.IncludeLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServer = await query
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexServer == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexServer), request.Id);
            }

            return Result.Ok(plexServer);
        }
    }
}