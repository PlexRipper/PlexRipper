using System.Data;
using System.Reflection;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Data.Contracts;
using Reaparr.Environment;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Reaparr.Data;

public sealed class ReaparrDbContext : DbContext, IReaparrDbContext, IReaparrDbContextDatabase
{
    public DbSet<PlexAccount> PlexAccounts { get; set; }

    public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

    public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; set; }

    public DbSet<FolderPath> FolderPaths { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<PlexLibrary> PlexLibraries { get; set; }

    public DbSet<PlexActor> PlexActors { get; set; }

    public DbSet<PlexGenre> PlexGenres { get; set; }

    public DbSet<PlexCountry> PlexCountries { get; set; }

    public DbSet<PlexMovie> PlexMovies { get; set; }

    public DbSet<PlexMovieMediaData> PlexMovieData { get; set; }

    public DbSet<PlexMovieMediaDataPart> PlexMovieDataParts { get; set; }

    public DbSet<PlexMovieMediaDataStream> PlexMovieDataStreams { get; set; }

    public DbSet<PlexTvShow> PlexTvShows { get; set; }

    public DbSet<PlexTvShowMediaQuality> PlexTvShowMediaQualities { get; set; }

    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

    public DbSet<PlexTvShowSeasonMediaQuality> PlexTvShowSeasonMediaQualities { get; set; }

    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

    public DbSet<PlexTvShowEpisodeMediaData> PlexTvShowEpisodeData { get; set; }

    public DbSet<PlexTvShowEpisodeMediaDataPart> PlexTvShowEpisodeDataParts { get; set; }

    public DbSet<PlexTvShowEpisodeMediaDataStream> PlexTvShowEpisodeDataStreams { get; set; }

    public DbSet<PlexServer> PlexServers { get; set; }

    public DbSet<PlexServerConnection> PlexServerConnections { get; set; }

    public DbSet<PlexServerStatus> PlexServerStatuses { get; set; }

    public DbSet<LibrarySyncQueue> LibrarySyncQueues { get; set; }

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; set; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; set; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; set; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; set; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; set; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; set; }

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

    public string DatabaseName { get; }

    public Task BulkReadAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        throw new NotSupportedException(
            "BulkReadAsync is not supported in with SQLite due to issues with UseTempDB and other limitations."
                + "Use EF native reading instead."
        );

    public async Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        await ExecuteBulkAsync(
            () =>
                DbContextBulkExtensions.BulkInsertAsync(
                    this,
                    entities,
                    bulkConfig,
                    cancellationToken: cancellationToken
                ),
            cancellationToken
        );

    public async Task BulkUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        await ExecuteBulkAsync(
            () =>
                DbContextBulkExtensions.BulkUpdateAsync(
                    this,
                    entities,
                    bulkConfig,
                    cancellationToken: cancellationToken
                ),
            cancellationToken
        );

    public async Task BulkInsertOrUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        Action<decimal>? progress = null,
        Type? type = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        await ExecuteBulkAsync(
            () =>
                DbContextBulkExtensions.BulkInsertOrUpdateAsync(
                    this,
                    entities,
                    bulkConfig,
                    progress,
                    type,
                    cancellationToken: cancellationToken
                ),
            cancellationToken
        );

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    public ReaparrDbContext(string databaseName)
    {
        DatabaseName = databaseName;
    }

    [ActivatorUtilitiesConstructor]
    public ReaparrDbContext(IPathProvider pathProvider)
        : this(pathProvider.DatabaseName) { }

    public ReaparrDbContext(DbContextOptions<ReaparrDbContext> options, string databaseName)
        : base(options)
    {
        DatabaseName = databaseName;
        Database.OpenConnection();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.DefaultConfiguration(typeof(ReaparrDbContext));
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseCollation(OrderByNaturalExtensions.CollationName);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.AddQuartz(x => x.UseSqlite());

        // TODO Make extensions methods
        builder = ReaparrDBContextSeed.SeedDatabase(builder);

        base.OnModelCreating(builder);
    }

    /// <summary>
    /// Executes a bulk operation within a transaction context.
    /// This method ensures that the database connection is opened and closed properly,
    /// and that the operation is committed if successful.
    /// This is to avoid "Prepare can only be called when the connection is open."
    /// </summary>
    private async Task ExecuteBulkAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var openedHere = conn.State != ConnectionState.Open;
        if (openedHere)
            await Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var tx = await BeginTransactionAsync(cancellationToken);
            await operation();
            await tx.CommitAsync(cancellationToken);
        }
        finally
        {
            if (openedHere)
                await Database.CloseConnectionAsync();
        }
    }

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
