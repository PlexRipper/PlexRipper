using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TickerQ.Utilities.Entities;

namespace Reaparr.Data.Contracts;

public interface IReaparrDbContext : IDisposable
{
    #region Properties

    #region Tables

    DbSet<PlexAccount> PlexAccounts { get; }

    DbSet<DownloadTaskMovieFileLog> DownloadTaskMovieFileLogs { get; }

    DbSet<DownloadTaskTvShowEpisodeFileLog> DownloadTaskTvShowEpisodeFileLogs { get; }

    DbSet<FolderPath> FolderPaths { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<PlexLibrary> PlexLibraries { get; }

    DbSet<PlexLibraryAccessHistoryEvent> PlexLibraryAccessHistoryEvents { get; }

    #region PlexMedia

    DbSet<PlexActor> PlexActors { get; }

    DbSet<PlexGenre> PlexGenres { get; }

    DbSet<PlexCountry> PlexCountries { get; }

    #endregion

    #region PlexMovie

    DbSet<PlexMovie> PlexMovies { get; }

    DbSet<PlexMovieMediaData> PlexMovieData { get; }

    #endregion

    #region PlexTvShow

    DbSet<PlexTvShow> PlexTvShows { get; }

    DbSet<PlexTvShowMediaQuality> PlexTvShowMediaQualities { get; }

    DbSet<PlexTvShowSeason> PlexTvShowSeason { get; }

    DbSet<PlexTvShowSeasonMediaQuality> PlexTvShowSeasonMediaQualities { get; }

    DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; }

    DbSet<PlexTvShowEpisodeMediaData> PlexTvShowEpisodeData { get; }

    #endregion

    #region PlexServers

    DbSet<PlexServer> PlexServers { get; }

    DbSet<PlexServerConnection> PlexServerConnections { get; }

    DbSet<PlexServerStatus> PlexServerStatuses { get; }

    DbSet<LibrarySyncJobQueue> LibrarySyncJobQueues { get; }

    #endregion

    #region BackgroundJobs

    DbSet<JobTimeTicker> TimeTickers { get; }

    DbSet<JobCronTicker> CronTickers { get; }

    DbSet<CronTickerOccurrenceEntity<JobCronTicker>> CronTickerOccurrences { get; }

    #endregion

    #endregion

    #region DownloadTasks

    DbSet<DownloadTaskMovie> DownloadTaskMovie { get; }

    DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; }

    DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; }

    DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; }

    DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; }

    DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; }

    #endregion

    #region JoinTables

    DbSet<PlexAccountServer> PlexAccountServers { get; }

    DbSet<PlexAccountLibrary> PlexAccountLibraries { get; }

    DbSet<PlexLibraryActors> PlexLibraryActors { get; }

    DbSet<PlexLibraryCountries> PlexLibraryCountries { get; }

    DbSet<PlexLibraryGenres> PlexLibraryGenres { get; }

    DbSet<PlexMovieActors> PlexMovieActors { get; }

    DbSet<PlexMovieCountries> PlexMovieCountries { get; }

    DbSet<PlexMovieGenres> PlexMovieGenres { get; }

    DbSet<PlexTvShowActors> PlexTvShowActors { get; }

    DbSet<PlexTvShowGenres> PlexTvShowGenres { get; }

    DbSet<PlexTvShowCountries> PlexTvShowCountries { get; }

    #endregion

    #region Comparison

    DbSet<PlexComparisonState> PlexComparisonScopes { get; }

    DbSet<PlexMovieComparison> PlexMovieComparisons { get; }

    DbSet<PlexTvShowComparison> PlexTvShowComparisons { get; }

    DbSet<PlexSeasonComparison> PlexSeasonComparisons { get; }

    DbSet<PlexEpisodeComparison> PlexEpisodeComparisons { get; }

    #endregion

    string DatabaseName { get; }

    #endregion Properties

    Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task BulkUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Executes an operation with automatic retry on <c>SQLITE_BUSY</c> and
    /// <c>SQLITE_BUSY_SNAPSHOT</c> errors.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxRetries">
    /// The maximum number of retry attempts. Each retry waits using exponential backoff
    /// with jitter starting at 100 ms.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// <para>
    /// Two classes of busy error are handled:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <b>SQLITE_BUSY / SQLITE_BUSY_RECOVERY / SQLITE_BUSY_TIMEOUT</b> — another
    ///     connection holds a lock. The operation is retried after a backoff delay.
    ///   </item>
    ///   <item>
    ///     <b>SQLITE_BUSY_SNAPSHOT</b> — the connection's read snapshot became stale
    ///     after another writer committed. The entire operation is restarted so that it
    ///     can acquire a fresh snapshot. Any data read in the failed attempt must be
    ///     re-queried inside the operation lambda.
    ///   </item>
    /// </list>
    /// <para>
    ///   <b>SQLITE_LOCKED</b> (same-connection conflict) is not retried and propagates
    ///   immediately, as it indicates an application-level bug.
    /// </para>
    /// </remarks>
    Task<T> ExecuteWithRetryAsync<T>(
        Func<IReaparrDbContext, Task<T>> operation,
        int maxRetries = 3,
        CancellationToken cancellationToken = default
    );

    Task<Result<T>> ExecuteSerializedTransactionAsync<T>(
        Func<IReaparrDbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default
    );

    Task<Result> ExecuteSerializedTransactionAsync(
        Func<IReaparrDbContext, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    );

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void ClearChangeTracker();

    /// <summary>
    /// Executes the given interpolated SQL against the database and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The interpolated SQL command with parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default);
}
