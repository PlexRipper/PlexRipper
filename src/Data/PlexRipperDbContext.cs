using System.Reflection;
using Environment;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public sealed class PlexRipperDbContext : DbContext, ISetupAsync
{
    private readonly IPathProvider _pathProvider;

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

    #region PlexServers

    public DbSet<PlexServer> PlexServers { get; set; }

    public DbSet<PlexServerConnection> PlexServerConnections { get; set; }

    public DbSet<PlexServerStatus> PlexServerStatuses { get; set; }

    #endregion

    #endregion

    #region JoinTables

    public DbSet<PlexAccountServer> PlexAccountServers { get; set; }

    public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; set; }

    public DbSet<PlexMovieGenre> PlexMovieGenres { get; set; }

    public DbSet<PlexMovieRole> PlexMovieRoles { get; set; }

    #endregion

    public string DatabaseName { get; set; }

    public string DatabasePath { get; set; }

    public string ConfigDirectory { get; set; }

    #endregion Properties

    #region Constructors

    public PlexRipperDbContext() { }

    public PlexRipperDbContext(IPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
        DatabaseName = _pathProvider.DatabaseName;
        DatabasePath = _pathProvider.DatabasePath;
        ConfigDirectory = _pathProvider.ConfigDirectory;
    }

    public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options, string databaseName = "") : base(options)
    {
        // This is to add tables when created in memory
        // https://stackoverflow.com/a/60497822/8205497
        Database.OpenConnection();
        Database.EnsureCreated();
        DatabaseName = databaseName;
    }

    #endregion Constructors

    #region Methods

    public async Task<Result> SetupAsync()
    {
        try
        {
            // Don't migrate when running in memory, this causes error:
            // "Relational-specific methods can only be used when the context is using a relational database provider."
            if (!Database.IsInMemory() && !EnvironmentExtensions.IsIntegrationTestMode())
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
            if (!EnvironmentExtensions.IsIntegrationTestMode())
            {
                Log.Information("Database was successfully connected!");
                Log.Information($"Database connected at: {DatabasePath}");
            }

            return Result.Ok();
        }

        Log.Error("Database could not be created and or migrated.");
        return Result.Fail($"Could not create database {DatabaseName} in {ConfigDirectory}").LogError();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.EnableDetailedErrors();
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