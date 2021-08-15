using System.Reflection;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class PlexRipperDbContext : DbContext, ISetupAsync
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

        public static string DatabaseName => EnviromentExtensions.IsIntegrationTestMode() ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";

        public string DatabasePath => @$"I:\Projects\PlexRipper\src\WebAPI\bin\Debug\net5.0\config\{DatabaseName}";


        #endregion Properties

        #region Constructors

        public PlexRipperDbContext()
        {

        }

        public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options) : base(options) { }

        #endregion Constructors

        #region Methods

        public async Task<Result> SetupAsync()
        {
            // Should the Database be deleted and re-created
            if (EnviromentExtensions.IsResetDatabase())
            {
                Log.Warning("ResetDB command is true, database will be deleted and re-created.");
                await Database.EnsureDeletedAsync();
            }

            if (EnviromentExtensions.IsIntegrationTestMode())
            {
                Log.Information("Database will be setup in TestMode");
                await Database.EnsureCreatedAsync();
            }

            try
            {
                if (!EnviromentExtensions.IsIntegrationTestMode())
                {
                    Log.Information("Attempting to migrate database");
                    await Database.MigrateAsync();
                }
            }
            catch (SqliteException e)
            {
                Log.Error("Failed to migrate the database.");
                return Result.Fail(new ExceptionalError(e)).LogError();
            }

            // Check if database exists and can be connected to.
            var exist = await Database.CanConnectAsync();
            if (exist)
            {
                Log.Information("Database was successfully connected!");
                return Result.Ok();
            }

            Log.Error("Database could not be created and or migrated.");
            return Result.Fail($"Could not create database {DatabaseName} in the config directory").LogError();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                Log.Error($"DatabasePath is invalid or empty: {DatabasePath}");
                return;
            }

            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseLazyLoadingProxies();
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                optionsBuilder
                    .UseSqlite(
                        $"Data Source={DatabasePath}",
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