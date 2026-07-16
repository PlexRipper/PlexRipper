using System.Data;
using System.Reflection;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.SQLite;
using EFCore.BulkExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Reaparr.Data;

public sealed class ReaparrDbContext : DbContext, IReaparrDbContext, IReaparrDbContextDatabase
{
    private readonly IPathProvider _pathProvider;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    public DbSet<PlexAccount> PlexAccounts { get; set; }

    public DbSet<FolderPath> FolderPaths { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<PlexLibrary> PlexLibraries { get; set; }

    public DbSet<PlexLibraryAccessHistoryEvent> PlexLibraryAccessHistoryEvents { get; set; }

    public DbSet<PlexActor> PlexActors { get; set; }

    public DbSet<PlexGenre> PlexGenres { get; set; }

    public DbSet<PlexCountry> PlexCountries { get; set; }

    public DbSet<PlexMovie> PlexMovies { get; set; }

    public DbSet<PlexMovieMediaData> PlexMovieData { get; set; }

    public DbSet<PlexTvShow> PlexTvShows { get; set; }

    public DbSet<PlexTvShowMediaQuality> PlexTvShowMediaQualities { get; set; }

    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

    public DbSet<PlexTvShowSeasonMediaQuality> PlexTvShowSeasonMediaQualities { get; set; }

    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

    public DbSet<PlexTvShowEpisodeMediaData> PlexTvShowEpisodeData { get; set; }

    public DbSet<PlexServer> PlexServers { get; set; }

    public DbSet<PlexServerConnection> PlexServerConnections { get; set; }

    public DbSet<PlexServerStatus> PlexServerStatuses { get; set; }

    public DbSet<LibrarySyncJobQueue> LibrarySyncJobQueues { get; set; }

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; set; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; set; }

    public DbSet<DownloadTaskMovieFileLog> DownloadTaskMovieFileLogs { get; set; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; set; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; set; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; set; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; set; }

    public DbSet<DownloadTaskTvShowEpisodeFileLog> DownloadTaskTvShowEpisodeFileLogs { get; set; }

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

    /// <inheritdoc/>
    public void ClearChangeTracker() => ChangeTracker.Clear();

    public ReaparrDbContext(IPathProvider pathProvider, IAppRuntimeInfo appRuntimeInfo)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;
        DatabaseName = pathProvider.DatabaseName;
    }

    /// <summary>
    /// Constructor for DbContextFactory - accepts pre-configured options.
    /// This is required for AddDbContextFactory to work properly.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public ReaparrDbContext(
        DbContextOptions<ReaparrDbContext> options,
        IPathProvider pathProvider,
        IAppRuntimeInfo appRuntimeInfo
    )
        : base(options)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;
        DatabaseName = pathProvider.DatabaseName;
    }

    /// <summary>
    /// Constructor for DbContextFactory with explicit database name - accepts pre-configured options used in unit and integration testing.
    /// </summary>
    public ReaparrDbContext(
        DbContextOptions<ReaparrDbContext> options,
        IPathProvider pathProvider,
        IAppRuntimeInfo appRuntimeInfo,
        string databaseName
    )
        : base(options)
    {
        _pathProvider = pathProvider;
        _appRuntimeInfo = appRuntimeInfo;
        DatabaseName = databaseName;

        Database.OpenConnection();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.DefaultConfiguration(_pathProvider, _appRuntimeInfo, typeof(ReaparrDbContext));
        }

        optionsBuilder.UseSeeding(ReaparrDBContextSeed.Seed(_pathProvider));

        optionsBuilder.UseAsyncSeeding(ReaparrDBContextSeed.SeedAsync(_pathProvider));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseCollation(OrderByNaturalExtensions.CollationName);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.AddQuartz(x => x.UseSqlite());

        base.OnModelCreating(builder);
    }

    /// <summary>
    /// Maximum number of retry attempts (not including the initial attempt) for SQLite busy errors.
    /// </summary>
    private const int MaxRetries = 8;

    /// <summary>
    /// Base delay in milliseconds before the first retry.
    /// </summary>
    private const int BaseDelayMs = 100;

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
            {
                if (attempt == MaxRetries)
                    throw;

                await Task.Delay(TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt)), cancellationToken);
            }
        }

        throw new InvalidOperationException("Unreachable");
    }

    /// <inheritdoc/>
    public override int SaveChanges()
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return base.SaveChanges();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
            {
                if (attempt == MaxRetries)
                    throw;

                Thread.Sleep(TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt)));
            }
        }

        throw new InvalidOperationException("Unreachable");
    }

    /// <summary>
    /// Executes a bulk operation within a transaction context with SQLite busy retry.
    /// This method ensures that the database connection is opened and closed properly,
    /// and that the operation is committed if successful.
    /// This is to avoid "Prepare can only be called when the connection is open."
    /// </summary>
    private async Task ExecuteBulkAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
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

                return;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
            {
                if (attempt == MaxRetries)
                    throw;

                await Task.Delay(TimeSpan.FromMilliseconds(BaseDelayMs * Math.Pow(2, attempt)), cancellationToken);
            }
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
