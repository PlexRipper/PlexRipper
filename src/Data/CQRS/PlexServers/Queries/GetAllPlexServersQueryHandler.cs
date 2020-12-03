using System.Collections.Generic;
using System.Linq;
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
    public class GetAllPlexServersQueryValidator : AbstractValidator<GetAllPlexServersQuery> { }

    public class GetAllPlexServersQueryHandler : BaseHandler,
        IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
    {
        public GetAllPlexServersQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PlexAccountId == 0)
            {
                // TODO This might return PlexServers which have no PlexAccounts available that can access them.
                var query = await _dbContext.PlexServers
                    .Include(x => x.ServerStatus)
                    .Include(x => x.PlexLibraries)
                    .ToListAsync(cancellationToken);

                return Result.Ok(query);
            }
            else
            {
                var query = await _dbContext.PlexAccountServers
                    .Include(x => x.PlexServer)
                    .ThenInclude(x => x.ServerStatus)
                    .Include(x => x.PlexServer)
                    .ThenInclude(x => x.PlexLibraries)
                    .Where(x => x.PlexAccountId == request.PlexAccountId)
                    .ToListAsync();
                return Result.Ok(query.Select(x => x.PlexServer).ToList());
            }
        }
    }
}