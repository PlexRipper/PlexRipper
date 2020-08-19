using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServersByPlexAccountIdQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetPlexServersByPlexAccountIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

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
        public GetPlexServersByAccountIdQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

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
