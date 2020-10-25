using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexAccounts
{
    public class GetAccountByUsernameValidator : AbstractValidator<GetPlexAccountByUsernameQuery>
    {
        public GetAccountByUsernameValidator()
        {
            RuleFor(x => x.Username).Length(5, 250).NotEmpty();
        }
    }

    public class GetPlexAccountByUsernameQueryHandler : BaseHandler, IRequestHandler<GetPlexAccountByUsernameQuery, Result<PlexAccount>>
    {
        public GetPlexAccountByUsernameQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexAccount>> Handle(GetPlexAccountByUsernameQuery request, CancellationToken cancellationToken)
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

            var plexAccount = await query
                .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

            if (plexAccount == null)
            {
                return ResultExtensions.Create404NotFoundResult($"Could not find a {nameof(PlexAccount)} with the username: {request.Username}");
            }

            // Remove any PlexLibraries the plexAccount has no access to
            // TODO This might be improved further since now all PlexLibraries will be retrieved from the database.
            var plexServers = plexAccount?.PlexAccountServers?.Select(x => x.PlexServer).ToList() ?? new List<PlexServer>();
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

            return Result.Ok(plexAccount);
        }
    }
}