using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetPlexAccountByIdWithPlexServersQuery : IRequest<Result<PlexAccount>>
    {
        public int Id { get; }

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> by its id with an include to the accessible <see cref="PlexServer"/>s.
        /// </summary>
        public GetPlexAccountByIdWithPlexServersQuery(int id)
        {
            Id = id;
        }
    }

    public class GetPlexAccountByIdWithPlexServersQueryValidator : AbstractValidator<GetPlexAccountByIdWithPlexServersQuery>
    {
        public GetPlexAccountByIdWithPlexServersQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexAccountByIdWithPlexServersQueryHandler : BaseHandler, IRequestHandler<GetPlexAccountByIdWithPlexServersQuery, Result<PlexAccount>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexAccountByIdWithPlexServersQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexAccount>> Handle(GetPlexAccountByIdWithPlexServersQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexAccountByIdWithPlexServersQuery, GetPlexAccountByIdWithPlexServersQueryValidator>(request);
            if (result.IsFailed) return result;

            var id = request.Id;

            var account = await _dbContext.PlexAccounts
                .Include(x => x.PlexAccountServers)
                .ThenInclude(x => x.PlexServer)
                .Select(x => new PlexAccount
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Username = x.Username,
                    Password = x.Password,
                    IsEnabled = x.IsEnabled,
                    IsValidated = x.IsValidated,
                    ValidatedAt = x.ValidatedAt,
                    PlexId = x.PlexId,
                    Uuid = x.Uuid,
                    Email = x.Email,
                    JoinedAt = x.JoinedAt,
                    Title = x.Title,
                    HasPassword = x.HasPassword,
                    AuthenticationToken = x.AuthenticationToken,
                    ForumId = x.ForumId,
                    PlexAccountServers = x.PlexAccountServers,
                    PlexServers = x.PlexAccountServers
                        .Where(z => z.PlexAccountId == id)
                        .Select(c => new PlexServer
                        {
                            Id = c.PlexServer.Id,
                            Name = c.PlexServer.Name,
                            Scheme = c.PlexServer.Scheme,
                            Address = c.PlexServer.Address,
                            Port = c.PlexServer.Port,
                            Version = c.PlexServer.Version,
                            Host = c.PlexServer.Host,
                            LocalAddresses = c.PlexServer.LocalAddresses,
                            MachineIdentifier = c.PlexServer.MachineIdentifier,
                            CreatedAt = c.PlexServer.CreatedAt,
                            UpdatedAt = c.PlexServer.UpdatedAt,
                            Owned = c.PlexServer.Owned,
                            Synced = c.PlexServer.Synced,
                            OwnerId = c.PlexServer.OwnerId,
                            Home = c.PlexServer.Home,
                            PlexAccountServers = c.PlexServer.PlexAccountServers,
                            ServerStatus = null,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            return ReturnResult(account, id);
        }
    }
}
