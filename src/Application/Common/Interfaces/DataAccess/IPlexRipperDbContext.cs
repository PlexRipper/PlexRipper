using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.JoinTables;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.DataAccess
{
    public interface IPlexRipperDbContext
    {
        DbSet<DownloadTask> DownloadTasks { get; set; }
        DbSet<FolderPath> FolderPaths { get; set; }
        DbSet<PlexGenre> PlexGenres { get; set; }
        DbSet<PlexAccount> PlexAccounts { get; set; }
        DbSet<PlexLibrary> PlexLibraries { get; set; }
        DbSet<PlexMovie> PlexMovies { get; set; }
        DbSet<PlexRole> PlexRoles { get; set; }
        DbSet<PlexServer> PlexServers { get; set; }
        DbSet<PlexServerStatus> PlexServerStatuses { get; set; }
        DbSet<PlexAccountServer> PlexAccountServers { get; set; }
        DbSet<PlexMovieGenre> PlexMovieGenres { get; set; }
        DbSet<PlexMovieRole> PlexMovieRoles { get; set; }
        DbSet<PlexAccountLibrary> PlexAccountLibraries { get; set; }
        DbSet<PlexSerie> PlexTvShows { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        EntityEntry Entry(object entity);
    }
}
