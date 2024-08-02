using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<DownloadTaskMovie> IncludeAll(this IQueryable<DownloadTaskMovie> downloadTasks) =>
        downloadTasks
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary);

    public static IQueryable<DownloadTaskTvShow> IncludeAll(this IQueryable<DownloadTaskTvShow> downloadTasks) =>
        downloadTasks
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            // Include Seasons
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary)
            // Include Episodes
            .Include(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary)
            // Include Files
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary);

    public static IQueryable<DownloadTaskTvShowSeason> IncludeAll(
        this IQueryable<DownloadTaskTvShowSeason> downloadTasks
    ) =>
        downloadTasks
            // Include Episodes
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary)
            // Include Files
            .Include(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children)
            .ThenInclude(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary);

    public static IQueryable<DownloadTaskTvShowEpisode> IncludeAll(
        this IQueryable<DownloadTaskTvShowEpisode> downloadTasks
    ) =>
        downloadTasks
            // Include Files
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
            .ThenInclude(x => x.PlexLibrary);
}
