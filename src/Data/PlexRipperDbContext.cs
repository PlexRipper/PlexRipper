using System;
using System.IO;
using System.Reflection;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class PlexRipperDbContext : DbContext
    {
        private readonly IFileSystem _fileSystem;

        #region Properties

        #region Tables

        public DbSet<PlexAccount> PlexAccounts { get; set; }

        public DbSet<DownloadTask> DownloadTasks { get; set; }

        public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

        public DbSet<FolderPath> FolderPaths { get; set; }

        public DbSet<DownloadFileTask> FileTasks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<PlexGenre> PlexGenres { get; set; }

        public DbSet<PlexLibrary> PlexLibraries { get; set; }

        #region PlexMovie

        public DbSet<PlexMovie> PlexMovies { get; set; }

        public DbSet<PlexMovieData> PlexMovieData { get; set; }

        public DbSet<PlexMovieDataPart> PlexMovieDataParts { get; set; }

        #endregion

        #region PlexTvShow

        public DbSet<PlexTvShow> PlexTvShows { get; set; }

        public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

        public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

        public DbSet<PlexTvShowEpisodeData> PlexTvShowEpisodeData { get; set; }

        public DbSet<PlexTvShowEpisodeDataPart> PlexTvShowEpisodeDataParts { get; set; }

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

        public PlexRipperDbContext(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options) { }

        #endregion Constructors

        #region Methods

        public Result Setup()
        {
            // Should the Database be deleted and re-created
            if (ResetDatabase)
            {
                Log.Warning("ResetDB command is true, database will be deleted and re-created.");
                Database.EnsureDeleted();
            }

            // TODO Re-enable Migrate when stable
            // DB.Database.Migrate();
            // Check if database exists and can be connected to.
            var exist = Database.CanConnect();
            if (!exist)
            {
                Log.Information("Database does not exist, creating one now.");
                Database.EnsureCreated();
                exist = Database.CanConnect();

                if (exist)
                {
                    Log.Information("Database was successfully created and connected!");
                    return Result.Ok();
                }

                Log.Error("Database could not be created.");
                return Result.Fail($"Could not create database {DatabaseName} in {_fileSystem.ConfigDirectory}").LogError();
            }
            Log.Information($"Database {DatabaseName} exists and is successfully connected!");
            return Result.Ok();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(_fileSystem.ConfigDirectory, DatabaseName);

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

            builder = PlexRipperDBContextSeed.SeedDatabase(builder);

            base.OnModelCreating(builder);
        }

        #endregion Methods
    }
}