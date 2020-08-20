using System.Linq;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetPlexAccountByIdQuery : IRequest<Result<PlexAccount>>
    {
        /// <summary>
        /// Returns the <see cref="PlexAccount"/> by its id with optional includes.
        /// </summary>
        public GetPlexAccountByIdQuery(int id, bool includePlexServers = false, bool includePlexLibraries = false)
        {
            Id = id;
            IncludePlexServers = includePlexServers;
            IncludePlexLibraries = includePlexLibraries;
        }

        public int Id { get; }
        public bool IncludePlexServers { get; }
        public bool IncludePlexLibraries { get; }
    }

    public class GetAccountByIdQueryValidator : AbstractValidator<GetPlexAccountByIdQuery>
    {
        public GetAccountByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetAccountByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexAccountByIdQuery, Result<PlexAccount>>
    {
        public GetAccountByIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<PlexAccount>> Handle(GetPlexAccountByIdQuery request, CancellationToken cancellationToken)
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
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexAccount == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexAccount), request.Id);
            }

            if (request.IncludePlexServers && request.IncludePlexLibraries)
            {
                // Remove any PlexLibraries the plexAccount has no access to
                // TODO This might be improved further since now all PlexLibraries will be retrieved from the database.
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

            return Result.Ok(plexAccount);
        }
    }
}