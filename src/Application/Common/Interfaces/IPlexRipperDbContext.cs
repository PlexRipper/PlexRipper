using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexRipperDbContext
    {
        DbSet<PlexAccount> PlexAccounts { get; set; }
        DbSet<Account> Accounts { get; set; }
        DbSet<PlexServer> PlexServers { get; set; }
        DbSet<PlexAccountServer> PlexAccountServers { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
