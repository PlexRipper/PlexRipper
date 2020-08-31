using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByIdQuery(int id, bool includeLibraries = false)
        {
            Id = id;
            IncludeLibraries = includeLibraries;
        }

        public int Id { get; }
        public bool IncludeLibraries { get; }
    }

    public class GetPlexServerByIdQueryValidator : AbstractValidator<GetPlexServerByIdQuery>
    {
        public GetPlexServerByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(5);
        }
    }


    public class GetPlexServerByIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByIdQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

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
