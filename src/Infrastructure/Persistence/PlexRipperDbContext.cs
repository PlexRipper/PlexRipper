using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain.Entities;
using System.IO;
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
            // TODO  EF Core Tracking is off
            // https://agirlamonggeeks.com/2019/05/06/entity-framework-asnotracking-why-how-ef-and-ef-core/
            // optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseLazyLoadingProxies();

            if (!optionsBuilder.IsConfigured)
            {
                var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dbPath = Path.Combine(rootDir, "PlexRipperDB.db");

                optionsBuilder
                    .UseSqlite(
                    $"Data Source={dbPath}",
                    b => b.MigrationsAssembly(typeof(PlexRipperDbContext).Assembly.FullName));
            }
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<PlexAccount> PlexAccounts { get; set; }
        public DbSet<PlexServer> PlexServers { get; set; }
        public DbSet<PlexAccountServer> PlexAccountServers { get; set; }
        public DbSet<PlexLibrary> PlexLibraries { get; set; }

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
