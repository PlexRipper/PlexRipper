using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using PlexRipper.Domain;

namespace Data.Contracts;

public interface IPlexRipperDbContext : IDisposable
{
    public string DatabaseName { get; }

    public DbSet<PlexAccount> PlexAccounts { get; }
    public DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; }
    public DbSet<DownloadWorkerLog> DownloadWorkerTasksLogs { get; }
    public DbSet<FolderPath> FolderPaths { get; }

    public DbSet<Notification> Notifications { get; }

    public DbSet<PlexLibrary> PlexLibraries { get; }
    public DbSet<PlexMovie> PlexMovies { get; }
    public DbSet<PlexTvShow> PlexTvShows { get; }
    public DbSet<PlexTvShowSeason> PlexTvShowSeason { get; }
    public DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; }

    public DbSet<PlexServer> PlexServers { get; }
    public DbSet<PlexServerConnection> PlexServerConnections { get; }
    public DbSet<PlexServerStatus> PlexServerStatuses { get; }
    public DbSet<PlexAccountServer> PlexAccountServers { get; }
    public DbSet<PlexAccountLibrary> PlexAccountLibraries { get; }

    public DbSet<DownloadTaskMovie> DownloadTaskMovie { get; }

    public DbSet<DownloadTaskMovieFile> DownloadTaskMovieFile { get; }

    public DbSet<DownloadTaskTvShow> DownloadTaskTvShow { get; }

    public DbSet<DownloadTaskTvShowSeason> DownloadTaskTvShowSeason { get; }

    public DbSet<DownloadTaskTvShowEpisode> DownloadTaskTvShowEpisode { get; }

    public DbSet<DownloadTaskTvShowEpisodeFile> DownloadTaskTvShowEpisodeFile { get; }

    public DbSet<PlexRole> PlexRoles { get; set; }

    public DbSet<PlexGenre> PlexGenres { get; set; }

    public DbSet<PlexCountry> PlexCountries { get; set; }

    public DbSet<PlexMovieRoles> PlexMovieRoles { get; set; }

    public DbSet<PlexMovieCountries> PlexMovieCountries { get; set; }

    public DbSet<PlexMovieGenres> PlexMovieGenres { get; set; }

    public DbSet<PlexTvShowRoles> PlexTvShowRoles { get; set; }

    public DbSet<PlexTvShowGenres> PlexTvShowGenres { get; set; }

    public DbSet<PlexTvShowCountries> PlexTvShowCountries { get; set; }

    public EntityEntry Entry(object entity);

    public int SaveChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

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

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
