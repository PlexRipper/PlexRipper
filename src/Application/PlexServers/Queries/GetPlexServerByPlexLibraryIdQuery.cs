using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByPlexLibraryIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexLibraryIdQuery(int id, bool includePlexLibraries = false)
        {
            Id = id;
            IncludePlexLibraries = includePlexLibraries;
        }

        public int Id { get; }
        public bool IncludePlexLibraries { get; }
    }

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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByPlexLibraryIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexLibraryIdQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexServers.AsQueryable();

            if (request.IncludePlexLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            var plexServer = await query
                .Where(x => x.PlexLibraries.Any(y => y.Id == request.Id))
                .FirstOrDefaultAsync(cancellationToken);

            if (plexServer == null)
            {
                return ResultExtensions.Get404NotFoundResult($"Could not find PlexLibrary with Id {request.Id} in any PlexServer");
            }

            return Result.Ok(plexServer);
        }
    }
}