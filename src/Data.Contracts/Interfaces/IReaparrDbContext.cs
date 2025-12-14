using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public interface IReaparrDbContext : IDisposable
{
    #region Properties

    #region Tables

    public DbSet<PlexAccount> PlexAccounts { get; }

    public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; }

    public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; }

    public DbSet<FolderPath> FolderPaths { get; }

    public DbSet<Notification> Notifications { get; }

    public DbSet<PlexLibrary> PlexLibraries { get; }

    #region PlexMedia

    public DbSet<PlexActor> PlexActors { get; }

    public DbSet<PlexGenre> PlexGenres { get; }

    public DbSet<PlexCountry> PlexCountries { get; }

    #endregion

    #region PlexMovie

    public DbSet<PlexMovie> PlexMovies { get; }

    public DbSet<PlexMovieMediaData> PlexMovieData { get; }

    public DbSet<PlexMovieMediaDataPart> PlexMovieDataParts { get; }

    public DbSet<PlexMovieMediaDataStream> PlexMovieDataStreams { get; }

    #endregion

    #region PlexTvShow

    public DbSet<PlexTvShow> PlexTvShows { get; }

    public DbSet<PlexTvShowMediaQuality> PlexTvShowMediaQualities { get; }

    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; }

    public DbSet<PlexTvShowSeasonMediaQuality> PlexTvShowSeasonMediaQualities { get; }

    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; }

    public DbSet<PlexTvShowEpisodeMediaData> PlexTvShowEpisodeData { get; }

    public DbSet<PlexTvShowEpisodeMediaDataPart> PlexTvShowEpisodeDataParts { get; }

    public DbSet<PlexTvShowEpisodeMediaDataStream> PlexTvShowEpisodeDataStreams { get; }

    #endregion

    #region PlexServers

    public DbSet<PlexServer> PlexServers { get; }

    public DbSet<PlexServerConnection> PlexServerConnections { get; }

    public DbSet<PlexServerStatus> PlexServerStatuses { get; }

    public DbSet<LibrarySyncJobQueue> LibrarySyncQueues { get; }

    #endregion

    #endregion

    #region DownloadTasks

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; }

    #endregion

    #region JoinTables

    public DbSet<PlexAccountServer> PlexAccountServers { get; }

    public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; }

    public DbSet<PlexLibraryActors> PlexLibraryActors { get; }

    public DbSet<PlexLibraryCountries> PlexLibraryCountries { get; }

    public DbSet<PlexLibraryGenres> PlexLibraryGenres { get; }

    public DbSet<PlexMovieActors> PlexMovieActors { get; }

    public DbSet<PlexMovieCountries> PlexMovieCountries { get; }

    public DbSet<PlexMovieGenres> PlexMovieGenres { get; }

    public DbSet<PlexTvShowActors> PlexTvShowActors { get; }

    public DbSet<PlexTvShowGenres> PlexTvShowGenres { get; }

    public DbSet<PlexTvShowCountries> PlexTvShowCountries { get; }

    #endregion

    public string DatabaseName { get; }

    #endregion Properties

    public Task BulkReadAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    public Task BulkInsertAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    public Task BulkUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    public Task BulkInsertOrUpdateAsync<T>(
        IList<T> entities,
        BulkConfig? bulkConfig = null,
        Action<decimal>? progress = null,
        Type? type = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    public EntityEntry Entry(object entity);

    public int SaveChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);
}
