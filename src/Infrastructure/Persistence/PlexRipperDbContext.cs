using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Infrastructure.Common.Interfaces;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Persistence
{
    public class PlexRipperDbContext : DbContext, IPlexRipperDbContext
    {

        public DbContext Instance => this;

        public PlexRipperDbContext() { }

        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SetConfig(optionsBuilder);
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

        private static void SetConfig(DbContextOptionsBuilder optionsBuilder, bool isTest = false)
        {
            optionsBuilder.UseLazyLoadingProxies();

            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            string dbName = isTest ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";
            string dbPath = Path.Combine(rootDir, dbName);

            optionsBuilder
                .UseSqlite(
                    $"Data Source={dbPath}",
                    b => b.MigrationsAssembly(typeof(PlexRipperDbContext).Assembly.FullName));
        }

        public static DbContextOptionsBuilder<PlexRipperDbContext> GetConfig()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
            SetConfig(optionsBuilder);
            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<PlexRipperDbContext> GetTestConfig()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlexRipperDbContext>();
            SetConfig(optionsBuilder, true);
            return optionsBuilder;
        }

    }
}
