using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class GetAllPlexAccountsQuery : IRequest<Result<List<PlexAccount>>>
    {

        public bool OnlyEnabled { get; }

        public GetAllPlexAccountsQuery(bool onlyEnabled = false)
        {
            OnlyEnabled = onlyEnabled;
        }
    }

    public class GetAllPlexAccountsQueryValidator : AbstractValidator<GetAllPlexAccountsQuery>
    {
        public GetAllPlexAccountsQueryValidator()
        {

        }
    }


    public class
        GetAllPlexAccountsHandler : IRequestHandler<GetAllPlexAccountsQuery, Result<List<PlexAccount>>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAllPlexAccountsHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<PlexAccount>>> Handle(GetAllPlexAccountsQuery request,
            CancellationToken cancellationToken)
        {
            var plexAccounts = await _dbContext.PlexAccounts
                    .Include(x => x.PlexAccountServers)
                    .ThenInclude(x => x.PlexServer)
                    .Select(plexAccount => new PlexAccount
                    {
                        Id = plexAccount.Id,
                        DisplayName = plexAccount.DisplayName,
                        Username = plexAccount.Username,
                        Password = plexAccount.Password,
                        IsEnabled = plexAccount.IsEnabled,
                        IsValidated = plexAccount.IsValidated,
                        ValidatedAt = plexAccount.ValidatedAt,
                        PlexId = plexAccount.PlexId,
                        Uuid = plexAccount.Uuid,
                        Email = plexAccount.Email,
                        JoinedAt = plexAccount.JoinedAt,
                        Title = plexAccount.Title,
                        HasPassword = plexAccount.HasPassword,
                        AuthenticationToken = plexAccount.AuthenticationToken,
                        ForumId = plexAccount.ForumId,
                        PlexAccountServers = plexAccount.PlexAccountServers,
                        PlexServers = plexAccount.PlexAccountServers
                            .Where(z => z.PlexAccountId == plexAccount.Id)
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
                                // Only select the PlexLibraries this PlexAccount has access to by looking at the PlexAccountLibraries table.
                                ServerStatus = null,
                                PlexLibraries = _dbContext.PlexAccountLibraries
                                    .Include(f => f.PlexLibrary)
                                    .Where(d => d.PlexAccountId == c.PlexAccountId && d.PlexServerId == c.PlexServer.Id)
                                    .Select(v => new PlexLibrary
                                    {
                                        Id = v.PlexLibraryId,
                                        Key = v.PlexLibrary.Key,
                                        Title = v.PlexLibrary.Title,
                                        Type = v.PlexLibrary.Type,
                                        UpdatedAt = v.PlexLibrary.UpdatedAt,
                                        CreatedAt = v.PlexLibrary.CreatedAt,
                                        ScannedAt = v.PlexLibrary.ScannedAt,
                                        ContentChangedAt = v.PlexLibrary.ContentChangedAt,
                                        Uuid = v.PlexLibrary.Uuid,
                                        LibraryLocationId = v.PlexLibrary.LibraryLocationId,
                                        LibraryLocationPath = v.PlexLibrary.LibraryLocationPath,
                                        PlexServerId = v.PlexLibrary.PlexServerId,
                                        Movies = v.PlexLibrary.Movies,
                                        Series = v.PlexLibrary.Series
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                    })
                    .ToListAsync();


            if (request.OnlyEnabled)
            {
                return Result.Ok(plexAccounts.Where(x => x.IsEnabled).ToList());
            }

            return Result.Ok(plexAccounts);
        }
    }
}
