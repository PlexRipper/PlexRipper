using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.JoinTables;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Data.Common.Interfaces
{
    public interface IPlexRipperDbContext : IDbContext
    {
        DbSet<PlexAccount> PlexAccounts { get; set; }
        DbSet<Account> Accounts { get; set; }
        DbSet<PlexServer> PlexServers { get; set; }
        DbSet<PlexAccountServer> PlexAccountServers { get; set; }
        DbSet<PlexLibrary> PlexLibraries { get; set; }
        ChangeTracker ChangeTracker { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        int SaveChanges();
        EntityEntry Entry(object entity);
    }
}
