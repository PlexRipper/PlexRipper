using System.Collections.Generic;
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
    public class GetAllPlexServersQuery : IRequest<Result<List<PlexServer>>>
    {
        public GetAllPlexServersQuery(bool includeLibraries = false, bool includeDownloadTasks = false)
        {
            IncludeLibraries = includeLibraries;
            IncludeDownloadTasks = includeDownloadTasks;
        }

        public bool IncludeLibraries { get; }
        public bool IncludeDownloadTasks { get; }
    }

    public class GetAllPlexServersQueryHandler : BaseHandler,
        IRequestHandler<GetAllPlexServersQuery, Result<List<PlexServer>>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAllPlexServersQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<PlexServer>>> Handle(GetAllPlexServersQuery request,
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
                .ToListAsync(cancellationToken);

            return Result.Ok(plexServer);
        }
    }
}