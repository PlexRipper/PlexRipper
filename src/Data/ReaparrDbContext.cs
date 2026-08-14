using System.Data;
using System.Reflection;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;
using TickerQ.EntityFrameworkCore.Configurations;
using TickerQ.Utilities.Entities;

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

    public DbSet<JobTimeTicker> TimeTickers { get; set; }

    public DbSet<JobCronTicker> CronTickers { get; set; }

    public DbSet<CronTickerOccurrenceEntity<JobCronTicker>> CronTickerOccurrences { get; set; }

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

    #region Comparison

    public DbSet<PlexComparisonState> PlexComparisonScopes { get; set; }

    public DbSet<PlexMovieComparison> PlexMovieComparisons { get; set; }

    public DbSet<PlexTvShowComparison> PlexTvShowComparisons { get; set; }

    public DbSet<PlexSeasonComparison> PlexSeasonComparisons { get; set; }

    public DbSet<PlexEpisodeComparison> PlexEpisodeComparisons { get; set; }

    #endregion

    public string DatabaseName { get; }

    public async Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        await ExecuteSerializedWriteAsync(
            async (_, ct) =>
                await ExecuteBulkAsync(
                    () => DbContextBulkExtensions.BulkInsertAsync(this, entities, bulkConfig, cancellationToken: ct),
                    ct
                ),
            cancellationToken
        );

    public async Task BulkUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class =>
        await ExecuteSerializedWriteAsync(
            async (_, ct) =>
                await ExecuteBulkAsync(
                    () => DbContextBulkExtensions.BulkUpdateAsync(this, entities, bulkConfig, cancellationToken: ct),
                    ct
                ),
            cancellationToken
        );

    /// <inheritdoc/>
    public Task<T> ExecuteWithRetryAsync<T>(
        Func<IReaparrDbContext, Task<T>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default
    ) =>
        ((DbContext)this).ExecuteWithRetryAsync(
            ctx => operation((IReaparrDbContext)ctx),
            maxRetries,
            cancellationToken
        );

    public Task<T> ExecuteSerializedWriteAsync<T>(
        Func<IReaparrDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default
    ) => this.ExecuteSerializedWriteAsync(ct => operation(this, ct), 8, cancellationToken);

    public Task ExecuteSerializedWriteAsync(
        Func<IReaparrDbContext, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    ) => this.ExecuteSerializedWriteAsync(ct => operation(this, ct), 8, cancellationToken);

    public Task<Result<T>> ExecuteSerializedTransactionAsync<T>(
        Func<IReaparrDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default
    ) =>
        Result.Try(
            new Func<Task<T>>(async () =>
            {
                T result = default!;
                await this.ExecuteSerializedTransactionAsync(
                    async ct =>
                    {
                        result = await operation(this, ct);
                    },
                    8,
                    cancellationToken
                );
                return result;
            })
        );

    public Task<Result> ExecuteSerializedTransactionAsync(
        Func<IReaparrDbContext, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    ) => Result.Try(() => this.ExecuteSerializedTransactionAsync(ct => operation(this, ct), 8, cancellationToken));

    /// <inheritdoc/>
    public Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default)
    {
        return this.ExecuteSerializedWriteAsync(
            ct1 => Database.ExecuteSqlInterpolatedAsync(sql, ct1),
            8,
            cancellationToken
        );
    }

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

        // Setup TickerQ
        builder.ApplyConfiguration(new TimeTickerConfigurations<JobTimeTicker>());
        builder.ApplyConfiguration(new CronTickerConfigurations<JobCronTicker>());
        builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations<JobCronTicker>());

        // Configurations need to override TickerQ default configurations
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    /// <summary>
    /// Executes a bulk operation with an open database connection.
    /// EFCore.BulkExtensions requires this for SQLite to avoid "Prepare can only be called when the connection is open."
    /// </summary>
    private async Task ExecuteBulkAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var openedHere = conn.State != ConnectionState.Open;
        if (openedHere)
            await Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await operation();
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

    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        this.SaveChangesSerializedAsync(8, cancellationToken);
}
