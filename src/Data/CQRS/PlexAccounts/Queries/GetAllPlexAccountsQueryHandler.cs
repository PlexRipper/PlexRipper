using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class GetAllPlexAccountsQueryValidator : AbstractValidator<GetAllPlexAccountsQuery> { }

    public class GetAllPlexAccountsQueryHandler : BaseHandler, IRequestHandler<GetAllPlexAccountsQuery, Result<List<PlexAccount>>>
    {
        public GetAllPlexAccountsQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

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

            if (!request.IncludePlexServers && !request.IncludePlexLibraries)
            {
                return Result.Ok(plexAccounts);
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
                        if (!plexServer.PlexLibraries.Any())
                        {
                            continue;
                        }

                        var x = plexServer.PlexLibraries[i].PlexAccountLibraries.Select(y => y.PlexAccountId).ToList();
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