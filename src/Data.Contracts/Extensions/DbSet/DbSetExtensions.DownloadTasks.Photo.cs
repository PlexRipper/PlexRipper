namespace Reaparr.Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<DownloadTaskPhotoFolder> IncludeAll(this IQueryable<DownloadTaskPhotoFolder> tasks) =>
        tasks
            .IgnoreQueryFilters(["IsEnabled"])
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
                .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children.OrderBy(y => y.CreatedAt))
                .ThenInclude(x => x.PlexLibrary)
            .Include(x => x.Children)
                .ThenInclude(x => x.Files.OrderBy(y => y.CreatedAt))
                    .ThenInclude(x => x.PlexServer)
            .Include(x => x.Children)
                .ThenInclude(x => x.Files.OrderBy(y => y.CreatedAt))
                    .ThenInclude(x => x.PlexLibrary)
            .Include(x => x.Files.OrderBy(y => y.CreatedAt))
                .ThenInclude(x => x.PlexServer)
            .Include(x => x.Files.OrderBy(y => y.CreatedAt))
                .ThenInclude(x => x.PlexLibrary);
}
