using System.Data;
using System.Reflection;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite;
using Data.Contracts;
using EFCore.BulkExtensions;
using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<PlexLibrary> PlexLibraries { get; set; }

    #region PlexMedia

    public DbSet<PlexActor> PlexActors { get; set; }

    public DbSet<PlexGenre> PlexGenres { get; set; }

    public DbSet<PlexCountry> PlexCountries { get; set; }

    #endregion

    #region PlexMovie

    public DbSet<PlexMovie> PlexMovies { get; set; }

    public DbSet<PlexMovieMediaData> PlexMovieData { get; set; }

    public DbSet<PlexMovieMediaDataPart> PlexMovieDataParts { get; set; }

    public DbSet<PlexMovieMediaDataStream> PlexMovieDataStreams { get; set; }

    #endregion

    #region PlexTvShow

    public DbSet<PlexTvShow> PlexTvShows { get; set; }

    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

    public DbSet<PlexTvShowEpisodeMediaData> PlexTvShowEpisodeData { get; set; }

    public DbSet<PlexTvShowEpisodeMediaDataPart> PlexTvShowEpisodeDataParts { get; set; }

    public DbSet<PlexTvShowEpisodeMediaDataStream> PlexTvShowEpisodeDataStreams { get; set; }

    #endregion

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

    public DbSet<PlexLibraryActors> PlexLibraryActors { get; set; }

    public DbSet<PlexLibraryCountries> PlexLibraryCountries { get; set; }

    public DbSet<PlexLibraryGenres> PlexLibraryGenres { get; set; }

    public DbSet<PlexMovieActors> PlexMovieActors { get; set; }

    public DbSet<PlexMovieCountries> PlexMovieCountries { get; set; }

    public DbSet<PlexMovieGenres> PlexMovieGenres { get; set; }

    public DbSet<PlexTvShowActors> PlexTvShowActors { get; set; }

    public DbSet<PlexTvShowGenres> PlexTvShowGenres { get; set; }

    public DbSet<PlexTvShowCountries> PlexTvShowCountries { get; set; }

    #endregion

    public string DatabaseName { get; }

    public async Task BulkReadAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        await DbContextBulkExtensions.BulkReadAsync(this, entities, bulkConfig, cancellationToken: cancellationToken);
    }

    public async Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var conn = this.Database.GetDbConnection();
        var openedHere = conn.State != ConnectionState.Open;
        if (openedHere)
        {
            await this.Database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            await using var tx = await this.BeginTransactionAsync(cancellationToken);
            await DbContextBulkExtensions.BulkInsertAsync(
                this,
                entities,
                bulkConfig,
                cancellationToken: cancellationToken
            );
            await tx.CommitAsync(cancellationToken);
        }
        finally
        {
            if (openedHere)
            {
                await this.Database.CloseConnectionAsync();
            }
        }
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

    public async Task BulkInsertOrUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        Action<decimal>? progress = null,
        Type? type = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var conn = this.Database.GetDbConnection();
        var openedHere = conn.State != ConnectionState.Open;
        if (openedHere)
        {
            await this.Database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            await using var tx = await this.BeginTransactionAsync(cancellationToken);
            await DbContextBulkExtensions.BulkInsertOrUpdateAsync(
                this,
                entities,
                bulkConfig,
                progress,
                type,
                cancellationToken: cancellationToken
            );
            await tx.CommitAsync(cancellationToken);
        }
        finally
        {
            if (openedHere)
            {
                await this.Database.CloseConnectionAsync();
            }
        }
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    #endregion Properties

    #region Constructors

    public PlexRipperDbContext(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public PlexRipperDbContext(IPathProvider pathProvider)
        : this(pathProvider.DatabaseName) { }

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
            optionsBuilder.DefaultConfiguration(typeof(PlexRipperDbContext));
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseCollation(OrderByNaturalExtensions.CollationName);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.AddQuartz(x => x.UseSqlite());

        // TODO Make extensions methods
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
    public Result Migrate() => Result.Try(() => Database.Migrate(), e => new ExceptionalError(e));

    /// <inheritdoc/>
    public IEnumerable<string> GetPendingMigrations() => Database.GetPendingMigrations();
}
