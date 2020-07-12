using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.JoinTables;
using System.IO;
using System.Reflection;

namespace PlexRipper.Data
{
    public class PlexRipperDbContext : DbContext, IPlexRipperDbContext
    {

        #region Properties

        #region Tables
        public DbSet<PlexAccount> PlexAccounts { get; set; }
        public DbSet<DownloadTask> DownloadTasks { get; set; }
        public DbSet<FolderPath> FolderPaths { get; set; }

        public DbSet<PlexGenre> PlexGenres { get; set; }
        public DbSet<PlexLibrary> PlexLibraries { get; set; }

        public DbSet<PlexMovie> PlexMovies { get; set; }
        public DbSet<PlexTvShow> PlexTvShows { get; set; }
        public DbSet<PlexRole> PlexRoles { get; set; }
        public DbSet<PlexServer> PlexServers { get; set; }
        public DbSet<PlexServerStatus> PlexServerStatuses { get; set; }
        #endregion

        #region JoinTables
        public DbSet<PlexAccountServer> PlexAccountServers { get; set; }
        public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; set; }
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
            // optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            string dbName = isTest ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";
            string dbPath = Path.Combine(rootDir + "/config", dbName);

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

            builder = PlexRipperDBContextSeed.SeedDatabase(builder);

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

        #endregion Methods

    }
}
