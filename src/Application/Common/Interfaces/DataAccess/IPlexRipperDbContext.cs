using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
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

        DbSet<PlexTvShow> PlexTvShows { get; set; }

        DbSet<PlexTvShowSeason> PlexTvShowSeason { get; set; }

        DbSet<PlexTvShowEpisode> PlexTvShowEpisodes { get; set; }

        DbSet<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

        DbSet<FileTask> FileTasks { get; set; }

        DbSet<PlexMovieData> PlexMovieData { get; set; }

        DbSet<PlexMovieDataPart> PlexMovieDataParts { get; set; }
        DbSet<PlexTvShowEpisodeData> PlexTvShowEpisodeData { get; set; }
        DbSet<PlexTvShowEpisodeDataPart> PlexTvShowEpisodeDataParts { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        EntityEntry Entry(object entity);
    }
}