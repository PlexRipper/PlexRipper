using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers
{
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
        public GetPlexServersByIdsQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexServer>>> Handle(GetPlexServersByIdsQuery request, CancellationToken cancellationToken)
        {
            var query =  PlexServerQueryable
                .Include(x => x.ServerStatus)
                .AsQueryable();

            if (request.IncludeLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServers = await query
                .Where(x => request.Ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            return Result.Ok(plexServers);
        }
    }
}