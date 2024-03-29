using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<FolderPath?> GetDestinationFolder(this IPlexRipperDbContext dbContext, int plexLibraryId)
    {
        return await dbContext.PlexLibraries.Include(x => x.DefaultDestination)
            .Select(x => x.DefaultDestination)
            .FirstOrDefaultAsync(x => x.Id == plexLibraryId);
    }
}