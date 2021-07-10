using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class PlexRipperDbContext : DbContext, ISetup
    {
        #region Properties

        #region Tables

        public DbSet<PlexAccount> PlexAccounts { get; set; }

        public DbSet<DownloadTask> DownloadTasks { get; set; }

        public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

        public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; set; }

        public DbSet<FolderPath> FolderPaths { get; set; }

        public DbSet<DownloadFileTask> FileTasks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<PlexGenre> PlexGenres { get; set; }

        public DbSet<PlexLibrary> PlexLibraries { get; set; }

        #region PlexMovie

        public DbSet<PlexMovie> PlexMovies { get; set; }

        #endregion

        #region PlexTvShow

        public DbSet<PlexTvShow> PlexTvShows { get; set; }

        public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

        public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

        #endregion

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

        private static bool IsTestMode
        {
            get
            {
                var testMode = Environment.GetEnvironmentVariable("IntegrationTestMode");
                return testMode != null && testMode == "true";
            }
        }

        private static bool ResetDatabase
        {
            get
            {
                var resetDb = Environment.GetEnvironmentVariable("ResetDB");
                return resetDb != null && resetDb == "true";
            }
        }

        private static string DatabaseName => IsTestMode ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";

        #endregion Properties

        #region Constructors

        public PlexRipperDbContext() { }

        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options) { }

        #endregion Constructors

        #region Methods

        public async Task<Result> SetupAsync()
        {
            // Should the Database be deleted and re-created
            if (ResetDatabase)
            {
                Log.Warning("ResetDB command is true, database will be deleted and re-created.");
                await Database.EnsureDeletedAsync();
            }

            if (!IsTestMode)
            {
                Log.Information("Attempting to migrate database");
                await Database.MigrateAsync();
            }
            else
            {
                Log.Information("Database will be setup in TestMode");
                await Database.EnsureCreatedAsync();
            }

            // Check if database exists and can be connected to.
            var exist = await Database.CanConnectAsync();
            if (exist)
            {
                Log.Information("Database was successfully connected!");
                return Result.Ok();
            }

            Log.Error("Database could not be created and or migrated.");
            return Result.Fail($"Could not create database {DatabaseName} in {FileSystemPaths.ConfigDirectory}").LogError();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(FileSystemPaths.ConfigDirectory, DatabaseName);

                // optionsBuilder.UseLazyLoadingProxies();
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder
                    .UseSqlite(
                        $"Data Source={dbPath}",
                        b => b.MigrationsAssembly(typeof(PlexRipperDbContext).Assembly.FullName));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<PlexMovie>()
                .Property(x => x.MediaData)
                .HasJsonValueConversion();

            builder.Entity<PlexTvShow>()
                .Property(x => x.MediaData)
                .HasJsonValueConversion();

            builder.Entity<PlexTvShowSeason>()
                .Property(x => x.MediaData)
                .HasJsonValueConversion();

            builder.Entity<PlexTvShowEpisode>()
                .Property(x => x.MediaData)
                .HasJsonValueConversion();

            builder = PlexRipperDBContextSeed.SeedDatabase(builder);

            base.OnModelCreating(builder);
        }

        #endregion Methods
    }
}