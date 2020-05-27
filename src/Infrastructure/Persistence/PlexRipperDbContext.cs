using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.JoinTables;
using PlexRipper.Infrastructure.Common.Interfaces;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Infrastructure.Persistence
{
    public class PlexRipperDbContext : DbContext, IPlexRipperDbContext
    {

        #region Properties

        public DbContext Instance => this;

        #region Tables
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PlexGenre> PlexGenres { get; set; }
        public DbSet<PlexAccount> PlexAccounts { get; set; }
        public DbSet<PlexLibrary> PlexLibraries { get; set; }

        public DbSet<PlexMovies> PlexMovies { get; set; }
        public DbSet<PlexRole> PlexRoles { get; set; }
        public DbSet<PlexServer> PlexServers { get; set; }
        #endregion

        #region JoinTables
        public DbSet<PlexAccountServer> PlexAccountServers { get; set; }
        public DbSet<PlexMovieGenre> PlexMovieGenres { get; set; }
        public DbSet<PlexMovieRole> PlexMovieRoles { get; set; }
        #endregion
        #endregion Properties

        #region Constructors

        public PlexRipperDbContext() { }

        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options) { }

        #endregion Constructors


        #region Methods

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder = GetConfig();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
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

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync(CancellationToken.None);
        }

        #endregion Methods





    }
}
