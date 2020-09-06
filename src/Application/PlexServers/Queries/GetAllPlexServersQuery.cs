using System.Collections.Generic;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetAllPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersQuery(bool includeLibraries = false)
        {
            IncludeLibraries = includeLibraries;
        }

        public bool IncludeLibraries { get; }
    }

    public class GetAllPlexServersQueryValidator : AbstractValidator<GetAllPlexServersQuery>
    {
        public GetAllPlexServersQueryValidator() { }
    }

    public class GetAllPlexServersQueryHandler : BaseHandler,
        IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
    {
        public GetAllPlexServersQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

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