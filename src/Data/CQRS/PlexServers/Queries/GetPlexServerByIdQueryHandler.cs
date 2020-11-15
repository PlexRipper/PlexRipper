using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexServers
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
            var query = _dbContext.PlexServers.AsQueryable();

            if (request.IncludeLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServer = await query
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexServer == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexServer), request.Id);
            }

            return Result.Ok(plexServer);
        }
    }
}