using System.Reflection;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite;
using Data.Contracts;
using EFCore.BulkExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using PlexRipper.Data.Common;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PlexRipper.Data;

public sealed class PlexRipperDbContext : DbContext, IPlexRipperDbContext, IPlexRipperDbContextDatabase
{
    #region Properties

    #region Tables

    public DbSet<PlexAccount> PlexAccounts { get; set; }

    public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

    public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; set; }

    public DbSet<FolderPath> FolderPaths { get; set; }

    public DbSet<FileTask> FileTasks { get; set; }

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

    #region DownloadTasks

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; set; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; set; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; set; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; set; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; set; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; set; }

    #endregion

    #region JoinTables

    public DbSet<PlexAccountServer> PlexAccountServers { get; set; }

    public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; set; }

    public DbSet<PlexMovieGenre> PlexMovieGenres { get; set; }

    public DbSet<PlexMovieRole> PlexMovieRoles { get; set; }

    #endregion

    public string DatabaseName { get; }

    public async Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        await DbContextBulkExtensions.BulkInsertAsync(this, entities, bulkConfig, cancellationToken: cancellationToken);
    }

    public async Task BulkUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        await DbContextBulkExtensions.BulkUpdateAsync(this, entities, bulkConfig, cancellationToken: cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    private static readonly NaturalSortComparer NaturalComparer = new(StringComparison.InvariantCultureIgnoreCase);

    #endregion Properties

    #region Constructors

    public PlexRipperDbContext(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public PlexRipperDbContext(DbContextOptions<PlexRipperDbContext> options, string databaseName)
        : base(options)
    {
        DatabaseName = databaseName;
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    #endregion Constructors

    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Source: https://github.com/tompazourek/NaturalSort.Extension
            SqliteConnection databaseConnection = new(DbContextConnections.ConnectionString);
            databaseConnection.CreateCollation(
                OrderByNaturalExtensions.CollationName,
                (x, y) => NaturalComparer.Compare(x, y)
            );

            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.LogTo(text => LogManager.DbContextLogger(text), LogLevel.Error);
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.UseSqlite(
                databaseConnection,
                b => b.MigrationsAssembly(typeof(PlexRipperDbContext).Assembly.FullName)
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseCollation(OrderByNaturalExtensions.CollationName);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.AddQuartz(x => x.UseSqlite());

        // NOTE: This has been added to PlexRipperDbContext.OnModelCreating
        // Based on: https://stackoverflow.com/a/63992731/8205497
        builder.Entity<PlexMovie>().Property(x => x.MediaData).HasJsonValueConversion();

        builder.Entity<PlexTvShow>().Property(x => x.MediaData).HasJsonValueConversion();

        builder.Entity<PlexTvShowSeason>().Property(x => x.MediaData).HasJsonValueConversion();

        builder.Entity<PlexTvShowEpisode>().Property(x => x.MediaData).HasJsonValueConversion();

        builder = PlexRipperDBContextSeed.SeedDatabase(builder);

        base.OnModelCreating(builder);
    }

    #endregion Methods

    /// <inheritdoc/>
    public bool CanConnect() => Database.CanConnect();

    /// <inheritdoc/>
    public bool IsInMemory() => Database.IsInMemory();

    /// <inheritdoc/>
    public void CloseConnection() => Database.CloseConnection();

    /// <inheritdoc/>
    public Result<bool> EnsureDeleted()
    {
        try
        {
            return Result.Ok(Database.EnsureDeleted());
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    /// <inheritdoc/>
    public Result Migrate()
    {
        try
        {
            Database.Migrate();
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetPendingMigrations() => Database.GetPendingMigrations();
}
