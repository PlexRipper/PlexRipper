using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexServers
{
    public class GetPlexServerByPlexLibraryIdQueryValidator : AbstractValidator<GetPlexServerByPlexLibraryIdQuery>
    {
        public GetPlexServerByPlexLibraryIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexServerByPlexLibraryIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByPlexLibraryIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexLibraryIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexLibraryIdQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexServers
                .Include(x => x.ServerStatus)
                .AsQueryable();

            if (request.IncludePlexLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServer = await query
                .Where(x => x.PlexLibraries.Any(y => y.Id == request.Id))
                .FirstOrDefaultAsync(cancellationToken);

            if (plexServer == null)
            {
                return ResultExtensions.Create404NotFoundResult($"Could not find PlexLibrary with Id {request.Id} in any PlexServer");
            }

            return Result.Ok(plexServer);
        }
    }
}