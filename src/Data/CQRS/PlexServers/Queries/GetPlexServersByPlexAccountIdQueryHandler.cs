using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexServers
{
    public class GetPlexServersByAccountIdQueryValidator : AbstractValidator<GetPlexServersByPlexAccountIdQuery>
    {
        public GetPlexServersByAccountIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexServersByAccountIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServersByPlexAccountIdQuery, Result<List<PlexServer>>>
    {
        public GetPlexServersByAccountIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexServer>>> Handle(GetPlexServersByPlexAccountIdQuery request,
            CancellationToken cancellationToken)
        {
            var plexServers = await _dbContext.PlexServers
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexAccount)
                .Where(x => x.PlexAccountServers
                    .Any(y => y.PlexAccount.Id == request.Id))
                .ToListAsync(cancellationToken);

            return Result.Ok(plexServers);
        }
    }
}