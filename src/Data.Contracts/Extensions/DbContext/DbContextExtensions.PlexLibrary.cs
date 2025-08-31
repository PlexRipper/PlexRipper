using Microsoft.EntityFrameworkCore;
using Reaparr.Domain;
using Reaparr.Logging;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexLibraryNameById(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        CancellationToken cancellationToken = default
    )
    {
        var plexLibraryName = await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);
        return plexLibraryName ?? "Library Name Not Found";
    }

    public static async Task<int> GetPlexServerIdFromPlexLibraryId(this IReaparrDbContext dbContext, int plexLibraryId)
    {
        return await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.PlexServerId)
            .FirstOrDefaultAsync(CancellationToken.None);
    }

    public static async Task UpdatePlexLibraryById(
        this IReaparrDbContext dbContext,
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var plexLibraryDb = await dbContext
            .PlexLibraries.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id, cancellationToken);

        if (plexLibraryDb is null)
        {
            _log.Here().Error($"PlexLibrary with Id {plexLibrary.Id} not found in the database");
            return;
        }

        dbContext.Entry(plexLibraryDb).CurrentValues.SetValues(plexLibrary);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
