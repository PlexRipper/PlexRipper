using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers.Queries
{
    public class GetPlexServerByIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByIdQuery(int id, bool includeLibraries = false, bool includeDownloadTasks = false)
        {
            Id = id;
            IncludeLibraries = includeLibraries;
            IncludeDownloadTasks = includeDownloadTasks;
        }

        public int Id { get; }
        public bool IncludeLibraries { get; }
        public bool IncludeDownloadTasks { get; }
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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexServerByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByIdQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexServers.AsQueryable();

            if (request.IncludeLibraries)
            {
                query = query.Include(x => x.PlexLibraries);
            }

            if (request.IncludeDownloadTasks)
            {
                query = query.Include(x => x.DownloadTasks);
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
