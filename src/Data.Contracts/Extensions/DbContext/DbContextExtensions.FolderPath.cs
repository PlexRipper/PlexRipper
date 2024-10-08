using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;
using Serilog;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<FolderPath> GetDownloadFolder(this IPlexRipperDbContext dbContext)
    {
        // This is the default download folder, which always exists in the database
        return (await dbContext.FolderPaths.FirstOrDefaultAsync(x => x.FolderType == FolderType.DownloadFolder))!;
    }

    public static async Task<FolderPath?> GetDestinationFolder(this IPlexRipperDbContext dbContext, int plexLibraryId)
    {
        var plexLibrary = await dbContext
            .PlexLibraries.Include(x => x.DefaultDestination)
            .FirstOrDefaultAsync(x => x.Id == plexLibraryId);

        if (plexLibrary is null)
        {
            Log.Error("PlexLibrary with Id {PlexLibraryId} not found", plexLibraryId);
            return null;
        }

        if (plexLibrary.DefaultDestination is null)
            return await dbContext.GetDefaultDestinationFolderPath(plexLibrary.Type);

        return plexLibrary.DefaultDestination;
    }

    /// <summary>
    /// Gets a dictionary of the default destination <see cref="FolderPath"/> for each <see cref="PlexMediaType"/>.
    /// </summary>
    /// <param name="mediaType"> The <see cref="PlexMediaType"/> to get the default destination <see cref="FolderPath"/> for.</param>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext"/> to use.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to use.</param>
    /// <returns></returns>
    public static async Task<FolderPath> GetDefaultDestinationFolderPath(
        this IPlexRipperDbContext dbContext,
        PlexMediaType mediaType,
        CancellationToken token = default
    )
    {
        switch (mediaType)
        {
            case PlexMediaType.Movie:
                return (await dbContext.FolderPaths.GetAsync(2, token))!;
            case PlexMediaType.TvShow:
            case PlexMediaType.Season:
            case PlexMediaType.Episode:
                return (await dbContext.FolderPaths.GetAsync(3, token))!;
            case PlexMediaType.Music:
            case PlexMediaType.Album:
            case PlexMediaType.Song:
                return (await dbContext.FolderPaths.GetAsync(4, token))!;
            case PlexMediaType.Photos:
                return (await dbContext.FolderPaths.GetAsync(5, token))!;
            case PlexMediaType.OtherVideos:
                return (await dbContext.FolderPaths.GetAsync(6, token))!;
            case PlexMediaType.Games:
                return (await dbContext.FolderPaths.GetAsync(7, token))!;
            case PlexMediaType.None:
            case PlexMediaType.Unknown:
            default:
                Log.Error(
                    "Unknown PlexMediaType {PlexMediaType} that could not be used to determine a default path, defaulting to the DownloadPath",
                    mediaType
                );
                return (await dbContext.FolderPaths.GetAsync(1, token))!;
        }
    }
}