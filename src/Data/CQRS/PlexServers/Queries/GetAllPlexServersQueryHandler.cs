using System.Collections.Generic;
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
            var query = _dbContext.PlexServers.AsQueryable();

            if (request.IncludeLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServer = await query
                .ToListAsync(cancellationToken);

            return Result.Ok(plexServer);
        }
    }
}