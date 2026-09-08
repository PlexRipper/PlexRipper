using Serilog;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<FolderPath> GetDownloadFolder(
        this IReaparrDbContext dbContext,
        IntegrationIdentity? integration = null
    )
    {
        int? downloadFolderId = null;
        if (integration is not null)
        {
            downloadFolderId = integration.Type switch
            {
                IntegrationType.Sonarr => await dbContext
                    .SonarrIntegrations.Where(x => x.Id == integration.Id)
                    .Select(x => x.DownloadFolderId)
                    .SingleOrDefaultAsync(CancellationToken.None),
                IntegrationType.Radarr => await dbContext
                    .RadarrIntegrations.Where(x => x.Id == integration.Id)
                    .Select(x => x.DownloadFolderId)
                    .SingleOrDefaultAsync(CancellationToken.None),
                _ => null,
            };
        }

        if (downloadFolderId is not null)
        {
            var downloadFolder = await dbContext.FolderPaths.FirstOrDefaultAsync(
                x => x.Id == downloadFolderId && x.FolderType == FolderType.DownloadFolder,
                CancellationToken.None
            );
            if (downloadFolder is not null)
                return downloadFolder;
        }

        // This is the default download folder, which always exists in the database
        return (
            await dbContext.FolderPaths.GetAsync(
                PlexMediaType.None.ToDefaultDestinationFolderId(),
                CancellationToken.None
            )
        )!;
    }

    public static async Task<FolderPath?> GetDestinationFolder(this IReaparrDbContext dbContext, int plexLibraryId)
    {
        var plexLibrary = await dbContext
            .PlexLibraries.Include(x => x.DefaultDestination)
            .FirstOrDefaultAsync(x => x.Id == plexLibraryId, CancellationToken.None);

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
    /// <param name="dbContext"> The <see cref="IReaparrDbContext"/> to use.</param>
    public static async Task<FolderPath> GetDefaultDestinationFolderPath(
        this IReaparrDbContext dbContext,
        PlexMediaType mediaType
    )
    {
        var id = mediaType.ToDefaultDestinationFolderId();

        // Default folder paths always exist
        return (await dbContext.FolderPaths.GetAsync(id, CancellationToken.None))!;
    }
}
