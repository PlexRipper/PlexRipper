using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetAllPlexAccountsQuery : IRequest<Result<List<PlexAccount>>>
    {
        public GetAllPlexAccountsQuery(bool includePlexServers = false, bool includePlexLibraries = false, bool onlyEnabled = false)
        {
            IncludePlexServers = includePlexServers;
            IncludePlexLibraries = includePlexLibraries;
            OnlyEnabled = onlyEnabled;
        }

        public bool IncludePlexServers { get; }
        public bool IncludePlexLibraries { get; }
        public bool OnlyEnabled { get; }
    }


    public class GetAllPlexAccountsQueryValidator : AbstractValidator<GetAllPlexAccountsQuery>
    {
        public GetAllPlexAccountsQueryValidator()
        {
        }
    }

    public class GetAllPlexAccountsHandler : BaseHandler, IRequestHandler<GetAllPlexAccountsQuery, Result<List<PlexAccount>>>
    {
        private readonly IMapper _mapper;

        public GetAllPlexAccountsHandler(IPlexRipperDbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _mapper = mapper;
        }

        public async Task<Result<List<PlexAccount>>> Handle(GetAllPlexAccountsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexAccounts.AsQueryable();

            if (request.IncludePlexServers && !request.IncludePlexLibraries)
            {
                query = query
                    .Include(x => x.PlexAccountServers)
                    .ThenInclude(x => x.PlexServer);
            }

            if (request.IncludePlexServers && request.IncludePlexLibraries)
            {
                query = query
                    .Include(v => v.PlexAccountServers)
                    .ThenInclude(x => x.PlexServer)
                    .ThenInclude(x => x.PlexLibraries)
                    .ThenInclude(x => x.PlexAccountLibraries);
            }

            List<PlexAccount> plexAccounts;
            if (request.OnlyEnabled)
            {
                plexAccounts = await query.Where(x => x.IsEnabled).ToListAsync(cancellationToken);
            }
            else
            {
                plexAccounts = await query.ToListAsync(cancellationToken);
            }

            // Remove any PlexLibraries the plexAccount has no access to
            // TODO This might be improved further since now all PlexLibraries will be retrieved from the database.
            foreach (var plexAccount in plexAccounts)
            {
                var plexServers = plexAccount.PlexAccountServers.Select(x => x.PlexServer).ToList();
                foreach (var plexServer in plexServers)
                {
                    // Remove inaccessible PlexLibraries
                    for (int i = plexServer.PlexLibraries.Count - 1; i >= 0; i--)
                    {
                        var x = plexServer?.PlexLibraries[i].PlexAccountLibraries.Select(y => y.PlexAccountId).ToList();
                        if (!x.Contains(plexAccount.Id))
                        {
                            plexServer.PlexLibraries.RemoveAt(i);
                        }
                    }
                }
            }

            return Result.Ok(plexAccounts);
        }
    }
}