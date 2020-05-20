using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dbPath = Path.Combine(rootDir, "PlexRipperDB.db");

            optionsBuilder.UseSqlite(
                $"Data Source={dbPath}",
                b => b.MigrationsAssembly(typeof(PlexRipperDbContext).Assembly.FullName));

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

        public override EntityEntry Entry(object entity)
        {
            return base.Entry(entity);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}
