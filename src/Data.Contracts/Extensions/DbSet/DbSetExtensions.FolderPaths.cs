using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    /// <summary>
    /// Get the current download <see cref="FolderPath"/>.
    /// </summary>
    /// <returns>The <see cref="FolderPath"/> of the download folder.</returns>
    public static async Task<FolderPath?> GetDownloadFolderAsync(this IQueryable<FolderPath> dbSet, CancellationToken token = default)
    {
        return await dbSet.FirstOrDefaultAsync(x => x.Id == 1, token);
    }

    /// <summary>
    /// Gets a dictionary of the default destination <see cref="FolderPath"/> for each <see cref="PlexMediaType"/>.
    /// </summary>
    /// <param name="dbSet">The <see cref="DbSet{FolderPath}"/> to extend.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    public static async Task<Dictionary<PlexMediaType, FolderPath>> GetDefaultDestinationFolderDictionary(
        this IQueryable<FolderPath> dbSet,
        CancellationToken token = default)
    {
        var folderPaths = await dbSet.ToListAsync(token);

        return new Dictionary<PlexMediaType, FolderPath>
        {
            { PlexMediaType.Movie, folderPaths.FirstOrDefault(x => x.Id == 2)! },
            { PlexMediaType.TvShow, folderPaths.FirstOrDefault(x => x.Id == 3)! },
            { PlexMediaType.Season, folderPaths.FirstOrDefault(x => x.Id == 3)! },
            { PlexMediaType.Episode, folderPaths.FirstOrDefault(x => x.Id == 3)! },
            { PlexMediaType.Music, folderPaths.FirstOrDefault(x => x.Id == 4)! },
            { PlexMediaType.Album, folderPaths.FirstOrDefault(x => x.Id == 4)! },
            { PlexMediaType.Song, folderPaths.FirstOrDefault(x => x.Id == 4)! },
            { PlexMediaType.Photos, folderPaths.FirstOrDefault(x => x.Id == 5)! },
            { PlexMediaType.OtherVideos, folderPaths.FirstOrDefault(x => x.Id == 6)! },
            { PlexMediaType.Games, folderPaths.FirstOrDefault(x => x.Id == 7)! },
            { PlexMediaType.None, folderPaths.FirstOrDefault(x => x.Id == 1)! },
            { PlexMediaType.Unknown, folderPaths.FirstOrDefault(x => x.Id == 1)! },
        };
    }
}