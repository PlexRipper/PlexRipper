using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

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

    string DatabaseName { get; }

    #endregion Properties

    Task BulkReadAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

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

    Task BulkInsertOrUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        Action<decimal>? progress = null,
        Type? type = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    EntityEntry Entry(object entity);

    Task<int> SaveChangesNewAsync(CancellationToken cancellationToken = default);

    void ClearChangeTracker();
}
