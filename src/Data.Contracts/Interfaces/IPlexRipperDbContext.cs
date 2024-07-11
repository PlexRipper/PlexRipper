using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PlexRipper.Domain;

namespace Data.Contracts;

public interface IPlexRipperDbContext
{
    public DbSet<PlexAccount> PlexAccounts { get; set; }
    public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }
    public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; set; }
    public DbSet<FolderPath> FolderPaths { get; set; }

    public DbSet<FileTask> FileTasks { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public DbSet<PlexGenre> PlexGenres { get; set; }
    public DbSet<PlexLibrary> PlexLibraries { get; set; }
    public DbSet<PlexMovie> PlexMovies { get; set; }
    public DbSet<PlexTvShow> PlexTvShows { get; set; }
    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }
    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

    public DbSet<PlexRole> PlexRoles { get; set; }
    public DbSet<PlexServer> PlexServers { get; set; }
    public DbSet<PlexServerConnection> PlexServerConnections { get; set; }
    public DbSet<PlexServerStatus> PlexServerStatuses { get; set; }
    public DbSet<PlexAccountServer> PlexAccountServers { get; set; }
    public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; set; }
    public DbSet<PlexMovieGenre> PlexMovieGenres { get; set; }
    public DbSet<PlexMovieRole> PlexMovieRoles { get; set; }
    public string DatabaseName { get; set; }
    public string DatabasePath { get; set; }
    public string ConfigDirectory { get; set; }

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; set; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; set; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; set; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; set; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; set; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; set; }

    public EntityEntry Entry(object entity);

    /// <summary>
    /// Determines if this <see cref="PlexRipperDbContext"/> has been setup already during integration or unit testing.
    /// </summary>
    public bool HasBeenSetup { get; set; }

    public int SaveChanges();
    public int SaveChanges(bool acceptAllChangesOnSuccess);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);

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
}
