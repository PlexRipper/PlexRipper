using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Persistence
{
    public class PlexRipperDbContext : DbContext, IPlexRipperDbContext
    {
        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }


        public DbSet<Account> Accounts { get; set; }
        public DbSet<PlexAccount> PlexAccounts { get; set; }
        public DbSet<PlexServer> PlexServers { get; set; }
        public DbSet<PlexAccountServer> PlexAccountServers { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync(CancellationToken.None);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}
