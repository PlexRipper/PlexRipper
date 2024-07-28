using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task UpdatePlexLibraryById(
        this IPlexRipperDbContext dbContext,
        PlexLibrary plexLibrary,
        CancellationToken cancellationToken = default
    )
    {
        var plexLibraryDb = await dbContext
            .PlexLibraries.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == plexLibrary.Id, cancellationToken);

        if (plexLibraryDb is null)
        {
            _log.ErrorLine($"PlexLibrary with Id {plexLibrary.Id} not found in the database.");
            return;
        }

        dbContext.Entry(plexLibraryDb).CurrentValues.SetValues(plexLibrary);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
